//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Open odb file from fix path.
//-----------------------------------------------------------------------------------
// The script loads an ODB++ job directory and then activates a step within this job.
// The local path to the ODB++ directory must be specified in the code. 
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
        	//put in a local ODB++ directory path here:
            PCBI_Host.LoadODBJob(@"C:\D\Jobs\fez_hacker_1_1_odb");

			IStep step = PCBI_Host.GetStep("fez_hacker");

			if(step ==null) return;
			step.ActivateStep();
        }

    }
}

