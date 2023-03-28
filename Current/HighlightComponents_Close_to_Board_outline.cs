//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Ceck distance components to board outline
//-----------------------------------------------------------------------------------
// This script is a so called automation script for the software "PCB-Investigator".
// It checks the distance between the components and the board edge (the so-called PCB Outline).
// If the distance is less than 0.5 mm, the script marks the components in question.
// A message is output indicating the distance between the components and the PCB edge in mils or microns, depending on the units used by the software. 
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
            if (parent.GetCurrentStep() == null) return;
            IStep step = parent.GetCurrentStep();
            IFilter filter = new IFilter(parent);

            if (parent.GetCurrentStep() == null) return;

            if(step.GetCMPLayer(true) != null)
            {
                step.GetCMPLayer(true).EnableLayer(true);
            }
            if (step.GetCMPLayer(false) != null)
            {
                step.GetCMPLayer(false).EnableLayer(true);
            }
            double distance = PCBI.MathUtils.IMath.MM2Mils(0.5f);
            if (parent.GetUnit())
            {
                MessageBox.Show("Component to Outline distance: " + PCBI.MathUtils.IMath.Mils2MM(distance).ToString("N3") + " micron" , "DRC Component to Board Outline");

            }
            else
            {
                MessageBox.Show("Component to Outline distance: " + distance.ToString("N3") + " mils", "DRC Component to Board Outline");
            }
            foreach (ICMPObject cmp in step.GetAllCMPObjects())
            {
                PCBI.MathUtils.IPolyClass CMP_Poly = cmp.GetPolygonOutline(false);
                IODBObject boardOutline = parent.GetCurrentStep().GetPCBOutlineAsODBObject();
                IPolyClass polyOutline = boardOutline.GetPolygonOutline();
                PointD start = new PointD(0,0);
                PointD end = new PointD(0, 0);

                if (cmp.GetPolygonOutline().DistanceTo(polyOutline, ref start, ref end) < distance) 
                {
                    cmp.Select(true);
                }
            }
            parent.UpdateView();
		}
				
    }
}
