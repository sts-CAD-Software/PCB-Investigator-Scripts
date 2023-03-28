//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Copy Component body to drawing layer.
//-----------------------------------------------------------------------------------
// The script traverses all pins on the "top" layer of the current step in a PCB design and creates polygons that map the outline of each pin.
// It uses the IPCBIWindow interface to access the current step layer and the layers of the PCB.
// The polygons are constructed using points, lines and arcs based on the outline of each pin, and are finally updated for display.
// The script is used to improve the clarity of the board and help customers select the pins they need. 
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
            IStep step = parent.GetCurrentStep();
            IODBLayer layer = (IODBLayer)step.GetLayer("top");
            IFilter filter = new IFilter(parent);
            // Pins
            foreach (ICMPObject cmp in step.GetAllCMPObjects())
            {
                foreach (IPin pin in cmp.GetPinList())
                {
                    IODBObject outlinePolygon = filter.CreatePolygon(layer);
                    ISurfaceSpecifics specOutline = (ISurfaceSpecifics)outlinePolygon.GetSpecifics();
                    bool polyStart = true;
                    PointF StartPoint = new PointF(0,0);
                    PointF EndPoint = new PointF(0, 0);
                    foreach (IEdge edge in pin.GetPolygonOutline(cmp).GetEdges())
                    {
                        if (polyStart)
                        {
                            polyStart = false;
                            StartPoint = new PointF((float)edge.Begin.X, (float)edge.Begin.Y);
                            specOutline.StartPolygon(false, StartPoint);


                            if (edge is IArcEdge)
                            {
                                IArcEdge aEdge = (IArcEdge)edge;
                                PointF start = new PointF((float)aEdge.Begin.X, (float)aEdge.Begin.Y);
                                PointF end = new PointF((float)aEdge.End.X, (float)aEdge.End.Y);
                                PointF center = new PointF((float)aEdge.Center.X, (float)aEdge.Center.Y);
                                specOutline.AddArc(start, end, center, aEdge.ClockWise);
                                if (aEdge.ClockWise)
                                {
                                    EndPoint = end;
                                }
                                else
                                {
                                    EndPoint = start;
                                }
                            }
                            else if (edge is ILineEdge)
                            {
                                ILineEdge aEdge = (ILineEdge)edge;
                                PointF start = new PointF((float)aEdge.Begin.X, (float)aEdge.Begin.Y);
                                PointF end = new PointF((float)aEdge.End.X, (float)aEdge.End.Y);
                                specOutline.AddLine(start, end);
                                EndPoint = end;
                            }

                        }
                        else
                            if (edge is IArcEdge)
                            {
                                IArcEdge aEdge = (IArcEdge)edge;
                                PointF start = new PointF((float)aEdge.Begin.X, (float)aEdge.Begin.Y);
                                PointF end = new PointF((float)aEdge.End.X, (float)aEdge.End.Y);
                                PointF center = new PointF((float)aEdge.Center.X, (float)aEdge.Center.Y);
                                specOutline.AddArc(start, end, center, aEdge.ClockWise);
                                if (aEdge.ClockWise)
                                {
                                    EndPoint = end;
                                }
                                else
                                {
                                    EndPoint = start;
                                }
                            }
                            else if (edge is ILineEdge)
                            {
                                ILineEdge aEdge = (ILineEdge)edge;
                                PointF start = new PointF((float)aEdge.Begin.X, (float)aEdge.Begin.Y);
                                PointF end = new PointF((float)aEdge.End.X, (float)aEdge.End.Y);
                                specOutline.AddLine(start, end);
                                EndPoint = end;
                            }
                    }
                    //specOutline.AddLine(EndPoint, StartPoint);
                    specOutline.EndPolygon(); //close the new contour
                    outlinePolygon.SetSpecifics(specOutline);
                }
            }
            parent.UpdateView();			
        }
		
   }
}
