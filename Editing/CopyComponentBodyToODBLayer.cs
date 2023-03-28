//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Copy Component shapes from ODB++ comp_+_top layer to misc layer objects.
//-----------------------------------------------------------------------------------
// This script allows to copy the shapes of parts on an ODB++ layer.
// A new layer is created for the top and bottom shapes of parts.
// The script goes through all the parts on the current board and copies their shapes to the corresponding layer.
// The result is a simplified overview of the shapes of the parts on the board. 
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
            if (parent.GetCurrentStep() == null) return;
            bool clearLayer = false;
            IStep step = parent.GetCurrentStep();
            IFilter filter = new IFilter(parent);

            if (parent.GetCurrentStep() == null) return;


            IODBLayer layerTopPins = null;
            IODBLayer layerbotPins = null;


            if (layerTopPins == null)
            {
                layerTopPins = filter.CreateEmptyODBLayer("component_top_shapes", step.Name);
                if (clearLayer)
                {
                    foreach (IODBObject o in layerTopPins.GetAllLayerObjects())
                    {
                        layerTopPins.RemoveObject(o);
                    }
                }
            }
            if (layerbotPins == null)
            {
                layerbotPins = filter.CreateEmptyODBLayer("component_bot_shapes", step.Name);
                if (clearLayer)
                {
                    foreach (IODBObject o in layerbotPins.GetAllLayerObjects())
                    {
                        layerbotPins.RemoveObject(o);
                    }
                }
            }

            foreach (ICMPObject cmp in step.GetAllCMPObjects())
            {
                PCBI.MathUtils.IPolyClass CMP_Poly = cmp.GetPolygonOutline(false);

                IODBObject outlinePolygon;
                if (cmp.PlacedTop)
                {
                    outlinePolygon = filter.CreatePolygon(layerTopPins);
                }
                else
                {
                    outlinePolygon = filter.CreatePolygon(layerbotPins);
                }
                ISurfaceSpecifics CMP_Body_Polygon = (ISurfaceSpecifics)outlinePolygon.GetSpecifics();
                bool polyStart = true;
                foreach (IEdge edge in CMP_Poly.GetEdges())
                {
                    if (polyStart)
                    {
                        polyStart = false;
                        CMP_Body_Polygon.StartPolygon(false, new PointF((float)edge.Begin.X, (float)edge.Begin.Y));

                    }
                    else
                        if (edge is IArcEdge)
                        {
                            IArcEdge aEdge = (IArcEdge)edge;
                            PointF start = new PointF((float)aEdge.Begin.X, (float)aEdge.Begin.Y);
                            PointF end = new PointF((float)aEdge.End.X, (float)aEdge.End.Y);
                            PointF center = new PointF((float)aEdge.Center.X, (float)aEdge.Center.Y);
                            CMP_Body_Polygon.AddArc(start, end, center, aEdge.ClockWise);
                        }
                        else if (edge is ILineEdge)
                        {
                            ILineEdge aEdge = (ILineEdge)edge;
                            PointF start = new PointF((float)aEdge.Begin.X, (float)aEdge.Begin.Y);
                            PointF end = new PointF((float)aEdge.End.X, (float)aEdge.End.Y);
                            CMP_Body_Polygon.AddLine(start, end);
                        }
                }
                CMP_Body_Polygon.EndPolygon(); //close the new contour
                outlinePolygon.SetSpecifics(CMP_Body_Polygon);
            }
            layerbotPins.EnableLayer(true);
            layerTopPins.EnableLayer(true);
            parent.UpdateView();
            parent.UpdateControlsAndResetView();
		}
				
    }
}
