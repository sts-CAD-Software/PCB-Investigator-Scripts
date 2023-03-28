//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Add drawing layer.
//-----------------------------------------------------------------------------------
// This script creates a new drawing layer in PCB Investigator. It checks if a job is loaded and then creates a new layer named "help_layer".
// The new layer is activated and the view is updated. 
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
			 IFilter filter = new IFilter(parent);
            IStep curStep = parent.GetCurrentStep();
            if (curStep == null)
            {
                MessageBox.Show("No Job loaded, please load a job before start this script!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            } 
            
            IODBLayer newLayer = filter.CreateEmptyODBLayer("help_layer", curStep.Name);
            newLayer.EnableLayer(true);
			
			IMatrix matrix = parent.GetMatrix();
            matrix.UpdateDataAndList();
            parent.UpdateView();
		}
		
    }
}
