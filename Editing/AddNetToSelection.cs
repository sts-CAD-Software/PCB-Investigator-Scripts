//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script (Synchronous)
// Created on 06.12.2017
// Author Fabio.Gruber
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 06.12.2017
// Description: Select a element and add a netname.
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID=AddNetToSelection_636481536908639447
// ButtonEnabled=16   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)
//
// The purpose of the script is to add a netname to a selected object in the current step of the PCB.
// The script prompts the user to enter a netname and then iterates through the selected objects, adding the netname to each object's layer and net information.
// The script also adds a unique identifier and button state so that it can be easily accessed from the ribbon menu. 
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
            string netName = "TestNet";
            PCB_Investigator.PCBIWindows.PCBIEnterTextDialog enterDLG = new PCB_Investigator.PCBIWindows.PCBIEnterTextDialog("Enter Net Name", "Net Name", netName, false, false);
            if (enterDLG.ShowDialog() == DialogResult.OK)
            {
                netName = enterDLG.GetText();
            }
            else return;

            IStep currentStep = parent.GetCurrentStep();

            foreach (IODBObject selectedObj in currentStep.GetSelectedElements())
            {
                ILayer parentLayer = currentStep.GetLayer(selectedObj.GetParentLayerName());
                if (parentLayer == null) continue;

                if (parentLayer is IODBLayer)
                    AddNetInfos(currentStep, (IODBLayer)parentLayer, netName, selectedObj);

            }

            parent.UpdateView();
        }

        private static void AddNetInfos(IStep step, IODBLayer parentLayer, String NetName, IODBObject netItem)
        {
            if (NetName.Length == 0) //the default net
                NetName = "$NONE$";

            int lastNetNr = step.GetNets().Count;
            INet outNet = step.GetNet(NetName);
            int netNr = -1;
            if (outNet == null)
                netNr = step.AddNet(NetName, "", out outNet);
            else
                netNr = outNet.GetNetNumber();

            netItem.PcbNetNumber = outNet.GetNetNumber(); //set the netinformation to the object
            int newNr = outNet.AddLayerRef(parentLayer.GetLayerName(), lastNetNr); //each net saves a list of all layers are used.
            if (newNr == lastNetNr)
                lastNetNr++;
            parentLayer.SetNetNumber(NetName, netItem.PcbNetNumber);//for each object the layer needs information that this net is used.
            outNet.AddFID(PCBI.FidType.Copper, parentLayer.GetLayerName(), netItem.GetIndexOnLayer(), ""); //this is specific to ODB++, if you don't use it and save the data as ODB++ some information get lost.
        }
    }
}