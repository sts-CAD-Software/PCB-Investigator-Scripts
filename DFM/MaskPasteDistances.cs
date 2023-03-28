//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 22.09.2015
// Autor Fabio Gruber
// Updated: 29.09.2016 (Progressbar and Error Message)
// 
// Check distances between maks and solder paste.
//-----------------------------------------------------------------------------------
// The script checks the distances between solder mask and solder paste on a PCB.
// It uses the PCB Investigator API to find the solder mask and solder paste layers and then traverses all objects in the solder paste layer to calculate the distances.
// The result is written to an Excel spreadsheet and copied to the clipboard for easy access.
// If no distances are found, an error message is displayed. 
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
        private PCB_Investigator.PCBIWindows.PCBIWorkingDialog wdlg;

        public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            //example to check first solder paste with first solder mask distances.
            double maxDist = 2; //mils

            wdlg = new PCB_Investigator.PCBIWindows.PCBIWorkingDialog();
            wdlg.SetAnimationStatus(false);
            wdlg.SetStatusPercent(0);
            wdlg.SetStatusText("Working");
            wdlg.CanCancel(true);


            IMatrix matrix = parent.GetMatrix();
            IStep step = parent.GetCurrentStep();

            wdlg.ShowWorkingDlgAsThread();

            List<string> layerNames = step.GetAllLayerNames();
            double value = 0;
            double valueStep = ((100.0 / layerNames.Count));

            Dictionary<IODBObject.DistanceResultClass, IODBObject> DistanceList = new Dictionary<IODBObject.DistanceResultClass, IODBObject>();
            IODBLayer SMTLayer = null;
            IODBLayer SPLayer = null;
            foreach (string layername in step.GetAllLayerNames())
            {
                wdlg.SetStatusText("Working on " + layername + "...");
                value += valueStep;
                wdlg.SetStatusPercent((int)(value));

                if (matrix.GetMatrixLayerType(layername) == MatrixLayerType.Solder_paste && SPLayer == null) //find top solderpaste
                {
                    SPLayer = (IODBLayer)step.GetLayer(layername);
                }
                else if (matrix.GetMatrixLayerType(layername) == MatrixLayerType.Solder_mask && SMTLayer == null) //find top mask layer
                {
                    SMTLayer = (IODBLayer)step.GetLayer(layername);
                }

                if (SMTLayer != null && SPLayer != null)
                {
                    foreach (IODBObject IODBO1 in SPLayer.GetAllLayerObjects())
                    {
                        RectangleD boundsToInflate = IODBO1.GetBoundsD();
                        boundsToInflate.Inflate(maxDist, maxDist);
                        foreach (IODBObject IODBO2 in SMTLayer.GetAllObjectInRectangle(boundsToInflate))
                        {
                            IODBObject.DistanceResultClass distance = IODBO1.DistanceTo(IODBO2);
                            if (distance.Distance >= 0)
                            {
                                DistanceList.Add(distance, IODBO2);
                                IODBO2.ObjectColorTemporary(Color.DarkRed);
                            }
                        }
                    }
                    break;
                }
            }
            wdlg.Dispose();
            if (DistanceList.Count > 0) //write result to excel
            {
                StringBuilder sb = new StringBuilder();

                var excelType = Type.GetTypeFromProgID("Excel.Application");
                dynamic excel = Activator.CreateInstance(excelType);
                excel.Visible = true;
                excel.Workbooks.Add();

                foreach (IODBObject.DistanceResultClass distanceResult in DistanceList.Keys)
                {
                    sb.Append("\t" + DistanceList[distanceResult].PcbNetNumber + "\t" + distanceResult.From + "\t" + distanceResult.To + "\t" + distanceResult.Distance + Environment.NewLine);

                }

                string LVText = "Distances\tNet Number\tMask Layer\tSolder Paste\tValue" + Environment.NewLine + sb.ToString();
                string LVCsv = sb.ToString();
                DataObject LVDataObject = new DataObject();
                LVDataObject.SetData(DataFormats.Text, true, LVText);
                LVDataObject.SetData(DataFormats.CommaSeparatedValue, true, LVCsv);
                Clipboard.SetDataObject(LVDataObject, true);

                excel.ActiveSheet.Paste();

                //release the object
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
            }
            else
            {
                MessageBox.Show("No Results found...\n Please check Layers for Paste- or Mask layers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}