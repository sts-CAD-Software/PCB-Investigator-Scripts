//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Open odb file from fix path.
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
            PCBI_Host.LoadODBJob(@"C:\D\Jobs\2014\fez_hacker_1_1_odb\fez_hacker_1_1_odb");

			IStep step = PCBI_Host.GetStep("fez_hacker");

			if(step ==null) return;
			step.ActivateStep();
        }

    }
}

