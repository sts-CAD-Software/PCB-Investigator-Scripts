//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 28.03.2015
// Autor Guenther
// 
// Prepare Data for real 3D.
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
using System.Diagnostics;

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
		    buttonPrepareFor3D(parent);
			
            parent.UpdateView(); 
		}
        private void buttonPrepareFor3D(IPCBIWindow Parent)
		{
                    float DrillMinSzize = 50;
            IMatrix matrix = Parent.GetMatrix();
            IStep step = Parent.GetCurrentStep();
            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            foreach (string layerName in matrix.GetAllDrillLayerNames())
            {

                ILayer layer = step.GetLayer(layerName);
                layer.EnableLayer(true);
                if (layer.GetType() == typeof(IODBLayer))
                {
                    foreach (IODBObject obj in layer.GetAllLayerObjects())
                    {
                        if (obj.GetDiameter() > DrillMinSzize)
                        {
                            obj.Select(true);
                        }
                    }
                }

            }
            Parent.UpdateSelection();
            Parent.UIAction.Execute(ID_ActionItem.ID_ADD_TO_OUTLINE_FROM_SELECTION);

            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);
            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            foreach (string layerName in matrix.GetAllDrillLayerNames())
            {
                ILayer drill_layer = step.GetLayer(layerName);
                foreach (string sigLayer in matrix.GetAllSignalLayerNames())
                {
                    Parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);
                    Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
                    Parent.UpdateSelection();
                    Parent.UpdateView();
                    IODBLayer odb_sig_layer = (IODBLayer)step.GetLayer(sigLayer);
                    IFilter filter = new IFilter(Parent);


                    if (drill_layer.GetType() == typeof(IODBLayer))
                    {
                        foreach (IODBObject obj in drill_layer.GetAllLayerObjects())
                        {
                            obj.Select(true);
                            if (obj.Type == IObjectType.Pad)
                            {
                                IPadSpecifics ops = (IPadSpecifics)obj.GetSpecifics();
                                ops.Positive = false;
                                obj.SetSpecifics(ops);
                            }
                            else if (obj.Type == IObjectType.Line)
                            {
                                ILineSpecifics ops = (ILineSpecifics)obj.GetSpecifics();
                                ops.Positive = false;
                                obj.SetSpecifics(ops);
                            }
                        }
                        Parent.UpdateSelection();
                        Parent.UIAction.Execute(ID_ActionItem.ID_COPY_SELECTED_ODBOBJECTS);
                        drill_layer.EnableLayer(false);
                        odb_sig_layer.EnableLayer(true);
                        Parent.UpdateSelection();
                        Parent.UpdateView();
                        Parent.UIAction.Execute(ID_ActionItem.ID_PASTE_COPIED_ITEMS);
                        odb_sig_layer.EnableLayer(false);
                    }
                }
            }

            //
            foreach (string layerName in matrix.GetAllDrillLayerNames())
            {
                ILayer drill_layer = step.GetLayer(layerName);

                if (drill_layer.GetType() == typeof(IODBLayer))
                {
                    foreach (IODBObject obj in drill_layer.GetAllLayerObjects())
                    {
                       if (obj.Type == IObjectType.Pad)
                            {
								IPadSpecifics ops = (IPadSpecifics)obj.GetSpecifics();
								ops.Positive = true;
								obj.SetSpecifics(ops);
							}
                            else if (obj.Type == IObjectType.Line)
                            {
                                ILineSpecifics ops = (ILineSpecifics)obj.GetSpecifics();
                                ops.Positive = true;
                                obj.SetSpecifics(ops);
                            }
                    }
                }
            }
            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);
            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_SIGNAL_LAYER_ON);
            Parent.UpdateView();
            //
            //Parent.UIAction.Execute(ID_ActionItem.ID_SAVE);
            //Process.Start(@"C:\Program Files (x86)\NativeBoardImport\NBI Catia V5\NBI_Catia_V5.exe");
            
        }

		      
    }
}