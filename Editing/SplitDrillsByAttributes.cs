// PCB-Investigator Automation Script
// Created on 2016-02-17
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Split Drills depending on attributes.
//-----------------------------------------------------------------------------------
// The script is used to split different drill layers into separate ODB++ files.
// The different drill attributes like "plated" or "via" are taken into account and the objects are sorted accordingly.
// The script then creates new drill layers with the sorted objects and saves them in separate ODB++ files.
// If all objects are of the same type, no splitting is performed.
// It uses a method to create new ODB++ layers and saves the objects in these layers. 
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
using System.Reflection;

namespace PCBIScript
{
	public class PScript : IPCBIScript
	{
		public PScript()
		{
		}

		public void Execute(IPCBIWindow parent)
		{
			bool withRoutChecked = true;
                
             SplitDrillWorker worker = new SplitDrillWorker();
             worker.doWork(parent, withRoutChecked, false);
		}
		   
   class SplitDrillWorker
    {
        List<IODBObject> drill_ = new List<IODBObject>();

        List<IODBObject> drillPlatedNormal = new List<IODBObject>();
        List<IODBObject> drillNonPlatedNormal = new List<IODBObject>();
        List<IODBObject> drillViaNormal = new List<IODBObject>();

        List<IODBObject> drillRout = new List<IODBObject>();
        List<IODBObject> drillPlatedRout = new List<IODBObject>();
        List<IODBObject> drillNonPlatedRout = new List<IODBObject>();
        List<IODBObject> drillViaRout = new List<IODBObject>();

        bool withRout = false;
        bool activateNewCreatedLayer = false;
        bool skipLayerWasAlreadySplitted = false;

        bool noAttributesPadsLayerNameCreated = false;
        bool noAttributesNotPadsLayerNameCreated = false;

        bool platedLayerNameCreatedRout = false;
        bool nonPlatedLayerNameCreatedRout = false;
        bool viaLayerNameCreatedRout = false;

        bool platedLayerNameCreatedNormal = false;
        bool nonPlatedLayerNameCreatedNormal = false;
        bool viaLayerNameCreatedNormal = false;

        internal void doWork(IPCBIWindow parent, bool splitRoutItem, bool activateNewLayer)
        {
            IStep step = parent.GetCurrentStep();
            IMatrix matrix = parent.GetMatrix();
            IFilter filter = new IFilter(parent);

            //if true the new layer need to be activated
            activateNewCreatedLayer = activateNewLayer;

            //sets the bool if rout should be used
            withRout = splitRoutItem;

            foreach (string currLayerName in matrix.GetAllDrillLayerNames())
            {
                Dictionary<string, List<IODBObject>> allNewLayerDict = new Dictionary<string, List<IODBObject>>();

                drill_ = new List<IODBObject>();

                drillPlatedNormal = new List<IODBObject>();
                drillNonPlatedNormal = new List<IODBObject>();
                drillViaNormal = new List<IODBObject>();

                drillRout = new List<IODBObject>();
                drillPlatedRout = new List<IODBObject>();
                drillNonPlatedRout = new List<IODBObject>();
                drillViaRout = new List<IODBObject>();

                noAttributesPadsLayerNameCreated = false;
                noAttributesNotPadsLayerNameCreated = false;

                platedLayerNameCreatedRout = false;
                nonPlatedLayerNameCreatedRout = false;
                viaLayerNameCreatedRout = false;

                platedLayerNameCreatedNormal = false;
                nonPlatedLayerNameCreatedNormal = false;
                viaLayerNameCreatedNormal = false;

                //only activate layer can be splitted
                if (activateNewCreatedLayer)
                {
                    if (!step.GetActiveLayerList().Contains(step.GetLayer(currLayerName)))
                        continue;
                }
                
                //checks if the layer is a splitted layer
                if ((currLayerName.Length > 7 && currLayerName.Substring(currLayerName.Length - 7, 7).Equals("_plated")) || (currLayerName.Length > 4 && currLayerName.Substring(currLayerName.Length - 4, 4).Equals("_via")))
                {
                    continue;
                }

                //checks if the new layer wasn't already splitted last time
                foreach (string allOtherLayerName in matrix.GetAllDrillLayerNames())
                {
                    if (activateNewCreatedLayer)
                    {
                        string searchedLayerName = currLayerName.ToLower() + "_plated";
                        if (allOtherLayerName.ToLower().Equals(searchedLayerName))
                            step.GetLayer(searchedLayerName).EnableLayer(true);
                        searchedLayerName = currLayerName.ToLower() + "_non_plated";
                        if (allOtherLayerName.ToLower().Equals(searchedLayerName))
                            step.GetLayer(searchedLayerName).EnableLayer(true);
                        searchedLayerName = currLayerName.ToLower() + "_via";
                        if (allOtherLayerName.ToLower().Equals(searchedLayerName))
                            step.GetLayer(searchedLayerName).EnableLayer(true);
                    }
                    if (allOtherLayerName.ToLower().Equals(currLayerName.ToLower() + "_plated") || allOtherLayerName.ToLower().Equals(currLayerName.ToLower() + "_non_plated") || allOtherLayerName.ToLower().Equals(currLayerName.ToLower() + "_via"))
                    {
                        skipLayerWasAlreadySplitted = true;
                        continue;
                    }
                }

                //if it was already splitted then skip it
                if (skipLayerWasAlreadySplitted)
                {
                    skipLayerWasAlreadySplitted = false;
                    continue;
                }

                //checks if layer is a drilllayer
                if (matrix.GetMatrixLayerType(currLayerName) == MatrixLayerType.Drill)
                {
                    ILayer lay = step.GetLayer(currLayerName);
                    List<IObject> objects = lay.GetAllLayerObjects();
                    foreach (IODBObject currObj in objects)
                    {
                        Dictionary<PCBI.FeatureAttributeEnum, string> objDict = currObj.GetAttributesDictionary();
                        if (objDict.Count != 0)
                        {
                            if (objDict.ContainsKey(PCBI.FeatureAttributeEnum.drill))
                            {
                                if (currObj.Type == IObjectType.Pad)
                                {
                                    #region Rout
                                    if (withRout)
                                    {
                                        if (objDict[PCBI.FeatureAttributeEnum.drill] == "non_plated")
                                        {
                                            drillNonPlatedNormal.Add(currObj);
                                            if (!nonPlatedLayerNameCreatedNormal)
                                            {
                                                allNewLayerDict.Add(currLayerName + "_non_plated", drillNonPlatedNormal);
                                                nonPlatedLayerNameCreatedNormal = true;
                                            }
                                        }
                                        else if (objDict[PCBI.FeatureAttributeEnum.drill] == "plated")
                                        {
                                            drillPlatedNormal.Add(currObj);
                                            if (!platedLayerNameCreatedNormal)
                                            {
                                                allNewLayerDict.Add(currLayerName + "_plated", drillPlatedNormal);
                                                platedLayerNameCreatedNormal = true;
                                            }
                                        }
                                        else if (objDict[PCBI.FeatureAttributeEnum.drill] == "via")
                                        {
                                            drillViaNormal.Add(currObj);
                                            if (!viaLayerNameCreatedNormal)
                                            {
                                                allNewLayerDict.Add(currLayerName + "_via", drillViaNormal);
                                                viaLayerNameCreatedNormal = true;
                                            }
                                        }
                                        else
                                        {
                                            drill_.Add(currObj);
                                            if (!noAttributesPadsLayerNameCreated)
                                            {
                                                allNewLayerDict.Add(currLayerName + "_", drill_);
                                                noAttributesPadsLayerNameCreated = true;
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Without Rout
                                    else
                                    {
                                        if (objDict[PCBI.FeatureAttributeEnum.drill] == "non_plated")
                                        {
                                            drillNonPlatedRout.Add(currObj);
                                            if (!nonPlatedLayerNameCreatedRout)
                                            {
                                                allNewLayerDict.Add(currLayerName + "_non_plated_rout", drillNonPlatedRout);
                                                nonPlatedLayerNameCreatedRout = true;
                                            }
                                        }
                                        else if (objDict[PCBI.FeatureAttributeEnum.drill] == "plated")
                                        {
                                            drillPlatedRout.Add(currObj);
                                            if (!platedLayerNameCreatedRout)
                                            {
                                                allNewLayerDict.Add(currLayerName + "_plated_rout", drillPlatedRout);
                                                platedLayerNameCreatedRout = true;
                                            }
                                        }
                                        else if (objDict[PCBI.FeatureAttributeEnum.drill] == "via")
                                        {
                                            drillViaRout.Add(currObj);
                                            if (!viaLayerNameCreatedRout)
                                            {
                                                allNewLayerDict.Add(currLayerName + "_via_rout", drillViaRout);
                                                viaLayerNameCreatedRout = true;
                                            }
                                        }
                                        else
                                        {
                                            drill_.Add(currObj);
                                            if (!noAttributesPadsLayerNameCreated)
                                            {
                                                allNewLayerDict.Add(currLayerName + "_", drill_);
                                                noAttributesPadsLayerNameCreated = true;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else if (withRout)
                                {
                                    #region Line-Arcs-Surfaces-Text + Rout
                                    if (objDict[PCBI.FeatureAttributeEnum.drill] == "non_plated")
                                    {
                                        drillNonPlatedRout.Add(currObj);
                                        if (!nonPlatedLayerNameCreatedRout)
                                        {
                                            allNewLayerDict.Add("rout_" + currLayerName + "_non_plated", drillNonPlatedRout);
                                            nonPlatedLayerNameCreatedRout = true;
                                        }
                                    }
                                    else if (objDict[PCBI.FeatureAttributeEnum.drill] == "plated")
                                    {
                                        drillPlatedRout.Add(currObj);
                                        if (!platedLayerNameCreatedRout)
                                        {
                                            allNewLayerDict.Add("rout_" + currLayerName + "_plated", drillPlatedRout);
                                            platedLayerNameCreatedRout = true;
                                        }
                                    }
                                    else if (objDict[PCBI.FeatureAttributeEnum.drill] == "via")
                                    {
                                        drillViaRout.Add(currObj);
                                        if (!viaLayerNameCreatedRout)
                                        {
                                            allNewLayerDict.Add("rout_" + currLayerName + "_via", drillViaRout);
                                            viaLayerNameCreatedRout = true;
                                        }
                                    }
                                    else
                                    {
                                        drillRout.Add(currObj);
                                        if (!noAttributesNotPadsLayerNameCreated)
                                        {
                                            allNewLayerDict.Add("rout_" + currLayerName, drillRout);
                                            noAttributesNotPadsLayerNameCreated = true;
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        else
                        {
                            #region No Attributes + Rout
                            if (withRout)
                            {
                                if (currObj.Type == IObjectType.Pad)
                                {
                                    drill_.Add(currObj);
                                    if (!noAttributesPadsLayerNameCreated)
                                    {
                                        allNewLayerDict.Add(currLayerName + "_", drill_);
                                        noAttributesPadsLayerNameCreated = true;
                                    }
                                }
                                else
                                {
                                    drillRout.Add(currObj);
                                    if (!noAttributesNotPadsLayerNameCreated)
                                    {
                                        allNewLayerDict.Add("rout_" + currLayerName, drillRout);
                                        noAttributesNotPadsLayerNameCreated = true;
                                    }
                                }
                            }
                            #endregion
                            #region No Attributes Without Rout
                            else
                            {
                                if (currObj.Type == IObjectType.Pad)
                                {
                                    drill_.Add(currObj);
                                    if (!noAttributesPadsLayerNameCreated)
                                    {
                                        allNewLayerDict.Add(currLayerName + "_", drill_);
                                        noAttributesPadsLayerNameCreated = true;
                                    }
                                }
                            }
                            #endregion
                        }
                    }

                    if (allNewLayerDict.Count > 1) //wenn alle vom gleichen Typ sind muss nicht gesplittet werden!
                    {
                        foreach (string currNewLayerName in allNewLayerDict.Keys)
                        {
                            filter = new IFilter(parent);
                            CreateNewDrillODBLayer(filter, currNewLayerName, parent, allNewLayerDict[currNewLayerName], activateNewCreatedLayer);
                        }
                    }
                }
            }
            matrix.UpdateDataAndList();
        }

        private void CreateNewDrillODBLayer(PCBI.Automation.IFilter filter, string newLayerName, IPCBIWindow parent, List<IODBObject> currIODBObjectList, bool activateLayer)
        {
            if (currIODBObjectList.Count == 0)
                return;

            IODBLayer layer = filter.CreateEmptyODBLayer(newLayerName, parent.GetCurrentStep().Name);
            Dictionary<string, int> shapeList = new Dictionary<string, int>();
            foreach (IODBObject obj in currIODBObjectList)
            {
                if (obj.Type == IObjectType.Pad)
                {
                    IPadSpecificsD specPad = (IPadSpecificsD)obj.GetSpecificsD();
                    if (!shapeList.ContainsKey(specPad.ODBSymbol_String))
                    {
                        int newShapeIndex = IFilter.AddToolFromODBString(layer, specPad.ODBSymbol_String, shapeList.Count);
                        shapeList.Add(specPad.ODBSymbol_String, newShapeIndex);
                    }
                    IODBObject pad = filter.CreatePad(layer);
                    IPadSpecificsD padInfosD = (IPadSpecificsD)obj.GetSpecificsD();
                    padInfosD.ShapeIndex = shapeList[specPad.ODBSymbol_String];
                    pad.SetSpecifics(padInfosD, shapeList[specPad.ODBSymbol_String]);
                }
                else if (obj.Type == IObjectType.Line)
                {
                    ILineSpecificsD specLine = (ILineSpecificsD)obj.GetSpecificsD();
                    if (!shapeList.ContainsKey(specLine.ODBSymbol_String))
                    {
                        int newShapeIndex = IFilter.AddToolFromODBString(layer, specLine.ODBSymbol_String, shapeList.Count);
                        shapeList.Add(specLine.ODBSymbol_String, newShapeIndex);
                    }
                    IODBObject line = filter.CreateLine(layer);
                    ILineSpecificsD lineSpecificsD = (ILineSpecificsD)obj.GetSpecificsD();
                    lineSpecificsD.ShapeIndex = shapeList[specLine.ODBSymbol_String];
                    line.SetSpecifics(lineSpecificsD);
                }
                else if (obj.Type == IObjectType.Arc)
                {
                    IArcSpecificsD specArc = (IArcSpecificsD)obj.GetSpecificsD();
                    if (!shapeList.ContainsKey(specArc.ODBSymbol_String))
                    {
                        int newShapeIndex = IFilter.AddToolFromODBString(layer, specArc.ODBSymbol_String, shapeList.Count);
                        shapeList.Add(specArc.ODBSymbol_String, newShapeIndex);
                    }
                    IODBObject arc = filter.CreateArc(layer);
                    IArcSpecificsD specificsArcD = (IArcSpecificsD)obj.GetSpecificsD();
                    specificsArcD.ShapeIndex = shapeList[specArc.ODBSymbol_String];
                    arc.SetSpecifics(specificsArcD);
                }
                else if (obj.Type == IObjectType.Surface)
                {
                    IODBObject surface = filter.CreatePolygon(layer);
                    ISurfaceSpecificsD surfaceSpecificsD = (ISurfaceSpecificsD)obj.GetSpecificsD();
                    surface.SetSpecifics(surfaceSpecificsD);
                }
                else if (obj.Type == IObjectType.Text)
                {
                    IODBObject text = filter.CreateText(layer);
                    ITextSpecificsD textSpecificsD = (ITextSpecificsD)obj.GetSpecificsD();
                    text.SetSpecifics(textSpecificsD);
                }
            }
            if (activateLayer)
                layer.EnableLayer(true);
            
            IMatrix matrix = parent.GetMatrix();
            matrix.SetMatrixLayerType(layer.LayerName, MatrixLayerType.Drill);
            matrix.SetMatrixLayerContext(layer.LayerName, MatrixLayerContext.Board);
        }
    }
	}
}