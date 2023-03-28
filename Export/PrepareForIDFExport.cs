//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 09.03.2016
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// All active layers will be polygonize without flatten of arcs, this is necessary for IDF export to get smaller elements.
//-----------------------------------------------------------------------------------
// GUID PrepareIDFExport_635900140114543122
// EnableOn JobOpen
// Category Edit
//
// This script is useful to convert all active layers of the current step into a polygonal shape without flattening arcs.
// This is necessary for exporting IDF files to get smaller elements.
// It script changes the layer data and when you save changes, many elements are removed and replaced with surfaces. 
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
            IStep curStep = parent.GetCurrentStep();
            if (curStep == null) return;

            foreach (ILayer layer in curStep.GetActiveLayerList())
            {
                if (layer is IODBLayer)
                {
                    ((IODBLayer)layer).PolygonizeLayer(0, false);
                }
            } 
            
            parent.UpdateView();
        }

    }
}