//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Copy all via pads from one layer("top") to another layer("ViaLayer").
//-----------------------------------------------------------------------------------
// The script creates a new layer in a PCB and copies all vias from an existing layer to the new layer.
// It checks if a job is loaded in the software and if the layers are defined correctly.
// The vias are created on the new layer as new pads with the same dimensions as the original vias. 
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
 			CreateNewLayer(parent, "ViaLayer");

			CopyAllViasToNewLayer(parent, "top", "ViaLayer");

            		parent.UpdateView(); 
		}

	 private void CopyAllViasToNewLayer(IPCBIWindow parent, string layerName, string newLayerName)
        { 
            IStep curStep = parent.GetCurrentStep();
            if (curStep == null)
            {
                MessageBox.Show("No Job loaded, please load a job before start this script!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            ILayer relevantLayer = curStep.GetLayer(layerName);
            ILayer NewLayer = curStep.GetLayer(newLayerName);
            if (NewLayer == null || relevantLayer == null || !(NewLayer is IODBLayer))
            {
                MessageBox.Show("Check Layers, something is wrong!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            List<IODBObject> ViaList = new List<IODBObject>();

            //check all elements for via attribute
            foreach (IObject obj in relevantLayer.GetAllLayerObjects())
            {
                if (obj is IODBObject)
                {
                    if (((IODBObject)obj).Type == IObjectType.Pad)
                    {
                        Dictionary<PCBI.FeatureAttributeEnum, string> attributesOfPad = ((IODBObject)obj).GetAttributesDictionary();

                        if (attributesOfPad.ContainsKey(PCBI.FeatureAttributeEnum.pad_usage))
                        {
                            if (attributesOfPad[PCBI.FeatureAttributeEnum.pad_usage] == "via")
                            { 
                                ViaList.Add((IODBObject)obj);
                            }
                        }
                    }
                }
            }
            IFilter filter = new IFilter(parent); //to create copies on new layer
            
            //add vias to new layer
            foreach(IODBObject viaPad in ViaList)
            {
                IPadSpecificsD padInfo = (IPadSpecificsD)viaPad.GetSpecificsD();

                int toolNr = IFilter.AddToolDefinitionRound((IODBLayer)NewLayer, (float)padInfo.Diameter, -1);

                IODBObject newViaPad = filter.CreatePad((IODBLayer)NewLayer);

                newViaPad.SetSpecifics(padInfo, toolNr);
                newViaPad.SetAttribute("pad_usage=via");
            }
        }
        private void CreateNewLayer(IPCBIWindow parent, string newlayername)
        {
            IFilter filter = new IFilter(parent);
            IStep curStep = parent.GetCurrentStep();
            if (curStep == null)
            {
                MessageBox.Show("No Job loaded, please load a job before start this script!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            IODBLayer newLayer = filter.CreateEmptyODBLayer(newlayername, curStep.Name);
            newLayer.EnableLayer(true);

            IMatrix matrix = parent.GetMatrix();
            matrix.SetMatrixLayerContext(newlayername, MatrixLayerContext.Board);
            matrix.SetMatrixLayerType(newlayername, MatrixLayerType.Signal);
            matrix.UpdateDataAndList();
            parent.UpdateView();

        }
		
    }
}