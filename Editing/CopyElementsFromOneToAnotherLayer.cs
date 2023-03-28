//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 15.06.2016
// Autor Fabio.Gruber
// 
// Copy all objects from first active layer to second active layer.
// You can change this in the first foreach to e.g. fix layer names or make your own dialog to select the layer to copy...
// This script works from PCB-Investigator version 7.9 up!
//-----------------------------------------------------------------------------------
// The script copies the objects from the first layer to the second layer and adapts them to the properties of the second layer.
// Objects such as lines, arcs, pads, polygons and texts can be copied.
// The script uses an IFilter interface to create and apply tool definitions to adjust the properties of the objects.
// The class is relevant to potential PCB software customers who can use the script to automate designs faster and easier. 
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
            IMatrix matrix = parent.GetMatrix();
            if (step == null || matrix == null) return;

            IODBLayer IODBLayerOrg = null;
            IODBLayer IODBLayerNew = null;
            foreach (ILayer activeLayer in step.GetActiveLayerList()) //search for the first two active layer to copy from first to second
            {
                if (activeLayer is IODBLayer)
                {
                    if (IODBLayerOrg == null) { IODBLayerOrg = (IODBLayer)activeLayer; }
                    else if (IODBLayerNew == null) { IODBLayerNew = (IODBLayer)activeLayer; }
                    else
                        break; //only the first two layers...
                }
            }
            if (IODBLayerOrg == null || IODBLayerNew == null) return;

            IFilter filter = new IFilter(parent);

            if (IODBLayerOrg is IODBLayer)
            {
                Dictionary<int, int> ShapeIndexOldNew = new Dictionary<int, int>(); //set new shape index if symbol is created.

                foreach (IODBObject obj in IODBLayerOrg.GetAllLayerObjects())
                {
                    IODBObject objNew = null;
                    IObjectSpecificsD spec = obj.GetSpecificsD();
                    switch (obj.Type)
                    {
                        case IObjectType.Arc:
                            objNew = filter.CreateArc(IODBLayerNew);
                            if (!ShapeIndexOldNew.ContainsKey(((IArcSpecificsD)spec).ShapeIndex))
                            {
                                int indexNew = IFilter.AddToolFromODBString(IODBLayerNew, ((IArcSpecificsD)spec).ODBSymbol_String);
                                ShapeIndexOldNew.Add(((IArcSpecificsD)spec).ShapeIndex, indexNew);
                            }
                            ((IArcSpecificsD)spec).ShapeIndex = ShapeIndexOldNew[((IArcSpecificsD)spec).ShapeIndex];
                            break;
                        case IObjectType.Line:
                            objNew = filter.CreateLine(IODBLayerNew);
                            if (!ShapeIndexOldNew.ContainsKey(((ILineSpecificsD)spec).ShapeIndex))
                            {
                                int indexNew = IFilter.AddToolFromODBString(IODBLayerNew, ((ILineSpecificsD)spec).ODBSymbol_String);
                                ShapeIndexOldNew.Add(((ILineSpecificsD)spec).ShapeIndex, indexNew);
                            }
                            ((ILineSpecificsD)spec).ShapeIndex = ShapeIndexOldNew[((ILineSpecificsD)spec).ShapeIndex];
                            break;
                        case IObjectType.Pad:
                            objNew = filter.CreatePad(IODBLayerNew);
                            if (!ShapeIndexOldNew.ContainsKey(((IPadSpecificsD)spec).ShapeIndex))
                            {
                                IFilter.ToolDefinition toolDef = filter.GetSymbolByShapeIndex(((IPadSpecificsD)spec).ShapeIndex, (IODBLayer)IODBLayerOrg);
                                int indexNew = -1;
                                if (toolDef.Type == PCBI.Symbol_Type.special)
                                {
                                    indexNew = IFilter.AddToolDefinitionSpecial(IODBLayerNew, (IODBLayer)IODBLayerOrg, toolDef.ShapeIndex);
                                }
                                else
                                {
                                    indexNew = IFilter.AddToolFromODBString(IODBLayerNew, ((IPadSpecificsD)spec).ODBSymbol_String);
                                }
                                ShapeIndexOldNew.Add(((IPadSpecificsD)spec).ShapeIndex, indexNew);
                            }
                            ((IPadSpecificsD)spec).ShapeIndex = ShapeIndexOldNew[((IPadSpecificsD)spec).ShapeIndex];
                            break;
                        case IObjectType.Surface:
                            objNew = filter.CreatePolygon(IODBLayerNew);
                            break;
                        case IObjectType.Text:
                            objNew = filter.CreateText(IODBLayerNew);
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("Case not possible!");
                            break;
                    }

                    if (objNew != null)
                        objNew.SetSpecifics(spec);
                }
            }
            else
                System.Diagnostics.Debug.WriteLine("Not implemented for components!");
            parent.UpdateView();
        }
    }
}