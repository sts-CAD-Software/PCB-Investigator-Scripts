//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 29.04.2014
// Autor Fabio
// 
// Empty template to fill for asynchronous script.
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