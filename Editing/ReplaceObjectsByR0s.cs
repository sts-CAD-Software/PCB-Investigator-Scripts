//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2015-05-18
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Check all active layers for symbols with diameter 0 and replace them by rounded symbol with diameter 1.

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
            
            //the symbol diameter selected by user
            float diameterReplacedSymbolsInMils = 1;

            foreach (ILayer activeLayer in step.GetActiveLayerList())
            {
                if (!(activeLayer is IODBLayer)) //component and picture layer irrelevant
                { continue; }

                IODBLayer Layer = (IODBLayer)activeLayer;
                int symbolIndex = -1;
                IFilter.AddToolDefinitionRound(Layer, diameterReplacedSymbolsInMils);
                foreach (IODBObject obj in Layer.GetAllLayerObjects())
                {
                    if (obj.GetDiameter() == 0) //all elements with diameter 0 replaced by new symbol
                    {
                        if (symbolIndex < 0)
                            symbolIndex = IFilter.AddToolDefinitionRound(Layer, diameterReplacedSymbolsInMils); //add new symbol only if used

                        obj.SetSpecifics(obj.GetSpecificsD(), symbolIndex);
                    }
                }
            }
        }
    }
}