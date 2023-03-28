//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 16.02.2016
// Autor Fabio.Gruber
// 
// Cut all lines on crossing points, lines on lines will be cutted on start and end of shorter line.
// This is helpful for creating clean outlines, to remove double elements in recesses.
//-----------------------------------------------------------------------------------
// GUID CutLines_635911405568636453
//
// The script traverses all selected lines, searches for intersection points and cuts the lines accordingly to create new lines at the intersection points.
// The script also contains functions to abort the cutting process and to define a maximum length for the generated lines. 
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

	    public static void Execute(IPCBIWindow parent)
        {
            //check all selected lines, if lines on lines cut them

            IStep step = parent.GetCurrentStep();
            IFilter filter = new IFilter(parent);
            if (step == null) return;

            List<IODBObject> selectedElements = step.GetSelectedElements();

            PCB_Investigator.PCBIWindows.PCBIWorkingDialog working = new PCB_Investigator.PCBIWindows.PCBIWorkingDialog();
            StopCutting = false;
            working.SetStatusText("Working on splitting lines...");
            working.CancelPressed += Working_CancelPressed;
            working.CanCancel(true);
            working.SetAnimationStatus(false);

            working.ShowWorkingDlgAsThread();

            for (int i = 0; i < selectedElements.Count; i++)
            {
                if (StopCutting) break;

                working.SetStatusPercent(i *100 / selectedElements.Count);

                IODBObject evtlLine = selectedElements[i];
                if (evtlLine.Type != IObjectType.Line)
                {
                    continue;
                }

                ILineSpecificsD line = (ILineSpecificsD)evtlLine.GetSpecificsD();
                for (int j = i + 1; j < selectedElements.Count; j++)
                {
                    IODBObject evtlLine2 = selectedElements[j];
                    if (evtlLine2.Type != IObjectType.Line || evtlLine == evtlLine2)
                    {
                        continue;
                    }
                    ILineSpecificsD line2 = (ILineSpecificsD)evtlLine2.GetSpecificsD();

                    PointD crossingP = IMath.CrossingPoint(line.Start, line.End, line2.Start, line2.End, true);
                    if (PointD.InfPoint != crossingP)
                    {
                        CreateSplittedLines(step, filter, evtlLine2, line2, crossingP, ref selectedElements);
                        CreateSplittedLines(step, filter, evtlLine, line, crossingP, ref selectedElements);
                        line = (ILineSpecificsD)evtlLine.GetSpecificsD(); //changed, get it new
                    }
                }

            }
            working.DoClose();

        }
        static bool StopCutting = false; //important to cancel
        static double errorLevel = 0.001; //max line lenght
        private static void Working_CancelPressed()
        {
            StopCutting = true;
        }

        private static void CreateSplittedLines(IStep step, IFilter filter, IODBObject evtlLine2, ILineSpecificsD line2, PointD pDest, ref List<IODBObject> selectedElements)
        {
            PointD lastEnd = line2.End;
            line2.End = pDest;
            if (Math.Abs( IMath.DistancePointToPoint(line2.End, line2.Start)) < errorLevel)
            {
                line2.End = lastEnd;
                line2.Start = pDest;
                if (Math.Abs(IMath.DistancePointToPoint(line2.End, line2.Start)) > errorLevel)
                    evtlLine2.SetSpecifics(line2);
                else
                    Debug.WriteLine("Line with length 0!");
            }
            else
            {
                evtlLine2.SetSpecifics(line2);

                ILayer parentLayer = step.GetLayer(evtlLine2.GetParentLayerName());
                if (parentLayer != null)
                {
                    IODBObject line2Part2 = filter.CreateLine((IODBLayer)parentLayer);
                    line2.End = lastEnd;
                    line2.Start = pDest;
                    line2Part2.SetSpecifics(line2);
                    selectedElements.Add(line2Part2);
                }
            }
        }
		
    }
}