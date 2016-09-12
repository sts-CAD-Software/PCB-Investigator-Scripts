//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 02.09.2014
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Asynchronous script to compare layers, this works only if two layers active.
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
using System.Threading;
using PCBI.MathUtils;

namespace PCBIScript
{
    public class PScript : IPCBIScriptASync
    {
        public PScript()
        {

        }
        public IPCBIWindow Parent;
        public void Execute(IPCBIWindow parent)
        {
            Parent = parent;
            parent.PCBIActiveLayerChanged += new EventHandler(parent_PCBIActiveLayerChanged);
            parent_PCBIActiveLayerChanged(null, null);
            parent.PCBIJobClosed += Wind_PCBIJobClosed; //stop if job is closed

            while (true)
            {
                Thread.Sleep(100);

                if (isDisposed)
                    break;
            }
        }
        private void Wind_PCBIJobClosed(object sender, EventArgs e)
        {
            Dispose();
        }

        bool isDisposed = false;
        public void Dispose()
        {
            Parent.PCBIActiveLayerChanged -= new EventHandler(parent_PCBIActiveLayerChanged);
            isDisposed = true;
        }
        public void parent_PCBIActiveLayerChanged(object sender, EventArgs e)
        {
            int i = 0;
			IStep step = Parent.GetCurrentStep();
			if(step==null) return;
            List<ILayer> activeLayers = step.GetActiveLayerList();
            if (activeLayers.Count != 2) return;
            foreach (ILayer layer in activeLayers)
            {
                if (layer.GetType() == typeof(IODBLayer))
                {
                    if (i == 0)
                    {
                        i++;
                        ((IODBLayer)layer).Col = Color.FromArgb(255, 255, 250, 0);
                    }
                    else
                        if (i == 1)
                        {
                            ((IODBLayer)layer).Col = Color.FromArgb(255, 255, 240, 0);
                        }
                }
            }
            Parent.IColorsetting.ColorMix = PCBI.DrawingMode.special;
            Parent.UpdateView();
        }
    }
}