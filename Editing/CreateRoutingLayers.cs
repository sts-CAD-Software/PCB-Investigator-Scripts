//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 24.10.2016
// Autor Fabio.Gruber
// 
// Create routing layers with extra layers for cutout and outline.
//-----------------------------------------------------------------------------------
// GUID newScript_636129084750092338

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
            double sizeRouting = PCBI.MathUtils.IMath.MM2Mils(2); //2 mm
            double sizeCutout = PCBI.MathUtils.IMath.MM2Mils(2); //2 mm
            double spaceCutout = PCBI.MathUtils.IMath.MM2Mils(100); //100 mm
            double lengthCutout = PCBI.MathUtils.IMath.MM2Mils(5); //5 mm 
            double sumUpLength = 0; //offset for start

            IStep step = parent.GetCurrentStep();
            if (step == null) return;

            PCBI.MathUtils.IPolyClass polyOfOutline = step.GetPCBOutlinePoly();

            if (polyOfOutline.GetEdgeCount() == 0) return;

            IFilter filter = new IFilter(parent);
            IODBLayer HelperLayer = filter.CreateEmptyODBLayer("rout_outline_helper", step.Name); //this helper layer contains the uncutted line elements
            IODBLayer outlineLayerRouting = filter.CreateEmptyODBLayer("rout_outline", step.Name);
            IODBLayer cutoutLayerRouting = filter.CreateEmptyODBLayer("rout_cutout", step.Name);

            #region clean layers for multi use
            List<IODBObject> objListToRemove = new List<IODBObject>();
            foreach (IODBObject obj in HelperLayer.GetAllLayerObjects())
                objListToRemove.Add(obj);
            HelperLayer.RemoveObjects(objListToRemove);

            objListToRemove = new List<IODBObject>();
            foreach (IODBObject obj in cutoutLayerRouting.GetAllLayerObjects())
                objListToRemove.Add(obj);
            cutoutLayerRouting.RemoveObjects(objListToRemove);

            objListToRemove = new List<IODBObject>();
            foreach (IODBObject obj in outlineLayerRouting.GetAllLayerObjects())
                objListToRemove.Add(obj);
            outlineLayerRouting.RemoveObjects(objListToRemove);
            #endregion

            int shapeIndexOutlinediameter = IFilter.AddToolDefinitionRound(HelperLayer, (float)sizeRouting);

            foreach (PCBI.MathUtils.IEdge edge in polyOfOutline.GetEdges())
            {
                #region outline elements
                if (edge.Type == IEdgeType.Arc)
                {
                    IODBObject newArc = filter.CreateArc(HelperLayer);

                    IArcSpecificsD arc = (IArcSpecificsD)newArc.GetSpecificsD();
                    arc.ClockWise = ((IArcEdge)edge).ClockWise;
                    if (arc.ClockWise)
                    {
                        arc.Start = edge.Begin;
                        arc.End = edge.End;
                    }
                    else
                    {
                        arc.End = edge.Begin;
                        arc.Start = edge.End;
                    }
                    arc.Center = ((PCBI.MathUtils.IArcEdge)edge).Center;
                    arc.ShapeIndex = shapeIndexOutlinediameter;

                    newArc.SetSpecifics(arc);
                }
                else
                {
                    IODBObject newLine = filter.CreateLine(HelperLayer);

                    ILineSpecificsD line = (ILineSpecificsD)newLine.GetSpecificsD();

                    line.Start = edge.Begin;
                    line.End = edge.End;
                    line.ShapeIndex = shapeIndexOutlinediameter;

                    newLine.SetSpecifics(line);
                }
                #endregion
            }

            //make one surface of all lines and arcs
            HelperLayer.CreateLayerNetList(true);
            HelperLayer.PolygonizeLayer(0, false);

            PointD lastPToIdentifyHole = PointD.InfPoint;

            foreach (IODBObject surface in HelperLayer.GetAllLayerObjects())
            {
                #region replace surfaces by routing lines and arcs
                if (surface.Type != IObjectType.Surface) continue;
                objListToRemove.Add(surface);
                List<ISurfaceSpecificsD> isle = ((ISurfaceSpecificsD)surface.GetSpecificsD()).SplitInIsleAndHoles(parent);

                if (isle.Count > 0)
                {
                    for (int i = 0; i < isle.Count; i++)
                    {
                        bool foundelement = false;
                        foreach (IODBObject linesAndArcs in isle[i].GetOutline())
                        {
                            if (linesAndArcs.Type == IObjectType.Arc)
                            {
                                #region arc
                                IODBObject newArc = filter.CreateArc(HelperLayer);
                                IArcSpecificsD arc = (IArcSpecificsD)linesAndArcs.GetSpecificsD();

                                if (lastPToIdentifyHole == PointD.InfPoint || !(lastPToIdentifyHole != arc.Start && lastPToIdentifyHole != arc.End))
                                {
                                    lastPToIdentifyHole = arc.End;
                                }
                                else
                                    break;
                                newArc.SetSpecifics(arc);
                                #endregion
                            }
                            else
                            {
                                #region line
                                IODBObject newLine = filter.CreateLine(HelperLayer);
                                ILineSpecificsD line = (ILineSpecificsD)linesAndArcs.GetSpecificsD();
                                if (lastPToIdentifyHole == PointD.InfPoint || line.Start == lastPToIdentifyHole)
                                {
                                    lastPToIdentifyHole = line.End;
                                }
                                else
                                    break;
                                newLine.SetSpecifics(line);
                                #endregion
                            }
                            foundelement = true;
                        }
                        if (foundelement) break;
                    }
                }
                #endregion
            }
            HelperLayer.RemoveObjects(objListToRemove); //surface remove
            int shapeIndexCutoutdiameter = IFilter.AddToolDefinitionRound(outlineLayerRouting, (float)sizeCutout);
            int shapeIndexCutoutdiameter2 = IFilter.AddToolDefinitionRound(cutoutLayerRouting, (float)sizeCutout);
            double sumUpGap = 0;

            foreach (IODBObject lineOrArc in HelperLayer.GetAllLayerObjects())
            {
                if (lineOrArc.Type == IObjectType.Line)
                {
                    #region lines
                    ILineSpecificsD line = (ILineSpecificsD)lineOrArc.GetSpecificsD();
                    double length = IMath.DistancePointToPoint(line.Start, line.End);
                    double lengthComplete = length;
                    while (true)
                    {
                        if (length <= lengthCutout)
                        {
                            #region short lines
                            sumUpLength += lengthCutout;
                            if (sumUpLength > spaceCutout)
                            {
                                sumUpGap += lengthCutout;
                                IODBObject lineObj = filter.CreateLine(cutoutLayerRouting);
                                ILineSpecificsD lineSub = (ILineSpecificsD)lineOrArc.GetSpecificsD();
                                lineSub.ShapeIndex = shapeIndexCutoutdiameter2;
                                lineSub.Start = IMath.GetPointOnLine(line.Start, line.End, lengthComplete - length);
                                length = 0;
                                lineSub.End = IMath.GetPointOnLine(line.Start, line.End, lengthComplete - length);
                                lineObj.SetSpecifics(lineSub);
                                if (sumUpGap > lengthCutout)
                                {
                                    sumUpGap = 0;
                                    sumUpLength = 0;
                                }

                                IAttributeElement attribRoutningComp = new IAttributeElement(PCBI.FeatureAttributeEnum.comp);
                                attribRoutningComp.Value = "none";
                                IAttribute.SetAttribute(attribRoutningComp, lineObj);
                            }
                            else
                            {
                                IODBObject lineObj = filter.CreateLine(outlineLayerRouting);
                                ILineSpecificsD lineSub = (ILineSpecificsD)lineOrArc.GetSpecificsD();
                                lineSub.ShapeIndex = shapeIndexCutoutdiameter;
                                lineSub.Start = IMath.GetPointOnLine(line.Start, line.End, lengthComplete - length);
                                length = 0;
                                lineSub.End = IMath.GetPointOnLine(line.Start, line.End, lengthComplete - length);
                                lineObj.SetSpecifics(lineSub);

                                IAttributeElement attribRoutningComp = new IAttributeElement(PCBI.FeatureAttributeEnum.comp);
                                attribRoutningComp.Value = "none";
                                IAttribute.SetAttribute(attribRoutningComp, lineObj);
                            }
                            break; 
                            #endregion
                        }
                        else
                        {
                            #region long lines
                            sumUpLength += lengthCutout;
                            if (sumUpLength > spaceCutout || sumUpGap > 0)
                            {
                                sumUpGap += lengthCutout;
                                IODBObject lineObj = filter.CreateLine(cutoutLayerRouting);

                                ILineSpecificsD lineSub = (ILineSpecificsD)lineOrArc.GetSpecificsD();
                                lineSub.ShapeIndex = shapeIndexCutoutdiameter2;
                                lineSub.Start = IMath.GetPointOnLine(line.Start, line.End, lengthComplete - length);
                                length -= lengthCutout;
                                lineSub.End = IMath.GetPointOnLine(line.Start, line.End, lengthComplete - length);

                                lineObj.SetSpecifics(lineSub);
                                if (sumUpGap > lengthCutout)
                                {
                                    sumUpGap = 0;
                                    sumUpLength = 0;
                                }

                                IAttributeElement attribRoutningComp = new IAttributeElement(PCBI.FeatureAttributeEnum.comp);
                                attribRoutningComp.Value = "none";
                                IAttribute.SetAttribute(attribRoutningComp, lineObj);
                            }
                            else
                            {
                                IODBObject lineObj = filter.CreateLine(outlineLayerRouting);

                                ILineSpecificsD lineSub = (ILineSpecificsD)lineOrArc.GetSpecificsD();
                                lineSub.ShapeIndex = shapeIndexCutoutdiameter;
                                lineSub.Start = IMath.GetPointOnLine(line.Start, line.End, lengthComplete - length);
                                length -= lengthCutout;
                                lineSub.End = IMath.GetPointOnLine(line.Start, line.End, lengthComplete - length);

                                lineObj.SetSpecifics(lineSub);

                                IAttributeElement attribRoutningComp = new IAttributeElement(PCBI.FeatureAttributeEnum.comp);
                                attribRoutningComp.Value = "none";
                                IAttribute.SetAttribute(attribRoutningComp, lineObj);
                            } 
                            #endregion
                        }
                    } 
                    #endregion
                }
                else if (lineOrArc.Type == IObjectType.Arc)
                {
                    #region arcs
                    IArcSpecificsD arc = (IArcSpecificsD)lineOrArc.GetSpecificsD();
                    double arcLength = IMath.DistancePointToPoint(arc.Start, arc.End);

                    sumUpLength += arcLength;

                    if (sumUpLength > spaceCutout || sumUpGap > 0)
                    {
                        sumUpGap += arcLength;
                        IODBObject arcObj = filter.CreateArc(cutoutLayerRouting);
                        arc.ShapeIndex = shapeIndexCutoutdiameter2;
                        arcObj.SetSpecifics(arc);
                        if (sumUpGap > lengthCutout)
                        {
                            sumUpGap = 0;
                            sumUpLength = 0;
                        }
                        IAttributeElement attribRoutningComp = new IAttributeElement(PCBI.FeatureAttributeEnum.comp);
                        attribRoutningComp.Value = "none";
                        IAttribute.SetAttribute(attribRoutningComp, arcObj);
                    }
                    else
                    {
                        IODBObject arcObj = filter.CreateArc(outlineLayerRouting);
                        arc.ShapeIndex = shapeIndexCutoutdiameter;
                        arcObj.SetSpecifics(arc);
                        IAttributeElement attribRoutningComp = new IAttributeElement(PCBI.FeatureAttributeEnum.comp);
                        attribRoutningComp.Value = "none";
                        IAttribute.SetAttribute(attribRoutningComp, arcObj);
                    }
                    #endregion
                }
            }
            //additional attributes are .feed, .speed, .rout_flag, .comp. and .rout_chain

            IMatrix matrix = parent.GetMatrix();
            if (matrix != null) matrix.UpdateDataAndList();
            parent.UpdateView();
        }
  
	}
}