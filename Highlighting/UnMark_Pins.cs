// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Unmark Pins and Component from special net with color.
//-----------------------------------------------------------------------------------
// This script is created to remove pins and components from a given trace and reset their color.
// It goes through all layers and resets the color of the pins and components.
// It can be used to highlight or emphasize a trace by hiding all other elements. 
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
                        pin.ResetPinColor(cmp);
                }
            }
            parent.UpdateView();			
        }
    }
}
