//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-11-17
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Change Endpoint of object to close gap between two selected objects.
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

            List<IODBObject> selection = step.GetSelectedElements();

            if (selection.Count == 2)
            {
                IObjectSpecificsD obj = selection[0].GetSpecificsD();

                if (obj is ILineSpecificsD)
                {
                    if (selection[1].Type == IObjectType.Line) //beide lines
                    {
                        #region line to line
                        ILineSpecificsD obj2 = (ILineSpecificsD)selection[1].GetSpecificsD();
                        double distStart_Start = IMath.DistancePointToPoint(((ILineSpecificsD)obj).Start, obj2.Start);
                        double distStart_End = IMath.DistancePointToPoint(((ILineSpecificsD)obj).Start, obj2.End);
                        double distEnd_Start = IMath.DistancePointToPoint(((ILineSpecificsD)obj).End, obj2.Start);
                        double distEnd_End = IMath.DistancePointToPoint(((ILineSpecificsD)obj).End, obj2.End);

                        Combination comb = CheckShortestDist(distStart_Start, distStart_End, distEnd_Start, distEnd_End);

                        if (comb == Combination.START_START)
                        {
                            //start of both
                            obj2.Start = ((ILineSpecificsD)obj).Start;
                        }
                        else if (comb == Combination.START_END)
                        {
                            obj2.End = ((ILineSpecificsD)obj).Start;
                        }
                        else if (comb == Combination.END_START)
                        {
                            obj2.Start = ((ILineSpecificsD)obj).End;
                        }
                        else
                        {
                            obj2.End = ((ILineSpecificsD)obj).End;
                        }
                        selection[1].SetSpecifics(obj2);

                        #endregion
                    }
                    else if (selection[1].Type == IObjectType.Arc)
                    {
                        #region line to arc
                        IArcSpecificsD obj2 = (IArcSpecificsD)selection[1].GetSpecificsD();
                        double distStart_Start = IMath.DistancePointToPoint(((ILineSpecificsD)obj).Start, obj2.Start);
                        double distStart_End = IMath.DistancePointToPoint(((ILineSpecificsD)obj).Start, obj2.End);
                        double distEnd_Start = IMath.DistancePointToPoint(((ILineSpecificsD)obj).End, obj2.Start);
                        double distEnd_End = IMath.DistancePointToPoint(((ILineSpecificsD)obj).End, obj2.End);

                        Combination comb = CheckShortestDist(distStart_Start, distStart_End, distEnd_Start, distEnd_End);

                        if (comb == Combination.START_START)
                        {
                            //start of both
                            ((ILineSpecificsD)obj).Start = obj2.Start;
                        }
                        else if (comb == Combination.START_END)
                        {
                            ((ILineSpecificsD)obj).Start = obj2.End;
                        }
                        else if (comb == Combination.END_START)
                        {
                            ((ILineSpecificsD)obj).End = obj2.Start;
                        }
                        else
                        {
                            ((ILineSpecificsD)obj).End = obj2.End;
                        }
                        selection[0].SetSpecifics(obj); 
                        #endregion
                    }
                }
                else if (obj is IArcSpecificsD)
                {
                    if (selection[1].Type == IObjectType.Line)
                    {
                        #region arc to line
                        ILineSpecificsD obj2 = (ILineSpecificsD)selection[1].GetSpecificsD();
                        double distStart_Start = IMath.DistancePointToPoint(((IArcSpecificsD)obj).Start, obj2.Start);
                        double distStart_End = IMath.DistancePointToPoint(((IArcSpecificsD)obj).Start, obj2.End);
                        double distEnd_Start = IMath.DistancePointToPoint(((IArcSpecificsD)obj).End, obj2.Start);
                        double distEnd_End = IMath.DistancePointToPoint(((IArcSpecificsD)obj).End, obj2.End);

                        Combination comb = CheckShortestDist(distStart_Start, distStart_End, distEnd_Start, distEnd_End);

                        if (comb == Combination.START_START)
                        {
                            //start of both
                            ((ILineSpecificsD)obj2).Start = ((IArcSpecificsD)obj).Start;
                        }
                        else if (comb == Combination.START_END)
                        {
                            ((ILineSpecificsD)obj2).End = ((IArcSpecificsD)obj).Start;
                        }
                        else if (comb == Combination.END_START)
                        {
                            ((ILineSpecificsD)obj2).Start = ((IArcSpecificsD)obj).End;
                        }
                        else
                        {
                            ((ILineSpecificsD)obj2).End = ((IArcSpecificsD)obj).End;
                        }
                        selection[1].SetSpecifics(obj2);
                        #endregion
                    }
                    else if (selection[1].Type == IObjectType.Arc) //beides arc
                    {
                        #region arc to arc
                        IArcSpecificsD obj2 = (IArcSpecificsD)selection[1].GetSpecificsD();
                        double distStart_Start = IMath.DistancePointToPoint(((IArcSpecificsD)obj).Start, obj2.Start);
                        double distStart_End = IMath.DistancePointToPoint(((IArcSpecificsD)obj).Start, obj2.End);
                        double distEnd_Start = IMath.DistancePointToPoint(((IArcSpecificsD)obj).End, obj2.Start);
                        double distEnd_End = IMath.DistancePointToPoint(((IArcSpecificsD)obj).End, obj2.End);
                        Combination comb = CheckShortestDist(distStart_Start, distStart_End, distEnd_Start, distEnd_End);

                        if (comb == Combination.START_START)
                        {
                            //start of both
                            ((IArcSpecificsD)obj).Start = obj2.Start;
                        }
                        else if (comb == Combination.START_END)
                        {
                            ((IArcSpecificsD)obj).Start = obj2.End;
                        }
                        else if (comb == Combination.END_START)
                        {
                            ((IArcSpecificsD)obj).End = obj2.Start;
                        }
                        else
                        {
                            ((IArcSpecificsD)obj).End = obj2.End;
                        }
                        ((IArcSpecificsD)obj).Center += ((IArcSpecificsD)obj2).Center;
                        ((IArcSpecificsD)obj).Center /= 2;

                        selection[0].SetSpecifics(obj);
                        #endregion
                    }
                }
            }
            parent.UpdateView();
		}
   private static Combination CheckShortestDist(double distStart_Start, double distStart_End, double distEnd_Start, double distEnd_End)
        {
            Combination comb = Combination.START_START;

            if (distStart_Start <= distStart_End && distStart_Start <= distEnd_End && distStart_Start <= distEnd_Start)
                comb = Combination.START_START;
            else if (distStart_End <= distStart_Start && distStart_End <= distEnd_End && distStart_End <= distEnd_Start)
                comb = Combination.START_END;
            else if (distEnd_Start <= distStart_Start && distEnd_Start <= distEnd_End && distEnd_Start <= distStart_End)
                comb = Combination.END_START;
            else
                comb = Combination.END_END;
            return comb;
        }
		 enum Combination
        {
            START_START,
            START_END,
            END_START,
            END_END
        }
    }
}
