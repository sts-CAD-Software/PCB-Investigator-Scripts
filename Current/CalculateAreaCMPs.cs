//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 16.03.2016
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Calculate Area of selected components side walls.
// Use option to calculate for TOP/BOT side of the PCB with components or only selected components.
//-----------------------------------------------------------------------------------
// GUID newScript_635937300928476402
// EnableOn JobOpen
// Category Report

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
using Microsoft.VisualBasic;

namespace PCBIScript
{
   public class PScript : IPCBIScript
	{
		public PScript()
		{
		}

        public void Execute(IPCBIWindow parent)
        {
            //set options here:
            bool onlySelectedComponent = true;
            bool CalculateTopSide = true;
            bool CalculateBotSide = true;
            double TopArea = 0;
            double BotArea = 0;

            IStep step = parent.GetCurrentStep();

            if (step == null) return; //no project loaded

            List<ICMPObject> allCMPs = step.GetAllCMPObjects();

            foreach (ICMPObject cmp in allCMPs)
            {
                if (onlySelectedComponent && !cmp.IsSelected) continue;

                PCBI.MathUtils.IPolyClass polygonOfCMP = cmp.GetPolygonOutline();

                foreach (PCBI.MathUtils.IEdge edge in polygonOfCMP.GetEdges())
                {
                    if (!CalculateTopSide && cmp.PlacedTop) continue;
                    if (!CalculateBotSide && !cmp.PlacedTop) continue;

                    //work with integrals to calculate the area
                    double extent = 0;
                    if (edge.Type == IEdgeType.Line)
                    {
                        extent = PCBI.MathUtils.IMath.DistancePointToPoint(edge.Begin, edge.End);
                    }
                    else
                    {
                        // Pi * R * winkel /180
                        double angle = PCBI.MathUtils.IMath.GetAngle(edge.Begin, edge.End, ((PCBI.MathUtils.IArcEdge)edge).Center, ((PCBI.MathUtils.IArcEdge)edge).ClockWise);
                        double radius = PCBI.MathUtils.IMath.DistancePointToPoint(edge.Begin, ((PCBI.MathUtils.IArcEdge)edge).Center);
                        extent = Math.PI * radius * angle / 180;
                    }
                    if (cmp.PlacedTop)
                    {
                        TopArea += extent * cmp.CompHEIGHT;
                    }
                    else
                    {
                        BotArea += extent * cmp.CompHEIGHT;
                    }
                }
            }

            double areaPCB = step.CalculateBoardArea();

            if (parent.GetUnit()) //change mils² to cm²
            {
                areaPCB = areaPCB / 155000.31; //(1/2.54*2.54)
                TopArea = TopArea / 155000.31;
                BotArea = BotArea / 155000.31;
            }

            System.Windows.Forms.MessageBox.Show((onlySelectedComponent ? "Selected Component side walls:" : "All side walls of Components:") + Environment.NewLine +
                (CalculateTopSide ? "Top Side Components: " + TopArea + Environment.NewLine : "") +
                (CalculateBotSide ? "Bot Side Components: " + BotArea + Environment.NewLine : "") +
                ((!CalculateBotSide && !CalculateTopSide) ? "No Elements allowed, please run check with other parameters!" : "") + Environment.NewLine +
                "Board Area " + areaPCB + (parent.GetUnit() ?  " cm²":" mils²")
                , "Result", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }
		
    }
}
