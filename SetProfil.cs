//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
//
// Set Profil to fix Rectangle.
// The example rectangle is fixed size with 15 Inch x 10 Inch, just change the newBounds rectangle to get the size your company needs.
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
			  IFilter filter = new IFilter(parent);
            IODBObject outlinePolygon = filter.CreateOutlinePolygon();
            IStep parentStep = parent.GetCurrentStep();
            ISurfaceSpecifics spec = (ISurfaceSpecifics)outlinePolygon.GetSpecifics();

            spec.StartPolygon(false, new PointF());
            RectangleF newBounds = new RectangleF(0, 0, 15000, 10000);

            //create 4 lines and add them to an contour polygon
            PointF leftUp = new PointF(newBounds.Left, newBounds.Top);
            PointF leftDown = new PointF(newBounds.Left, newBounds.Bottom);
            PointF rightUp = new PointF(newBounds.Right, newBounds.Top);
            PointF rightDown = new PointF(newBounds.Right, newBounds.Bottom);
            spec.AddLine(leftUp, rightUp);
            spec.AddLine(rightUp, rightDown);
            spec.AddLine(rightDown, leftDown);
            spec.AddLine(leftDown, leftUp);

            spec.EndPolygon(); //close the new contour

            parentStep.SetPCBOutline(spec);
            parent.UpdateView(); 
		}
		
		public  StartMethode GetStartMethode()
		{
			return StartMethode.Synchronous;
		}
    }
}
