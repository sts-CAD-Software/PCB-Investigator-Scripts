// PCB-Investigator Automation Script
// Created on 2015-28-10
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
//
// Cross Probe Components in Expedition. This example works only with running Expedition!
//-----------------------------------------------------------------------------------
// This script allows cross-probing of components in Expedition.
// Cross-probing refers to the ability to switch between two views of a circuit or design while maintaining focus on a specific component or part.
// In this script, the selected components are highlighted in Expedition and the zoom is set to the selection.
// The script also includes code that connects to the Expedition PCB design software and validates a license.
// It also contains a method for releasing resources used by the application.
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
using System.Reflection;
using System.Threading;
namespace PCBIScript
{
    public class PScript : IPCBIScriptASync
    {
        public PScript()
        {
        }
        public IPCBIWindow Parent;
        public dynamic pcbApp = null;
        public dynamic pcbDoc = null;
        public void Execute(IPCBIWindow parent)
        {
            Parent = parent;
            IStep step = parent.GetCurrentStep();
            if (step == null) return;
            IFilter filter = new IFilter(parent);


            try
            {
                pcbApp = System.Runtime.InteropServices.Marshal.GetActiveObject("MGCPCB.Application");

            }
            catch (Exception)
            {
                MessageBox.Show("MGCPCB needs to be running!");
            }

            if (pcbApp == null)
            {
                pcbApp = Type.GetTypeFromProgID("MGCPCB.Application");
                pcbApp = Activator.CreateInstance(pcbApp);
            }
            pcbDoc = pcbApp.ActiveDocument;
            MGCPCB_Connect();

            parent.PCBISelectionChanged += new EventHandler(parent_PCBISelectionChanged);
            while (true)
            {
                Thread.Sleep(1000);

                if (isDisposed)
                {
                    parent.PCBISelectionChanged -= parent_PCBISelectionChanged;
                    break;
                }
            }
            //release the object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pcbDoc);
        }
        void parent_PCBISelectionChanged(object sender, EventArgs e)
        {
            pcbDoc.UnSelectAll();
            foreach (ICMPObject cmp in Parent.GetCurrentStep().GetSelectedCMPs())
            {
                pcbDoc.FindComponent(cmp.Ref).Selected = true;
            }
	// Set zoom to selection
	pcbDoc.ActiveView.SetExtentsToSelection();
	// Set zoom to visible bounds
	//pcbDoc.ActiveView.SetExtents(Parent.GetVisibleArea().X, Parent.GetVisibleArea().Y, Parent.GetVisibleArea().Right, Parent.GetVisibleArea().Bottom,2);
        }
        bool isDisposed = false;
        public void Dispose()
        {
            isDisposed = true;
        }
        public void MGCPCB_Connect()
        {
            // connects to MGCPCB    
            int key;
            int licenseToken;
            key = pcbDoc.Validate(0);
            dynamic licenseServer = Type.GetTypeFromProgID("MGCPCBAutomationLicensing.Application");
            licenseServer = Activator.CreateInstance(licenseServer);
            licenseToken = licenseServer.GetToken(key);
                MessageBox.Show(licenseToken.ToString() + "MGCPCB needs to be running!");
            licenseServer = null;
            try
            {
                pcbDoc.Validate(licenseToken);
            }
            catch
            {
                MessageBox.Show("MGCPCB needs to be running!");
            }
        }
    }
}
