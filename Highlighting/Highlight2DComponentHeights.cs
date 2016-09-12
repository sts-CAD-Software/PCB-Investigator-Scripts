//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script for DFM Check
// Created on 23.09.2015
// Autor Fabio Gruber
// support@easylogix.de
// 
// Show a 2D side view of components for each side with board (if the board thickness is available as property).
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
            bool allCMPHeightsInMM = true;
            //check all pads on top and bot signal layers and mask layers for open space in mask...
            
            IMatrix matrix = parent.GetMatrix();
            IStep step = parent.GetCurrentStep();

            if (step == null || matrix == null) return;

            PCB_Investigator.PCBIWindows.PCBIWorkingDialog working = new PCB_Investigator.PCBIWindows.PCBIWorkingDialog();
            working.CanCancel ( false);
            working.SetStatusText("Creating 2D side views...");
            working.SetAnimationStatus(true);

            working.ShowWorkingDlgAsThread();

            ICMPLayer topCMPLayer = step.GetCMPLayer(true);
            ICMPLayer botCMPLayer = step.GetCMPLayer(false);

            IFilter filter = new IFilter(parent);

            IODBLayer frontLayer = filter.CreateEmptyODBLayer("side_front", step.Name, false);
            IODBLayer sideLayer = filter.CreateEmptyODBLayer("side_side", step.Name, false);
            float boardThickness = GetBoardHeight(step);
            
            //board from side and front
            CreateBoardView(step, filter, frontLayer, boardThickness, true);
            CreateBoardView(step, filter, sideLayer, boardThickness, false);

            CreateComponents(true, topCMPLayer, frontLayer, sideLayer, boardThickness / 2, filter, allCMPHeightsInMM);
            CreateComponents(false, botCMPLayer, frontLayer, sideLayer, boardThickness / 2, filter, allCMPHeightsInMM);




            working.DoClose();
            
            PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("See new layers side... for results.", "Views created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            matrix.UpdateDataAndList();
            parent.UpdateView();
        }

        private static void CreateComponents(bool top, ICMPLayer CMPLayer, IODBLayer frontLayer, IODBLayer sideLayer, float offset, IFilter filter, bool allValuesInMM)
        {
            if (CMPLayer == null) return;
            foreach (ICMPObject component in CMPLayer.GetAllLayerObjects())
            {
                float height = (float)component.CompHEIGHT;
                IPackageSpecificsD usedPackage = component.GetPackageSpecificsD();
                if (height == 0)
                {
                    height = usedPackage.Height;
                }
                if (height == 0)
                {
                    #region check height
                    //check attributes
                    foreach (string attrib in component.GetComponentAttributes())
                    {
                        if (attrib.ToLowerInvariant().Contains("height"))
                        {
                            string[] attribtValueFinder = attrib.Split('=');

                            if (attribtValueFinder.Length > 1)
                            {
                                float.TryParse(attribtValueFinder[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out height);
                            }
                            else if (attribtValueFinder[0].Contains("\'"))
                            {
                                string subStr = attribtValueFinder[0].Substring(attribtValueFinder[0].IndexOf('\'') + 1);
                                if (subStr.Contains("\'"))
                                {
                                    subStr = subStr.Substring(0, subStr.IndexOf('\''));
                                    float.TryParse(subStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out height);
                                }
                            }
                        }
                    }
                    #endregion
                }
                if(allValuesInMM)
                    height = (float)IMath.MM2Mils(height);

                #region front layer
                int componentFront = IFilter.AddToolDefinitionRect(frontLayer, (float)component.Bounds.Width, height, (float)component.Bounds.Width, false);

                IODBObject cmpFront = filter.CreatePad(frontLayer);

                IPadSpecificsD padCMPFront = (IPadSpecificsD)cmpFront.GetSpecificsD();

                if (top) padCMPFront.Location = new PointD(component.Bounds.X + component.Bounds.Width / 2, offset + height / 2);
                else padCMPFront.Location = new PointD(component.Bounds.X + component.Bounds.Width / 2, -(offset + height / 2));

                cmpFront.SetSpecifics(padCMPFront, componentFront);
                cmpFront.FreeText = component.Ref;
                if (top) cmpFront.ObjectColorTemporary(Color.Silver);
                else cmpFront.ObjectColorTemporary(Color.Gray);
                #endregion
                #region side layer
                int componentSide = IFilter.AddToolDefinitionRect(sideLayer, (float)component.Bounds.Height, (float)component.Bounds.Height, height, false);

                IODBObject cmpSide = filter.CreatePad(sideLayer);

                IPadSpecificsD padCMPSide = (IPadSpecificsD)cmpSide.GetSpecificsD();

                if (top)
                    padCMPSide.Location = new PointD(offset + height / 2, component.Bounds.Y + component.Bounds.Height / 2);
                else
                    padCMPSide.Location = new PointD(-(offset + height / 2), component.Bounds.Y + component.Bounds.Height / 2);

                cmpSide.SetSpecifics(padCMPSide, componentSide);
                cmpSide.FreeText = component.Ref;
                if (top) cmpSide.ObjectColorTemporary(Color.Silver);
                else cmpSide.ObjectColorTemporary(Color.Gray);
                #endregion
            }
        }

        private static void CreateBoardView(IStep step, IFilter filter, IODBLayer frontLayer, float boardThickness, bool front)
        {
            if (boardThickness > 0)
            {
                #region draw board front
                int shapeIndexBoardHeight = IFilter.AddToolDefinitionRect(frontLayer, boardThickness, boardThickness, boardThickness, false);

                //add line with board symbol index to have correct thickness
                IODBObject boardLineFront = filter.CreateLine(frontLayer);

                ILineSpecificsD lineDetails = (ILineSpecificsD)boardLineFront.GetSpecificsD();
                if (front)
                {
                    lineDetails.Start = new PointD(step.GetBoundsD().Left + boardThickness / 2, 0);
                    lineDetails.End = new PointD(step.GetBoundsD().Right - boardThickness / 2, 0);
                }
                else
                {
                    lineDetails.Start = new PointD(0,step.GetBoundsD().Top + boardThickness / 2);
                    lineDetails.End = new PointD(0,step.GetBoundsD().Bottom - boardThickness / 2);
                }
                boardLineFront.SetSpecifics(lineDetails, shapeIndexBoardHeight);
                boardLineFront.ObjectColorTemporary ( Color.Green);
                #endregion
            }
        }
        private float GetBoardHeight(IStep odbStep)
        {
            foreach (string attribute in odbStep.GetStepAttributes())
            {
                if (attribute.StartsWith(".board_thickness"))
                {
                    #region board_thickness
                    string[] relAttribute = attribute.Split('=');

                    double relThickness = 0;

                    if (relAttribute.Length > 1)
                    {
                        double.TryParse(relAttribute[1], System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out relThickness);
                    }
                    return (float)relThickness;
                    #endregion
                }
            }

            return 0; //default PCB thickness?
        }

    }
}
