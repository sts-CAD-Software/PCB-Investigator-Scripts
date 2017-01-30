//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 30.01.2017
// Autor Fabio.Gruber
// 
// Script to create copy layer with cutted elements on profile outline.
//-----------------------------------------------------------------------------------
// GUID newScript_636213806757650301

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

		public void Execute(IPCBIWindow parentPCBI)
		{
		      if (parentPCBI == null) { MessageBox.Show("No license!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); return; }

            if (!parentPCBI.JobIsLoaded) return;
            IStep curStep = parentPCBI.GetCurrentStep();
            if (curStep == null) return; //no step -> no job -> break;
IFilter filter = new IFilter(parentPCBI);
            
foreach(ILayer layer in curStep.GetActiveLayerList())
{
            
            IODBLayer newLayer = filter.CreateEmptyODBLayer("newLayerWithCutting_"+layer.GetLayerName(), curStep.Name, false);
            IPolyClass outlinePoly = curStep.GetPCBOutlinePoly();

 			 if ((!(layer is IODBLayer)) || !CutRectangle(outlinePoly, (IODBLayer)layer, newLayer, filter, parentPCBI))
                MessageBox.Show("Something is wrong, cutting has not worked!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
			
            parentPCBI.UpdateView(); 
            IMatrix matrix = parentPCBI.GetMatrix();
            matrix.UpdateDataAndList();
		}
		      internal static bool CutRectangle(IPolyClass rectanglePoly, IODBLayer layer, IODBLayer NewlayerToAddElements, IFilter filter, IPCBIWindow parentPCBI)
        {
            if (layer == null) return false; //no layer
            if (NewlayerToAddElements == null) return false; //no layer for new elements

            //use bounds to get relevant objects
            //cut intersecting objects

            //helper for Symbols
            Dictionary<int, int> symbolUsed = new Dictionary<int, int>();
            //Performance
            RectangleD relevantRectangle = rectanglePoly.GetBounds();

            foreach (IODBObject obj in layer.GetAllLayerObjects())
            {
                if (relevantRectangle.IntersectsWith(obj.GetBoundsD()))
                {
                    #region cut elements
                    //cutting
                    IPolyClass polygon = obj.GetPolygonOutline();

                    IPolyClass cuttingPolygon;
                    if (rectanglePoly.GetBounds().Contains(polygon.GetBounds()))
                    { cuttingPolygon = polygon; }
                    else
                    {   cuttingPolygon = polygon.Intersect(rectanglePoly);}

                    if (cuttingPolygon.GetEdgeCount() == 0) continue; //bounds cutting but not of any interest

                     List<ISurfaceSpecificsD> subPolygons = cuttingPolygon.SplitInIsleAndHoles(parentPCBI, NewlayerToAddElements);

                    foreach (ISurfaceSpecificsD subPolygon in subPolygons) //create part surfaces
                    {
                        IODBObject surface = filter.CreatePolygon(NewlayerToAddElements);
                        subPolygon.Positive = obj.Positive;

                        surface.SetSpecifics(subPolygon);
                        Dictionary<string, string> attributes = obj.GetAttributesDictionaryStringKeys();
                        foreach (string attribute in attributes.Keys)
                        {
                            surface.SetAttribute(attributes[attribute], attribute); //copy attributes 
                        }
                    } 
                    #endregion
                }
                else 
                {
                    //not relevant -> ignore
                }
            }

            return true; //finish
        }
  
    }
}
