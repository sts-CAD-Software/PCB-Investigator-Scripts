//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Check components and mark PTHs.
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
 			//your code here
			            int count = 0;

            IStep step = parent.GetCurrentStep();

            IODBLayer outsideToplayer = step.GetOutsideODBLayer(true);
            IODBLayer outsideBotlayer = step.GetOutsideODBLayer(false);

            if (step.GetCMPLayer(true) != null)
            {
                step.GetCMPLayer(true).EnableLayer(true);
                foreach (ICMPObject cmp in step.GetCMPLayer(true).GetAllLayerObjects())
                {
                    foreach (IPin pin in cmp.GetPinList())
                    {

                        IODBObject po = pin.GetIPinPad(outsideToplayer, cmp);
                        if (po != null)
                        {
                            IPadSpecifics os = (IPadSpecifics)po.GetSpecifics();
                            if (os.Type == PCBI.Symbol_Type.r)
                            {
                                cmp.ObjectColor = Color.Red;
                                cmp.AddComponentAttribute("techno","thr");
                                count++;
                               
                                break;
                            }
                        }
                    }
                }
            }
            if (step.GetCMPLayer(false) != null)
            {
                step.GetCMPLayer(false).EnableLayer(true);
                foreach (ICMPObject cmp in step.GetCMPLayer(false).GetAllLayerObjects())
                {
                    foreach (IPin pin in cmp.GetPinList())
                    {

                        IODBObject po = pin.GetIPinPad(outsideBotlayer, cmp);
                        if (po != null)
                        {
                            IPadSpecifics os = (IPadSpecifics)po.GetSpecifics();
                            if (os.Type == PCBI.Symbol_Type.r)
                            {
                                cmp.ObjectColor = Color.Blue;
                                cmp.AddComponentAttribute("techno", "thr");
                                count++;
                                
                                break;
                            }
                        }
                    }
                }
            }
            
            parent.UpdateView(); 
		}
		
    }
}