//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Create polygon out of selectet lines and arcs.
//-----------------------------------------------------------------------------------
// The script creates a polygon from a selection of lines and arcs in PCB Investigator.
// It creates a new empty ODB layer "polygons_n" and then adds all selected arcs and lines one by one to create the polygon.
// The polygon is then added as a new ODB object on the "polygons_n" layer.
// If the polygon has multiple parts, each part is added separately.
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
            IFilter filter = new IFilter(parent);
            IODBLayer layerPolygons = filter.CreateEmptyODBLayer("polygons_n", step.Name);

            bool polyStart = true;
            List<IODBObject> listOfSelection = step.GetSelectedElements();

            PCBI.MathUtils.IPolyClass poly = new PCBI.MathUtils.IPolyClass();

            foreach (IODBObject obj in listOfSelection)
            {
                IObjectSpecificsD os = obj.GetSpecificsD();

                if (os.GetType() == typeof(IArcSpecificsD))
                {
                    IArcSpecificsD aEdge = (IArcSpecificsD)os;

                    if (polyStart)
                    {
                        polyStart = false;
                    }
                    poly.AddEdge(aEdge.Start, aEdge.End, aEdge.Center, aEdge.ClockWise);
                }
                else if (os.GetType() == typeof(ILineSpecificsD))
                {
                    ILineSpecificsD aEdge = (ILineSpecificsD)os;
                    if (polyStart)
                    {
                        polyStart = false;
                    }
                    poly.AddEdge(aEdge.Start, aEdge.End);

                }
            }

                if (poly.GetSubPolygons().Count > 0)
                {
                    foreach (PCBI.MathUtils.IPolyClass polyC in poly.GetSubPolygons())
                    {

                        if (polyC.GetBounds().Width > 0.001 && polyC.GetBounds().Height > 0.001)
                        {
                            IODBObject surfaceFromPoly = polyC.GetSurfaceFromPolygon(layerPolygons);
                        }
                    }
                    layerPolygons.EnableLayer(true);
                }
                else
                {
                    IODBObject suf = poly.GetSurfaceFromPolygon(layerPolygons);
                    layerPolygons.EnableLayer(true);
                }

            parent.UpdateView();
            IMatrix matrix = parent.GetMatrix();
            matrix.UpdateDataAndList();
		}
		
    }
}