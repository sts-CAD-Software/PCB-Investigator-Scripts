// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Example to export data to excel, component references, partnames and packages are written in excel table.
// This can script is a basic example how to export data to MS excel, you can update it to add more information for components or change it a little bit to export e.g. net information.
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
            IStep step = parent.GetCurrentStep();
            if (step == null) return;
            IFilter filter = new IFilter(parent);

            StringBuilder sb = new StringBuilder();

            var excelType = Type.GetTypeFromProgID("Excel.Application");
            dynamic excel = Activator.CreateInstance(excelType);
            excel.Visible = true;
            excel.Workbooks.Add();

            int i = 1;
            //excel.Cells(i, 1).Value = "Reference \t Partname \t Package \t Value";
            //excel.Cells(i, 1).Font.Size = "14";
            //i = i + 1;
            sb.Append("Reference;Partname;Package;Value" + Environment.NewLine);
            foreach (ICMPObject cmp in step.GetAllCMPObjects())
            {
                string SValue = cmp.Value;
                if (String.IsNullOrEmpty(SValue)) SValue = "0"; 
                sb.Append(cmp.Ref + ";" + cmp.PartName + ";" + cmp.UsedPackageName + ";" + SValue + Environment.NewLine);
                //excel.Cells(i, 1).Value = cmp.Ref +";" + cmp.PartName + ";"+ cmp.UsedPackageName +";" + cmp.Value;
                //excel.Cells(i, 1).Font.Size = "14";
                i = i + 1;
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
