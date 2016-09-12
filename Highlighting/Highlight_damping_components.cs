//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Mark Pins and Component from special net with color.
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
            List<String> GroundNames = new List<string>();
            GroundNames.Add("gnd");
            GroundNames.Add("ground");
            GroundNames.Add("/gnd");
            List<String> VCCNames = new List<string>();
            VCCNames.Add("3v");
            VCCNames.Add("vcc");
            VCCNames.Add("vtt");
            VCCNames.Add("/1v");
            
            foreach (ICMPObject cmp in step.GetAllCMPObjects())
            {
                if (cmp.GetPinList().Count == 2)
                {
                    List<IPin> plist = cmp.GetPinList();
                    bool gndfound = false;
                    bool vccFound = false;
                    foreach (IPin pin in plist)
                    {
                        if (StartsWithStringOfList(GroundNames ,pin.GetNetNameOnIPin(cmp).ToLower()))
                        {
                            gndfound = true;
                        }
                        if (StartsWithStringOfList(VCCNames, pin.GetNetNameOnIPin(cmp).ToLower()))
                        {
                            vccFound = true;
                        }
                    }
                    if (gndfound && vccFound)
                    {
                        cmp.ObjectColor = Color.ForestGreen;
                        foreach (IPin pin in plist)
                        {
                            if (StartsWithStringOfList(GroundNames, pin.GetNetNameOnIPin(cmp).ToLower()))
                            {
                                pin.SetPinColor(Color.BlueViolet, cmp);
                            }
                            if (StartsWithStringOfList(VCCNames, pin.GetNetNameOnIPin(cmp).ToLower()))
                            {
                                pin.SetPinColor(Color.Red, cmp);
                            }
                        }
                    }
                }
            }
            parent.UpdateView();
		}
        private bool StartsWithStringOfList(List<String> StringList, string SearchItem)
        {
            foreach (string s in StringList)
            {
                if(SearchItem.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
