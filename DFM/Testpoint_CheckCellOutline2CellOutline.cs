//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2017-04-26
// Author EasyLogix
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 2017-06-13
// Description: Check the distance between components and testpoints.
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID Testpoint_Check_636288129921985500
// ButtonEnabled=5   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)
//
// This script is designed to check the distances between components and test points on the board.
// It checks the top and bottom planes for components, searches for test points, and then checks the distances between the test points and the components and between the test points themselves.
// If the distance is less than a certain threshold, this is indicated in a dialog box report.
// The goal is to provide customers with an easy way to determine if the distances between components and test points in their circuits are correct. 
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
        private const string tpReferencePrexix = "P";  // Testpunkt Ref Prefix
        private double MinDistanceTP2CMP = PCBI.MathUtils.IMath.MM2Mils(5);  // Abstand zu Bauteil
        private double MinDistanceTP2TP = PCBI.MathUtils.IMath.MM2Mils(0.8);  // Abstand TP zu TP
        private int tpCountTotal = 0;
        public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            if (parent == null || !parent.JobIsLoaded)
            {
                MessageBox.Show("No job loaded.");
                return;
            }

            IStep step = parent.GetCurrentStep();
            IMatrix matrix = parent.GetMatrix();

            if (matrix == null || step == null)
            {
                MessageBox.Show("No job loaded.");
                return;
            }

            PCB_Investigator.PCBIWindows.PCBIReportDialog reportDlg = new PCB_Investigator.PCBIWindows.PCBIReportDialog("TP Check Cell Outline to Cell Outline and TP to high Componen", "Result:");


            //Check Top Component Layer
            tpCountTotal = 0;
            string topComponentLayerName = matrix.GetTopComponentLayer();
            if (!string.IsNullOrWhiteSpace(topComponentLayerName))
            {
                ILayer dataLayer = step.GetLayer(topComponentLayerName);
                if (dataLayer != null && dataLayer is ICMPLayer)
                {
                    CheckTestpoints((ICMPLayer)dataLayer, ref reportDlg);
                }
            }

            //Check Bot Component Layer
            string botComponentLayerName = matrix.GetBotComponentLayer();
            if (!string.IsNullOrWhiteSpace(botComponentLayerName))
            {
                ILayer dataLayer = step.GetLayer(botComponentLayerName);
                if (dataLayer != null && dataLayer is ICMPLayer)
                {
                    CheckTestpoints((ICMPLayer)dataLayer, ref reportDlg);
                }
            }

            parent.UpdateView();
            reportDlg.ShowDlg(PCB_Investigator.PCBIWindows.PCBIReportDialog.WindowType.Modal);
        }

        private void CheckTestpoints(ICMPLayer layer, ref PCB_Investigator.PCBIWindows.PCBIReportDialog reportDlg)
        {
            if (layer == null)
            {
                MessageBox.Show("Layer is null referenced.");
                return;
            }

            reportDlg.AppendLog("Analyzing " + layer.GetLayerName() + "          -*-*-*-*-*-*-*-*-*-*-*-*-*---------------------------------");

            PointD fromPoint = PointD.Empty, toPoint = PointD.Empty;

            List<IObject> allComponents = layer.GetAllLayerObjects();
            double maxDistance = Math.Max(MinDistanceTP2TP, MinDistanceTP2CMP) * 1.01;
            
            int tpCountOK = 0;
            foreach (IObject element in allComponents)
            {
                if (element is ICMPObject)
                {
                    ICMPObject tpCmp = (ICMPObject)element;

                    if (tpCmp.Ref.StartsWith(tpReferencePrexix))  //cmp is a Testpoint...
                    {
                        tpCountTotal++;
                        bool hasWrongDistance = false;
                        RectangleD checkRect = tpCmp.GetBoundsD();
                        checkRect.Inflate(maxDistance, maxDistance);

                        IPolyClass tpPoly = tpCmp.GetPolygonOutline(false);

                        foreach (IObject nearElement in layer.GetAllObjectInRectangle(checkRect))
                        {
                            if (nearElement is ICMPObject)
                            {
                                ICMPObject nearCmp = (ICMPObject)nearElement;
                                if (tpCmp.Ref == nearCmp.Ref) continue;
                                if (!nearCmp.Ref.StartsWith(tpReferencePrexix) && nearCmp.CompHEIGHT < PCBI.MathUtils.IMath.MM2Mils(5)) continue; // Abfrage ob Bauteil kleiner 5 mm, dann ignorieren ausser Testpunkte

                                IPolyClass nearCmpPoly = nearCmp.GetPolygonOutline(false);

                                if (tpPoly != null && nearCmpPoly != null)
                                {
                                    double distance = tpPoly.DistanceTo(nearCmpPoly, ref fromPoint, ref toPoint);
                                    if (nearCmp.Ref.StartsWith(tpReferencePrexix))  //nearCmpPoly is also a Testpoint...
                                    {
                                        if (distance < MinDistanceTP2TP)
                                        {
                                            if (hasWrongDistance == false)
                                            {
                                                tpCmp.ObjectColor = Color.Orange;
                                                hasWrongDistance = true;
                                            }
                                            reportDlg.AppendLog(" -> " + tpCmp.Ref + " too close to " + nearCmp.Ref + " (" + PCBI.MathUtils.IMath.Mils2MM(distance).ToString("F3") + "mm)", PCB_Investigator.PCBIWindows.PCBIReportDialog.LogType.Warning);
                                        }
                                        
                                    }
                                    else
                                    {
                                        if (distance < MinDistanceTP2CMP)
                                        {
                                            if (hasWrongDistance)
                                            {
                                                tpCmp.ObjectColor = Color.Red;
                                                hasWrongDistance = true;
                                            }
                                            reportDlg.AppendLog(" -> " + tpCmp.Ref + " too close to " + nearCmp.Ref + " (" + PCBI.MathUtils.IMath.Mils2MM(distance).ToString("F3") + "mm)", PCB_Investigator.PCBIWindows.PCBIReportDialog.LogType.Warning);
                                        }
                                    }
                                }
                            }
                        }

                        if (!hasWrongDistance)
                        {
                            tpCountOK++;
                            tpCmp.ObjectColor = Color.Green;
                            reportDlg.AppendLog(" -> " + tpCmp.Ref + " is OK");
                        }
                    }
                }
            }

            reportDlg.AppendLog("=> " + tpCountOK + " of " + tpCountTotal + " Testpoints are OK **************************************************");
        }
    }
}