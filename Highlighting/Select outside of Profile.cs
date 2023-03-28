//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 14.10.2015
// Autor Fabio Gruber
// 
// Select all elements outside the board contour, e.g. to delete them.
//-----------------------------------------------------------------------------------
// The main goal of the script is to mark and select all objects on all layers (except component layers) within the scope of the PCB profile. 
// The script accesses the current step and matrix instance to get information about the PCB and its layers. 
// Then it retrieves the PCB profile as an ODB object and checks whether each object on the plane is inside the profile or not. 
// If an object is inside the profile, it is ignored, otherwise it is marked and selected. 
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
                    continue; //no component layer

                ILayer Layer = step.GetLayer(layerName);
                if (Layer is IODBLayer)
                {
                    bool foundOne = false;
                    IODBLayer layer = (IODBLayer)Layer;
                    foreach (IODBObject obj in layer.GetAllLayerObjects())
                    {
                        if (profile.IsPointOfSecondObjectIncluded(obj))
                        {
                            //inside not relevant
                        }
                        else
                        {
                            obj.Select(true);
                            foundOne = true;
                        }
                    }
                    if (foundOne) Layer.EnableLayer(true);
                }
            }
            Parent.UpdateSelection();
            Parent.UpdateView();
        }		
    }
}