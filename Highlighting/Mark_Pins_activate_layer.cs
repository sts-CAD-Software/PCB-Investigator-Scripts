// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Mark Pins and Component from special net with color.
//-----------------------------------------------------------------------------------
// This script highlights the selected lines and arcs in a PCB by marking them with a dashed line and a colored circle.
// The script is automatically executed when a PCB is opened and remains active in the background until the PCB is closed.
// The script is used to make it easier to find and identify lines and arcs in a board. 
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
			IFilter filter = new IFilter(parent);
			foreach (ICMPObject cmp in step.GetAllCMPObjects())
			{
				List<IPin> plist = cmp.GetPinList();
				foreach (IPin pin in plist)
				{
					if (pin.GetNetNameOnIPin(cmp).ToLower().Contains("gnd"))
					{
						pin.SetPinColor(Color.Blue, cmp);
					}
				}
			}
            ICMPLayer layer = step.GetCMPLayer(true);
            if (layer != null) layer.EnableLayer(true);
            parent.UpdateView();			
        }
    }
}
