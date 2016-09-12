//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Open one file on specific location
//-----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using PCBI.Automation;

namespace PCBIScript
{
	public class PScript : IPCBIScript
	{
		public PScript()
		{
		}

		public void Execute(IPCBIWindow PCBI_Host)
		{
			LoadInformation Information;
			// Gerber
            PCBI_Host.LoadData(@"D:\D_\jobs\_Jobs\A2z34\gerberfile.ger", out Information);
        }
	
    }
}
