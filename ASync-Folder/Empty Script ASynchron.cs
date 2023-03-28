//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 29.04.2014
// Autor easylogix
// 
// Empty template to fill for asynchronous script. This example is only for adding your own code, you can also create this empty template in scripting engine dialog directly.
//-----------------------------------------------------------------------------------
//This script is an empty template for an asynchronous script in PCB-Investigator.
//It does not contain any specific functions or methods, but only serves as a starting point for adding custom code.
//The "PScript" class implements the IPCBIScriptASync interface and contains the "Execute" method that executes the custom code.
//The script also contains an infinite loop that checks every second to see if the script has finished.
//The Dispose method is used to properly terminate the script when it is no longer needed.
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
   public class PScript : IPCBIScriptASync
	{
		public PScript()
		{
		}

		public void Execute(IPCBIWindow parent)
		{
			//add code here

			while (true)
            		{
                	Thread.Sleep(1000);

                	if (isDisposed)
                    		break;
			}
		}
		
		bool isDisposed = false;
        public void Dispose()
        {
            isDisposed = true;
        }
    }
}
