//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 22.09.2015
// Autor Fabio Gruber
// 
// Check distances between maks and solder paste.
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
            //example to check first solder paste with first solder mask distances.
            double maxDist = 2; //mils

            IMatrix matrix = parent.GetMatrix();
            IStep step = parent.GetCurrentStep();

            Dictionary<IODBObject.DistanceResultClass, IODBObject> DistanceList = new Dictionary<IODBObject.DistanceResultClass, IODBObject>();
            IODBLayer SMTLayer = null;
            IODBLayer SPLayer = null;
            foreach (string layername in step.GetAllLayerNames())
            {
                if (matrix.GetMatrixLayerType(layername) == MatrixLayerType.Solder_paste && SPLayer ==null) //find top solderpaste
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
        }
      		
    }
}