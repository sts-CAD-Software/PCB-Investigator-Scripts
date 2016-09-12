// PCB-Investigator Automation Script
// Created on 2014-11-17
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Select a group of lines and arcs to find openings at the end of each object.
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
            double tolerance = 1;

            List<IODBObject> selection = step.GetSelectedElements();

            foreach (IODBObject selObj in selection)
            {
                IODBLayer layer = (IODBLayer)step.GetLayer(selObj.GetParentLayerName());

                if (selObj.Type == IObjectType.Line)
                {
                    ILineSpecificsD line = (ILineSpecificsD)selObj.GetSpecificsD();
                    List<IObject> startPos = layer.GetAllObjectsOnPosition(line.Start.ToPointF());
                    bool setColor = true;
                    setColor = CheckConnection(tolerance, selObj, line.Start, startPos, setColor);
                    if (setColor)
                    {
                        List<IObject> endPos = layer.GetAllObjectsOnPosition(line.End.ToPointF());
                        setColor = CheckConnection(tolerance, selObj, line.End, endPos, setColor);
                    }
                    if (setColor)
                    {
                        selObj.ObjectColorTemporary (Color.Aquamarine);  
                    }
                }
                else if (selObj.Type == IObjectType.Arc)
                {
                    IArcSpecificsD arc = (IArcSpecificsD)selObj.GetSpecificsD();
                    
                    List<IObject> startPos = layer.GetAllObjectsOnPosition(arc.Start.ToPointF());
                    bool setColor = true;
                    setColor = CheckConnection(tolerance, selObj, arc.Start, startPos, setColor);
                    if (setColor)
                    {
                        List<IObject> endPos = layer.GetAllObjectsOnPosition(arc.End.ToPointF());
                        setColor = CheckConnection(tolerance, selObj, arc.End, endPos, setColor);
                    }
                    if (setColor)
                    {
                        selObj.ObjectColorTemporary(Color.Aquamarine);
                    }
                }
            }

            step.ClearSelection();
        }

        private static bool CheckConnection(double tolerance, IODBObject selObj, PointD lineStart, List<IObject> relevantObjects, bool setColor)
        {
            foreach (IODBObject relObject in relevantObjects)
            {
                if (relObject == selObj) continue;

                if (relObject.Type == IObjectType.Line)
                {
                    ILineSpecificsD lineRel = (ILineSpecificsD)relObject.GetSpecificsD();

                    if (IMath.DistancePointToPoint(lineRel.End, lineStart) < tolerance || IMath.DistancePointToPoint(lineRel.Start, lineStart) < tolerance)
                    { setColor = false; break; }
                }
                else if (relObject.Type == IObjectType.Arc)
                {
                    IArcSpecificsD arcRel = (IArcSpecificsD)relObject.GetSpecificsD();

                    if (IMath.DistancePointToPoint(arcRel.End, lineStart) < tolerance || IMath.DistancePointToPoint(arcRel.Start, lineStart) < tolerance)
                    { setColor = false; break; }
                }
            }
            return setColor;
        }
  
    }
}
