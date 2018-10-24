//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script (Synchronous)
// Created on 24.10.2018
// Author Guenther.Schindler
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 24.10.2018
// Description: Set origin for all Baord Layers and profile to left bottom corner
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID=SetOriginLetBottom_636759874699483078
// ButtonEnabled=1   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)

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
			//your code here
			SetOriginBotLeft(parent);
			parent.UpdateView(); 
		}
        private void SetOriginBotLeft(IPCBIWindow parent)
        {
            IStep step = parent.GetCurrentStep();
            IMatrix PCB_Matrix = parent.GetMatrix();
            IODBObject profile = step.GetPCBOutlineAsODBObject();
            ISurfaceSpecificsD profileSurface = (ISurfaceSpecificsD)profile.GetSpecificsD(); ;

            RectangleD profileRect = profileSurface.GetBounds();
            PointD OriginPoint = new PointD(-profileRect.Left, -profileRect.Top); ;

            foreach (string layerName in PCB_Matrix.GetAllBoardLayerNames(true))
            {
                step.GetLayer(layerName).MoveLayer(OriginPoint.ToPointF());
            }
            profile.SetOffset(OriginPoint);
           
            parent.UpdateView();
        }		
	}
}