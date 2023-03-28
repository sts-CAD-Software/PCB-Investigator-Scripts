//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 18.01.2017
// Autor Fabio.Gruber
// 
// Create a testpoint overview.
//-----------------------------------------------------------------------------------
// GUID newScript_636153340204189979
//
// This script creates a test point at all drill holes on the upper or lower side of the PCB, depending on the selected side.
// The positions of the test points are arranged in a 500 x 500 grid. The grid is represented by horizontal and vertical lines on which X and Y texts are displayed.
// The script also deletes existing "testpoint_locations_top" and "testpoint_locations_bot" layers if the useFlattenStep variable is set to false. 
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
            string PackageName = "TESTPUNKT";
            bool topSide = true;
            bool useFlattenStep = true;

            if (!parent.JobIsLoaded) return;
            IFilter filter = new IFilter(parent);
            IStep step = parent.GetCurrentStep();
            IMatrix matrix = parent.GetMatrix();
            if (matrix == null) return;

            step.TurnOffAllLayer();
            step.ClearSelection();
            //nicht für flatten step, da durch Threads zu langsam und dann evtl die neuen Daten wieder gelöscht werden!
            if (!useFlattenStep)
            {
                matrix.DelateLayer("testpoint_locations_top", false);
                matrix.DelateLayer("testpoint_locations_bot", false);
            }
            if (topSide)  CreateForOneSide(true, step, filter, PackageName, matrix.GetAllDrillLayersForThisLayer(matrix.GetTopSignalLayer()));
            else CreateForOneSide(false, step, filter, PackageName, matrix.GetAllDrillLayersForThisLayer(matrix.GetBotSignalLayer()));

            //gitter 500x500

            IODBLayer rasterLayer = filter.CreateEmptyODBLayer("raster", step.Name);
            int shapeIndex = IFilter.AddToolDefinitionRound(rasterLayer, 10);
            RectangleF boundsStep = step.GetBounds();

            int countYLines = (int)(boundsStep.Height / 500) + 1;
            int countXLines = (int)(boundsStep.Width / 500) + 1;

            for (int y = 0; y <= countYLines; y++)
            {
                for (int x = 0; x <= countXLines; x++)
                {
                    for (int i = 0; i < 5; i++) //5 sublines
                    {
                        int yVal = y * 500;
                        int xVal = x * 500;
                        if (x < countXLines)
                            CreateHorLine(filter, rasterLayer, shapeIndex, yVal, xVal + i * 100 + 25);
                        if (y < countYLines)
                            CreateVertLine(filter, rasterLayer, shapeIndex, yVal + i * 100 + 25, xVal);
                    }
                }
            }

            for (int y = 0; y < countYLines; y++)
            {
                #region draw text
                IODBObject textForPad = filter.CreateText(rasterLayer);
                ITextSpecificsD text = (ITextSpecificsD)textForPad.GetSpecificsD();

                text.Text = "Y" + (y + 1);
                text.TextSize = new SizeF(50, 80);
                text.Location = new PCBI.MathUtils.PointD(-120, y * 500 + 200);
                text.WidthFactor = 1;
                textForPad.SetSpecifics(text);

                //right side
                IODBObject textForPad2 = filter.CreateText(rasterLayer);
                ITextSpecificsD text2 = (ITextSpecificsD)textForPad.GetSpecificsD();

                text2.Text = text.Text;
                text2.TextSize = new SizeF(50, 80);
                text2.Location = new PCBI.MathUtils.PointD(countXLines * 500 + 120, y * 500 + 200);
                text2.WidthFactor = 1;
                textForPad2.SetSpecifics(text2);
            }
            for (int x = 0; x < countXLines; x++)
            {
                IODBObject textForPad = filter.CreateText(rasterLayer);
                ITextSpecificsD text = (ITextSpecificsD)textForPad.GetSpecificsD();

                text.Text = "X" + (x + 1);
                text.TextSize = new SizeF(50, 80);
                text.Location = new PCBI.MathUtils.PointD(x * 500 + 200, -100);
                text.Rotation = -90;
                text.WidthFactor = 1;
                textForPad.SetSpecifics(text);

                //top side
                IODBObject textForPad2 = filter.CreateText(rasterLayer);
                ITextSpecificsD text2 = (ITextSpecificsD)textForPad.GetSpecificsD();

                text2.Text = text.Text;
                text2.TextSize = new SizeF(50, 80);
                text2.Location = new PCBI.MathUtils.PointD(x * 500 + 300, countYLines * 500 + 100);
                text2.Rotation = 90;
                text2.WidthFactor = 1;
                textForPad2.SetSpecifics(text2);
                #endregion
            }

            foreach (string drillName in matrix.GetAllDrillLayerNames())
            {
                IODBLayer drillLayer = (IODBLayer)step.GetLayer(drillName);

                foreach (IODBObject drill in drillLayer.GetAllLayerObjects())
                {
                    Dictionary<PCBI.FeatureAttributeEnum, string> attribs = drill.GetAttributesDictionary();
                   
                    if (drill.Type == IObjectType.Pad && attribs.ContainsKey(PCBI.FeatureAttributeEnum.drill) && attribs.ContainsKey(PCBI.FeatureAttributeEnum.geometry))
                    {
                        if (attribs[PCBI.FeatureAttributeEnum.drill].ToUpperInvariant() == "NON_PLATED" && attribs[PCBI.FeatureAttributeEnum.geometry].ToUpperInvariant() == "ICTAUFNAHME")
                        {
                            IODBObject drillMarker = filter.CreatePad(rasterLayer);
                            IPadSpecificsD drillSpec = (IPadSpecificsD)drill.GetSpecificsD();

                            int shapeIndexNew = IFilter.AddToolDefinitionRound(rasterLayer, (float)drillSpec.Diameter);
                            drillSpec.ShapeIndex = shapeIndexNew;
                            drillMarker.SetSpecifics(drillSpec);
                        }
                    }
                }
            }

            matrix.UpdateDataAndList();

            if (!useFlattenStep)
                parent.UIAction.Execute(ID_ActionItem.ID_PRINT_PREVIEW);
        }
        private static void CreateHorLine(IFilter filter, IODBLayer rasterLayer, int shapeIndex, int y, int x)
        {
            IODBObject rasterLine = filter.CreateLine(rasterLayer);
            ILineSpecificsD line = (ILineSpecificsD)rasterLine.GetSpecificsD();

            line.Start = new PCBI.MathUtils.PointD(x, y);
            line.End = new PCBI.MathUtils.PointD(x + 50, y);
            line.ShapeIndex = shapeIndex;

            rasterLine.SetSpecifics(line);
        }
        private static void CreateVertLine(IFilter filter, IODBLayer rasterLayer, int shapeIndex, int y, int x)
        {
            IODBObject rasterLine = filter.CreateLine(rasterLayer);
            ILineSpecificsD line = (ILineSpecificsD)rasterLine.GetSpecificsD();

            line.Start = new PCBI.MathUtils.PointD(x, y);
            line.End = new PCBI.MathUtils.PointD(x, y + 50);
            line.ShapeIndex = shapeIndex;

            rasterLine.SetSpecifics(line);
        }
        void CreateForOneSide(bool top, IStep step, IFilter filter, string PackageName, List<string> DrillLayers)
        {
            ICMPLayer compLayer = step.GetCMPLayer(top);
            if (compLayer == null) return;

            IODBLayer newLayer = filter.CreateEmptyODBLayer("testpoint_locations_" + (top ? "top" : "bot"), step.Name);
            if (newLayer == null) return;

            List<IODBLayer> allDrillLayers = new List<IODBLayer>();
            foreach (string drillName in DrillLayers)
                allDrillLayers.Add((IODBLayer)step.GetLayer(drillName));

            int shapeIndex = IFilter.AddToolDefinitionRound(newLayer, 75);
            int shapeIndexConnection = IFilter.AddToolDefinitionRound(newLayer, 1);

            foreach (ICMPObject cmp in compLayer.GetAllLayerObjects())
            {
                if (!cmp.UsedPackageName.Contains( PackageName)) continue;

                IODBObject markerPad = filter.CreatePad(newLayer);
                IPadSpecificsD pad = (IPadSpecificsD)markerPad.GetSpecificsD();

                pad.Location = new PCBI.MathUtils.PointD(cmp.Position);
                pad.ShapeIndex = shapeIndex;

                markerPad.SetSpecifics(pad);
                markerPad.ObjectColor = Color.Green;
                markerPad.SetAttribute("Steel needle <BST> (Testpoint)");
                bool special = false;
                foreach (IODBLayer drillLayer in allDrillLayers)
                {
                    #region check drills
                    foreach (IODBObject drill in drillLayer.GetAllObjectsOnPosition(cmp.Position))
                    {
                        Dictionary<PCBI.FeatureAttributeEnum, string> attribs = drill.GetAttributesDictionary();

                        if (attribs.ContainsKey(PCBI.FeatureAttributeEnum.drill))
                        {
                            if (attribs[PCBI.FeatureAttributeEnum.drill].ToUpperInvariant() == "VIA")
                            {
                                markerPad.ObjectColor = Color.Blue;
                                markerPad.SetAttribute("Pyramid <H> (Via)");
                                special = true;
                                break;
                            }
                        }
                    }
                    if (special) break;
                    #endregion
                }
                if (!special)
                {
                    //check for component pin
                    foreach (ICMPObject comp in compLayer.GetAllObjectsOnPosition(cmp.Position))
                    {
                        if (comp == cmp) continue; //testpunkt selbst wird ignoriert

                        foreach (IPin pin in comp.GetPinList())
                        {
                            PCBI.MathUtils.IPolyClass cmpPoly = pin.GetPolygonOutline(comp);
                            if (cmpPoly.PointInPolygon(pad.Location))
                            {
                                markerPad.ObjectColor = Color.Red;
                                markerPad.SetAttribute("Serrated <C>"); //hier evtl noch überprüfen ob pin bzw. body drüber liegt?
                                special = true;                            
                                break;
                            }
                        }
                        if (special) break;
                    }
                }
            }
            foreach (ICMPObject cmp in compLayer.GetAllLayerObjects()) //neue schleife da erst alle pads plaziert werden sollen!
            {
                if (!cmp.UsedPackageName.Contains(PackageName)) continue;

                IODBObject textForPad = filter.CreateText(newLayer);
                ITextSpecificsD text = (ITextSpecificsD)textForPad.GetSpecificsD();

                text.Text = cmp.Ref.Remove(0, 2); //Annahme das alle mit TP beginnen
                text.TextSize = new SizeF(25, 50);
                text.Location = new PCBI.MathUtils.PointD(cmp.Position.X + 50, cmp.Position.Y - 10);
                text.WidthFactor = 0.6;
                textForPad.SetSpecifics(text);
                textForPad.ObjectColor = Color.DarkGray;

                //text location should not be intersecting!
                List<IObject> otherObjectsOnSameLocation = newLayer.GetAllObjectInRectangle(textForPad.GetBoundsD());
                int offset = 50;
                bool horChecked = false;
                while (otherObjectsOnSameLocation.Count > 1)
                {
                    //move text
                    if (horChecked)
                        text.Location = new PCBI.MathUtils.PointD(cmp.Position.X, cmp.Position.Y + offset);
                    else
                        text.Location = new PCBI.MathUtils.PointD(cmp.Position.X - offset - textForPad.GetBoundsD().Width, cmp.Position.Y - 10);
                    offset += 50;
                    horChecked = true;
                    textForPad.SetSpecifics(text);
                    otherObjectsOnSameLocation = newLayer.GetAllObjectInRectangle(textForPad.GetBoundsD());
                }

                IODBObject connectionLine = filter.CreateLine(newLayer);
                ILineSpecificsD line = (ILineSpecificsD)connectionLine.GetSpecificsD();

                line.ShapeIndex = shapeIndexConnection;
                line.Start = new PCBI.MathUtils.PointD(cmp.Position);
                line.End = new PCBI.MathUtils.PointD(text.Location.X, text.Location.Y + 25);
                connectionLine.SetSpecifics(line);
                connectionLine.ObjectColor = Color.LightGray;
            }
        }

    }
}