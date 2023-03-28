//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script (Synchronous)
// Created on 09.05.2022
// Author Guenther.Schindler
// SDK online reference http://manual.pcb-investigator.com/InterfaceDocumentation/index.php
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 31.05.2022 add negative object handling
// Description: Create Polygon of selection.
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID=selectionToPolyGon_637877004572433762
// ButtonEnabled=1   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)
// AutoStart=false
//
// This script is used to edit a selected outline in the current PCB board layout file.
// The script uses a loop to retrieve any selected object whose outline is divided into polygons and subdivided into positive or negative faces.
// It then creates a new polygon object on the same plane as the selected object and replaces the selected object with the new polygon object.
// If the original object is negative, the new polygon object will also be set negative.
//-----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using PCBI.Plugin;
using PCBI.Plugin.Interfaces;
using System.Windows.Forms;
using System.Drawing;
using PCBI.Automation;
using PCBIScript.DebugHelp; 
using System.IO;
using System.Linq;
using System.Drawing.Drawing2D;
using PCBI.MathUtils;

namespace PCBIScript
{
	public class PScript : IPCBIScript
	{
		public void Execute(IPCBIWindow parent)
		{
            IStep step = parent.GetCurrentStep();
            if(step==null)return;
            
            IFilter filter = new IFilter(parent);
            
            foreach(IODBObject selectedObj in step.GetSelectedElements())
            {
             IPolyClass poly = selectedObj.GetPolygonOutline();
             if(poly==null|| poly.GetEdgeCount()==0)continue;
             
             IODBLayer layer = step.GetLayer(selectedObj.GetParentLayerName()) as IODBLayer;
             if(layer==null)continue;
             
             foreach(ISurfaceSpecificsD surf in poly.SplitInIsleAndHoles(parent, layer))
             {
             IODBObject objSurface = filter.CreatePolygon(layer);
             
             bool doSetNegative = false;
             if(!selectedObj.Positive)
                doSetNegative = true;
                
             selectedObj.ReplaceItemBy(objSurface);
             
             if(doSetNegative)
             {
                surf.Positive = false; 
             } 
             objSurface.SetSpecifics(surf);
             }
            }
            
            
            parent.UpdateView(); 
		}
	}
}