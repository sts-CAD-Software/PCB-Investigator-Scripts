//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 29.06.2016
// Autor Guenther
// 
// Empty template to fill for synchronous script.
//-----------------------------------------------------------------------------------
// GUID newScript_636028110537710508
//This script starts the external program "CadenceNetGroup2PCBI.exe" passing the path of the current ODB++ job and the name of the current step as arguments.
//This enables the PCB-Investigator to open and display the ODB++ files created by the Cadence Allegro PCB Editor.
//The script then updates the PCB Investigator view to display the newly loaded data. 
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
using System.Diagnostics;

namespace PCBIScript
{
   public class PScript : IPCBIScript
	{
		public PScript()
		{
		}

		public void Execute(IPCBIWindow parent)
		{
 			//your code here
            ProcessStartInfo psi = new ProcessStartInfo();
		string realPCBI = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + System.IO.Path.DirectorySeparatorChar + "EasyLogix" + System.IO.Path.DirectorySeparatorChar + "PCB-Investigator" + Path.DirectorySeparatorChar + "Scripts" + Path.DirectorySeparatorChar;
            psi.FileName = realPCBI + @"\CadenceNetGroup2PCBI.exe";
            psi.Arguments = parent.GetODBJobDirectory()  + " -step " + parent.GetCurrentStep().Name;
            Process.Start(psi);
			
            parent.UpdateView(); 
		}
		
    }
}