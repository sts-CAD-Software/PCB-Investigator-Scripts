//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate

// Open ODB++ Job on hardcoded path on your local drive D (You have to change the path depending on your directory structure).
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
            PCBI_Host.LoadODBJob(@"D:\jobs\examples\odbjob"); //this works only if the location exists and the folder contains an odb++ design!

			IStep step = PCBI_Host.GetStep("pcb"); //the step name must be correct to find the step with this method.

			if(step ==null) return;
				step.ActivateStep();
        }
		
    }
}

