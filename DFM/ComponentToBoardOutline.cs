//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 12.07.2017
// Autor Guenther.Schindler
// 
// Empty template to fill for synchronous script.
//-----------------------------------------------------------------------------------
// GUID newScript_636354458630923556
//
// This script is a template for synchronized PCB-Investigator automation scripts that allow users to quickly and easily perform an analysis of the distance between components and the PCB outline.
// The script loads a list of components and compares the distance between each component and the PCB outline.
// If the measured distance is less than the user-specified threshold, a results list of affected components is generated and displayed.
// The user can then select the affected components and have the view of the PCB automatically zoomed to the selected area. 
//---------------------------------------------------------------------------------

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
// example line to import dll DLLImport C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\searchedDLL.dll;
//this line includes a custom dll, e.g. your own dll or other microsoft dlls

namespace PCBIScript
{
    public class PScript : IPCBIScript
    {
        IPCBIWindow parent;
        public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            this.parent = parent;
            IStep step = parent.GetCurrentStep();
            if (step == null) return;

            List<string> columsStrings = new List<string>();
            columsStrings.Add("Analyse Resource");
            columsStrings.Add("Reference");
            columsStrings.Add("Distance");
            columsStrings.Add("Rule");
            columsStrings.Add("Start");
            columsStrings.Add("End");
            PCB_Investigator.PCBIWindows.PCBIResultDialog resultDLG = new PCB_Investigator.PCBIWindows.PCBIResultDialog(columsStrings);
            double unit = 1;
            double distance = PCBI.MathUtils.IMath.MM2Mils(1.0f);
            if (parent.GetUnit())
            {
                unit = 25.4;
                //MessageBox.Show("Component to Outline distance: " + PCBI.MathUtils.IMath.Mils2MM(distance).ToString("N3") + " micron", "DRC Component to Board Outline");

            }
            else
            {
                //MessageBox.Show("Component to Outline distance: " + distance.ToString("N3") + " mils", "DRC Component to Board Outline");
                unit = 1;
                
            }
            foreach (ICMPObject cmp in step.GetAllCMPObjects())
            {
                PCBI.MathUtils.IPolyClass CMP_Poly = cmp.GetPolygonOutline(false);
                IODBObject boardOutline = parent.GetCurrentStep().GetPCBOutlineAsODBObject();
                IPolyClass polyOutline = boardOutline.GetPolygonOutline();
                PointD start = new PointD(0, 0);
                PointD end = new PointD(0, 0);
                double measuredDistance = cmp.GetPolygonOutline().DistanceTo(polyOutline, ref start, ref end);
                if (measuredDistance < distance)
                {
                    ListViewItem lvi = new ListViewItem("Cpmp2Outline");
                    lvi.SubItems.Add(cmp.Ref);
                    lvi.SubItems.Add((measuredDistance * unit).ToString());     
                    lvi.SubItems.Add((distance * unit).ToString());    
                    lvi.SubItems.Add((start * unit).ToString());
                    lvi.SubItems.Add((end * unit).ToString());
                    lvi.Tag = cmp;
                    resultDLG.AddListViewItem(lvi);
                }
            }

            resultDLG.ItemSelectionChanged += ResultDLG_ItemSelectionChanged1; ;
            resultDLG.Size = new Size(500, 350);
            resultDLG.Show();
        }

        private void ResultDLG_ItemSelectionChanged1(ListView.SelectedListViewItemCollection Selection)
        {
            IStep step = parent.GetCurrentStep();
            if (step != null)
            {
                foreach (ListViewItem lvi in Selection)
                {

                    if (step != null)
                    {
                        step.ClearSelection();
                        ((ICMPObject)lvi.Tag).Select(true);
                        step.TurnOffAllLayer();

                        IODBLayer layer = step.GetOutsideODBLayer(((ICMPObject)lvi.Tag).PlacedTop);
                        if (layer != null) layer.EnableLayer(true);

                        parent.UpdateSelection();

                        PCBI.MathUtils.RectangleD zoomRect = ((ICMPObject)lvi.Tag).GetBoundsD();
                        if (zoomRect == PCBI.MathUtils.RectangleD.Empty)
                            parent.ZoomToSelection();
                        else
                        {
                            zoomRect.Inflate(zoomRect.Width * 0.5, zoomRect.Height * 0.5);
                            parent.ZoomRect(zoomRect);
                        }
                    }
                    else
                    {
                        ((ICMPObject)lvi.Tag).Select(true);
                        parent.UpdateSelection();
                        parent.UpdateView();
                    }
                }
            }
        }
    }
}
