//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 20.01.2015
// Autor  support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Mark all pins with wrong net connection (it checks the pin pad and compare it with net data of component pin).
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
 			//only for current step
	 IStep step = parent.GetCurrentStep();

            List<INet> allNets = step.GetNets();
            IODBLayer topSignal = step.GetOutsideODBLayer(true);
            IODBLayer botSignal = step.GetOutsideODBLayer(false);
            int countDifferentces = 0;
            foreach (INet net in allNets)
            {
                foreach (INetObject cmpPinCombi in net.ComponentList)
                {
                    IPin pinRel = cmpPinCombi.ICMP.GetPin(cmpPinCombi.PinIndex);

                    string netName = pinRel.GetNetNameOnIPin(cmpPinCombi.ICMP);

                    IODBObject relPad = pinRel.GetIPinPad(cmpPinCombi.ICMP.PlacedTop?topSignal:botSignal, cmpPinCombi.ICMP);

                    if (relPad!=null && relPad.NetName != netName)
                    {
                        pinRel.SetPinColor(Color.AliceBlue, cmpPinCombi.ICMP);
                        countDifferentces++;
                       
                    }
                    else
                        pinRel.SetPinColor(Color.BurlyWood, cmpPinCombi.ICMP);

                }
            }

            //you can add some code to show a report by using countDifferentces

            parent.UpdateView();

		}
		
    }
}