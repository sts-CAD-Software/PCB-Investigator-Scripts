//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Export Component Attributes to Excel.
//-----------------------------------------------------------------------------------
// This script is used to export a list of components on a PCB to an Excel spreadsheet.
// An Excel object is created that accepts the data and inserts it into the spreadsheet.
// The script reads the component references, names, packages and values from all CMP objects (components) on the current PCB layer.
// The data is copied to the clipboard as text and CSV and then pasted into Excel. 
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
            if (step == null) return;
            IFilter filter = new IFilter(parent);

            StringBuilder sb = new StringBuilder();

            var excelType = Type.GetTypeFromProgID("Excel.Application");

            dynamic excel = Activator.CreateInstance(excelType);
            excel.Visible = true;
            excel.Workbooks.Add();

            sb.Append("Reference;Partname;Package;Value" + Environment.NewLine);
            foreach (ICMPObject cmp in step.GetAllCMPObjects())
            {
                string SValue = cmp.Value;
                if (String.IsNullOrEmpty(SValue)) SValue = "0"; 
                sb.Append(cmp.Ref + ";" + cmp.PartName + ";" + cmp.UsedPackageName + ";" + SValue + Environment.NewLine);
            }


            string LVText = "Reference\tPartname\tPackage\tValue"+ Environment.NewLine + sb.ToString();
            string LVCsv = sb.ToString();
            DataObject LVDataObject = new DataObject();
            LVDataObject.SetData(DataFormats.Text, true, LVText);
            LVDataObject.SetData(DataFormats.CommaSeparatedValue, true, LVCsv);
            Clipboard.SetDataObject(LVDataObject,true);

            excel.ActiveSheet.Paste();

            //release the object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
		}
		
    }
}
