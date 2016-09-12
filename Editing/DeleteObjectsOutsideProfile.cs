//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 14.10.2015
// Autor Fabio Gruber
// 
// Delete all elements outside of PCB profile.
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

 		public void Execute(IPCBIWindow Parent)
        {
            IStep step = Parent.GetCurrentStep();
            IMatrix matrix = Parent.GetMatrix();
            if (step == null) return;
            IODBObject profile = step.GetPCBOutlineAsODBObject();

            foreach (string layerName in step.GetAllLayerNames())
            {
                if (matrix.GetMatrixLayerType(layerName) == MatrixLayerType.Component)
                    continue; //no component layers

                ILayer Layer = step.GetLayer(layerName);
                if (Layer is IODBLayer)
                {
                    List<IODBObject> objectsToDelete = new List<IODBObject>();
                    IODBLayer layer = (IODBLayer)Layer;
                    foreach (IODBObject obj in layer.GetAllLayerObjects())
                    {
                        if (profile.IsPointOfSecondObjectIncluded(obj))
                        {
                            //inside not delete
                        }
                        else
                        {
                            objectsToDelete.Add(obj);
                        }
                    }
                    layer.RemoveObjects(objectsToDelete);
                }
            }
            Parent.UpdateView();
        }    
	}	
}