//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 29.04.2014
// Autor Fabio
// 
// Example to highlight pins on top component layer with asynchronous script.
//-----------------------------------------------------------------------------------

using PCBI.Automation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PCBIScript
{
    public class PScript : IPCBIScriptASync
    {
       
        public void Execute(PCBI.Automation.IPCBIWindow parent)
        {
            //do  work here
            bool exampleColorReset = true;
            while (true)
            {
                Thread.Sleep(1000);

                if (isDisposed)
                    break;

                //example work to highlight pins of gnd net
                IStep curStep = parent.GetCurrentStep();
                if (curStep == null) return;

                ICMPLayer topLayer = curStep.GetCMPLayer(true);

                if (topLayer == null) continue;

                foreach (ICMPObject cmp in topLayer.GetAllLayerObjects())
                {
                    foreach (IPin pin in cmp.GetPinList())
                    {
                        if (pin.GetNetNameOnIPin(cmp).ToLowerInvariant().Contains("gnd"))
                        {
                            if(exampleColorReset)
                                pin.ResetPinColor(cmp);
                            else
                                pin.SetPinColor(System.Drawing.Color.Aqua, cmp);
                        }
                    }
                }
                exampleColorReset = !exampleColorReset;
				parent.UpdateView();
            }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            isDisposed = true;
        }
    }
}
