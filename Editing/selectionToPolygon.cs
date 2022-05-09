//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script (Synchronous)
// Created on 09.05.2022
// Author Fabio.Gruber
// SDK online reference http://manual.pcb-investigator.com/InterfaceDocumentation/index.php
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 09.05.2022
// Description: Replace selection by surface.
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID=padToPolygon_637877000111068306
// ButtonEnabled=1   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)
// AutoStart=false

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
		public PScript()
		{
		}

		public void Execute(IPCBIWindow parent)
		{
			IStep step = parent.GetCurrentStep();
			if(step==null)return;
			
			foreach(IODBObject selectedObj in step.GetSelectedElements())
			{
			 IPolyClass poly = selectedObj.GetPolygonOutline();
			 if(poly==null|| poly.GetEdgeCount()==0)continue;
			 
			 IODBLayer layer = step.GetLayer(selectedObj.GetParentLayerName()) as IODBLayer;
			 if(layer==null)continue;
			 
			 IODBObject objSurface = poly.GetSurfaceFromPolygon(layer);
			 selectedObj.ReplaceItemBy(objSurface);
			}
			
			parent.UpdateView(); 
		}
	}
}