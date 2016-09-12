//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Create report to check all net names are written in the freetext?
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
            Dictionary<string, string> NetDiffs = new Dictionary<string, string>();

            foreach (string layername in step.GetAllLayerNames())
            {
                ILayer layer = step.GetLayer(layername);

                if (layer == null) continue;
                if (!(layer is IODBLayer)) continue;

                IODBLayer mainLayer = (IODBLayer)layer;

                foreach (IODBObject obj in mainLayer.GetAllLayerObjects())
                {
                    if (obj.FreeText.Trim() != obj.NetName.Trim())
                    {
                        if (!NetDiffs.ContainsKey(obj.NetName))
                            NetDiffs.Add(obj.NetName, obj.FreeText);
                    }
                }
            }

            //have a list of all differences of netname and freetext
            StringBuilder MessageTextBuilder = new StringBuilder();
            MessageTextBuilder.AppendLine("Differences of FreeText and Net name for:");
            foreach (string key in NetDiffs.Keys)
            {
                MessageTextBuilder.AppendLine("Net "+key + " - "+NetDiffs[key]);
            }

            MessageBox.Show(MessageTextBuilder.ToString(), "Differences", MessageBoxButtons.OK, MessageBoxIcon.Information);
        		}
		
    }
}