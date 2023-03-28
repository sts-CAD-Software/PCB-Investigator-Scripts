//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 13.10.2015
// Autor Guenther
// 
// Conect to an Excel sheet and wirte via locations
//-----------------------------------------------------------------------------------
// This script connects to an Excel sheet and writes data about vias and pads on a PCB layout via locations.
// The script creates an instance of the Excel.Application object and makes it visible. Then it creates a new workbook in Excel.
// The script then goes through each net on the PCB layout and checks each net object to see if it is a pad or a via. If it is a via, the script writes the type, net name, and position of the via to a StringBuilder object. 
// If it is a pad on the top or bottom signal layer, the script writes the type, net name, and position of the pad to the StringBuilder object. 
// Once all pads and vias have been processed, the data is copied to the clipboard as both text and comma-separated values. Finally, the data is pasted into the active sheet in Excel. 
// The script then releases the Excel object. 
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
        public IPCBIWindow Parent;
		public void Execute(IPCBIWindow parent)
		{
 			//your code here
            Parent = parent;
            Report_Via_Pad();
            parent.UpdateView(); 
		}

       public void Report_Via_Pad()
        {
            var excelType = Type.GetTypeFromProgID("Excel.Application");

            dynamic excel = Activator.CreateInstance(excelType);
            excel.Visible = true;
            excel.Workbooks.Add();
           //////

            StringBuilder sb = new StringBuilder();

            IStep step = Parent.GetCurrentStep();
            IMatrix m = Parent.GetMatrix();

            foreach (INet NetList in step.GetNets())
            {

                foreach (IODBObject obj in NetList.GetAllNetObjects(Parent))
                {
                    if (m.GetMatrixLayerType(obj.GetParentLayerName()) == MatrixLayerType.Drill)
                    {
                        if (obj.Type == IObjectType.Pad)
                        {
                            IPadSpecifics ps = (IPadSpecifics)obj.GetSpecifics();
                            float x = ps.Location.X;
                            float Y = ps.Location.Y;
                            if (Parent.GetUnit())
                            {
                                float unit = 25.4f / 1000;
                                x = x * unit;
                                Y = Y * unit;

                            }
                            sb.Append("Via;" + obj.NetName + ";" + x.ToString("N3") + ";" + Y.ToString("N3") + Environment.NewLine);
                        }
                    }
                    else
                        if (obj.GetParentLayerName().ToLower() == m.GetTopSignalLayer().ToLower())
                        {
                            if (obj.Type == IObjectType.Pad)
                            {
                                IPadSpecifics ps = (IPadSpecifics)obj.GetSpecifics();
                                if (ps.Type != PCBI.Symbol_Type.r)
                                {
                                    float x = ps.Location.X;
                                    float Y = ps.Location.Y;
                                    if (Parent.GetUnit())
                                    {
                                        float unit = 25.4f / 1000;
                                        x = x * unit;
                                        Y = Y * unit;

                                    }
                                    sb.Append("PAD_top;" + obj.NetName + ";" + x.ToString("N3") + ";" + Y.ToString("N3") + Environment.NewLine);
                                }
                            }
                        }
                        else
                            if (obj.GetParentLayerName().ToLower() == m.GetBotSignalLayer().ToLower())
                            {
                                if (obj.Type == IObjectType.Pad)
                                {
                                    IPadSpecifics ps = (IPadSpecifics)obj.GetSpecifics();
                                    if (ps.Type != PCBI.Symbol_Type.r)
                                    {
                                        float x = ps.Location.X;
                                        float Y = ps.Location.Y;
                                        if (Parent.GetUnit())
                                        {
                                            float unit = 25.4f / 1000;
                                            x = x * unit;
                                            Y = Y * unit;

                                        }
                                        sb.Append("PAD_bot;" + obj.NetName + ";" + x.ToString("N3") + ";" + Y.ToString("N3") + Environment.NewLine);
                                    }
                                }
                            }
                }

            }
           //////
            
            //string LVText = "Type\tNetName\tposx\tposy" + Environment.NewLine + sb.ToString();
            string LVText = "Type;NetName;posx;posy" + Environment.NewLine + sb.ToString();
            string LVCsv = sb.ToString();
            DataObject LVDataObject = new DataObject();
            LVDataObject.SetData(DataFormats.Text, false, LVText);
            LVDataObject.SetData(DataFormats.CommaSeparatedValue, false, LVCsv);
            Clipboard.SetDataObject(LVDataObject, true);

            excel.ActiveSheet.Paste();

            //release the object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
        }
		
    }
}