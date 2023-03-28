//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 23.11.2016
// Autor Fabio.Gruber
// 
// Example script how to open rar archiv, this can easiely be modified for 7z or zip.
//-----------------------------------------------------------------------------------
// GUID newScript_636154949275554301
//
// The script uses the WinRAR software, which must be installed on the C drive.
// If the software is not found, a warning is displayed and the script terminates.
// The archive is unpacked and the contents are stored in a temporary directory.
// Then each folder in the directory is searched and each file is loaded into PCB-Investigator.
// If a file can be loaded, a confirmation message is displayed, otherwise a warning is displayed. 
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
 			try{
 			OpenFileDialog openRar = new OpenFileDialog();
 			openRar.Filter = "Rar-Archiv(*.rar)|*.rar";
 			openRar.Title = "Select RAR-Archiv";
 			if(openRar.ShowDialog()!= DialogResult.OK) return;
			
			var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = @"C:\Program Files\WinRAR\RAR.exe";

            if(!File.Exists(processStartInfo.FileName))
            {
			     MessageBox.Show("This script works only with installation of WinRAR on your C-Drive. Please change path in code or install WinRar first.","Warning" );
                return;
            }
            string directoryPath = Path.GetTempPath()+ DateTime.Now.Ticks+"\\";
            string args =  "x "+openRar.FileName+" "+directoryPath;
            
            processStartInfo.Arguments =args;
            
            var process = System.Diagnostics.Process.Start(processStartInfo);
			
			process.WaitForExit();
			
			DirectoryInfo di = new DirectoryInfo(directoryPath);
			
			foreach(DirectoryInfo diSub in di.GetDirectories())
			{
    			IPCBIWindow.LoadInformation info;
    			
    			if(parent.LoadData(diSub.FullName, out info))
    			{
    			    MessageBox.Show("Job loaded "+diSub.FullName,"Info");
                    break;
    			}
    			else
    			     MessageBox.Show("Not loaded, because "+info.ToString(),"Warning");
			}
            }
            catch (Exception ex)
            {
			     MessageBox.Show("Error! "+ex.ToString(),"Error" );
            
            }
		}
		
    }
}
