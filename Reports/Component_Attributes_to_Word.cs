//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Export Component Attributes to Excel.
//-----------------------------------------------------------------------------------
// The script exports component properties from the current step view in PCB-Investigator to a Word document.
// It creates a new Word instance, adds a new document, inserts the component properties into the document and closes the Word instance.
// The result is a table with reference name, component name, component package and value. 
//----------------------------------------------------------------------------------

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

            dynamic wordApp = Activator.CreateInstance(Type.GetTypeFromProgID("Word.Application"));
            wordApp.Visible = true;

            dynamic wordDoc = wordApp.Documents.Add();

            foreach (ICMPObject cmp in step.GetAllCMPObjects())
            {
                string SValue = cmp.Value;
                if (String.IsNullOrEmpty(SValue)) SValue = "0";
                sb.Append(cmp.Ref + ";" + cmp.PartName + ";" + cmp.UsedPackageName + ";" + SValue + Environment.NewLine);
            }


            string LVText = "Reference\tPartname\tPackage\tValue" + Environment.NewLine + sb.ToString();

            dynamic rng = wordApp.ActiveDocument.Range(0, 0);
            rng.Text = LVText;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
		}
		
    }
}
