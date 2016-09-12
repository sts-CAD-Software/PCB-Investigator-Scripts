//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Count Pins and Component from selection.
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
            if (parent == null) return;
            IStep step = parent.GetCurrentStep();
            IFilter filter = new IFilter(parent);
            int componentCount = step.GetSelectedCMPs().Count;
            int PinCount = 0;
            foreach (ICMPObject cmp in step.GetSelectedCMPs())
            {
                 PinCount += cmp.GetPinList().Count;
            }
            MessageBox.Show("Selected components: " + componentCount.ToString() + "  Pin count of all selected components: " + PinCount.ToString(), "Component/Pin count");
		}
		
    }
}
