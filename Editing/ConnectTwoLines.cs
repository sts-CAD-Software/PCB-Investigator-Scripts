//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 07.03.2016
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Close gaps in line connections by moving both lines (two selected lines) to the intersecting point.
// First you select the two relevant lines, than start the script and you will be modified to end on the same point.
//-----------------------------------------------------------------------------------
// GUID ConnectLines_635900140114543121
// EnableOn JobOpen
// Category Edit

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

                if (obj1.Type != IObjectType.Line)
                    return;
                if (obj2.Type != IObjectType.Line)
                    return;

                ILineSpecificsD obS1 = (ILineSpecificsD)obj1.GetSpecificsD();
                ILineSpecificsD obS2 = (ILineSpecificsD)obj2.GetSpecificsD();

                PCBI.MathUtils.PointD crossingPoint = PCBI.MathUtils.IMath.CrossingPoint(obS1.Start, obS1.End, obS2.Start, obS2.End, false);

                if (PCBI.MathUtils.PointD.InfPoint == crossingPoint) return; //parallel lines do not work

                #region set ends of lines to the crossing Point
                if (PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS1.End) < PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS1.Start))
                    obS1.End = crossingPoint;
                else
                    obS1.Start = crossingPoint;

                obj1.SetSpecifics(obS1);

                if (PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS2.End) < PCBI.MathUtils.IMath.DistancePointToPoint(crossingPoint, obS2.Start))
                    obS2.End = crossingPoint;
                else
                    obS2.Start = crossingPoint;

                obj2.SetSpecifics(obS2); 
                #endregion
            }

            parent.UpdateView();
        }



    }
}
