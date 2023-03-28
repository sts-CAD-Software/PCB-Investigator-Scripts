//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 18.04.2016
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Close gaps in line connections by moving both lines (two selected lines) to the intersecting point.
// First you select the two relevant lines, than start the script and you will be modified to end on the same point.
// Update: Arcs added with extra cases.
//-----------------------------------------------------------------------------------
// GUID ConnectLines_635900140114543121
// EnableOn JobOpen
// Category Edit
//
// This script allows to connect two selected lines in a PCB design software by merging them at the point where they cross.
// If the two selected elements are lines, they will be merged directly at the intersection.
// If they contain arcs, special cases are handled to ensure that they are correctly aligned at the intersection.
// The script is optimized to work only with two selected elements, each of which can be lines or arcs. 
//-----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using PCBI.Plugin;
using PCBI.Plugin.Interfaces;
using System.Windows.Forms;
using System.Drawing;
using PCBI.Automation;
using System.IO;
using System.Drawing.Drawing2D;
using PCBI.MathUtils;

namespace PCBIScript
{
    public class PScript : IPCBIScript
    {
        public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            IStep step = parent.GetCurrentStep();
            if (step == null) return;

            if (step.GetSelectedElementsCount() == 2) //this script is optimiezd for two line elements
            {
                List<IODBObject> selectedElements = step.GetSelectedElements();

                IODBObject obj1 = selectedElements[0];
                IODBObject obj2 = selectedElements[1];

                bool firstArc = false;
                bool secondArc = false;
                if (obj1.Type == IObjectType.Arc)
                { firstArc = true; }
                if (obj2.Type == IObjectType.Arc)
                { secondArc = true; }

                if (!firstArc && obj1.Type != IObjectType.Line)
                {

                    return;
                }
                else if (!secondArc && obj2.Type != IObjectType.Line)
                {
                    return;

                }
                ILineSpecificsD obS1;
                ILineSpecificsD obS2;
                if (firstArc)
                {
                    obS1 = new ILineSpecificsD();

                    IArcSpecificsD arcS1 = (IArcSpecificsD)obj1.GetSpecificsD();
                    obS1.Start = arcS1.Start;
                    obS1.End = arcS1.End;
                }
                else
                {
                    obS1 = (ILineSpecificsD)obj1.GetSpecificsD();
                }
                if (secondArc)
                {
                    obS2 = new ILineSpecificsD();

                    IArcSpecificsD arcS2 = (IArcSpecificsD)obj2.GetSpecificsD();
                    obS2.Start = arcS2.Start;
                    obS2.End = arcS2.End;
                }
                else
                {
                    obS2 = (ILineSpecificsD)obj2.GetSpecificsD();
                }

                //make simple check for crossing point, this is nearly correct for arc endings
                PCBI.MathUtils.PointD crossingPoint = PCBI.MathUtils.IMath.CrossingPoint(obS1.Start, obS1.End, obS2.Start, obS2.End, false);

                if (PCBI.MathUtils.PointD.InfPoint == crossingPoint) return; //parallel lines do not work

                #region set ends of lines to the crossing Point

                if (!firstArc)
                {
                    if (PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS1.End) < PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS1.Start))
                        obS1.End = crossingPoint;
                    else
                        obS1.Start = crossingPoint;

                    obj1.SetSpecifics(obS1);
                }
                else
                {
                    //special case for arc
                    IArcSpecificsD arcS1 = (IArcSpecificsD)obj1.GetSpecificsD();
                    if (PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS1.End) < PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS1.Start))
                        arcS1.End = crossingPoint;
                    else
                        arcS1.Start = crossingPoint;

                    obj1.SetSpecifics(arcS1);
                    obj1.UpdateInternal();
                }

                if (!secondArc)
                {
                    if (PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS2.End) < PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS2.Start))
                        obS2.End = crossingPoint;
                    else
                        obS2.Start = crossingPoint;

                    obj2.SetSpecifics(obS2);
                }
                else
                {
                    //special case for arc
                    IArcSpecificsD arcS2 = (IArcSpecificsD)obj2.GetSpecificsD();
                    if (PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, arcS2.End) < PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, arcS2.Start))
                        arcS2.End = crossingPoint;
                    else
                        arcS2.Start = crossingPoint;

                    obj2.SetSpecifics(arcS2);
                    obj2.UpdateInternal();
                }
                #endregion
            }

            parent.UpdateView();
        }
    }
}
