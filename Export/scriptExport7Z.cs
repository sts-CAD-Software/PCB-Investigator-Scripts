//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 23.11.2016
// Autor Fabio.Gruber
// 
// Example script how to save 7z archiv, this can easiely be modified for tgz or rar.
//-----------------------------------------------------------------------------------
// GUID newScript_636154949275554301
//
// This script is an example of how to create and save a 7z archive.
// The script shows how to open a SaveFileDialog to select the location and name of the archive.
// Then the archive is created using the 7-Zip tool, which must be installed on the D-drive.
// The archive is created from the ODB++ job directory and recursively all contained files and folders are added. 
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
 			SaveFileDialog saveTgz = new SaveFileDialog();
 			saveTgz.Filter = "Tgz-Archiv(*.7z)|*.7z";
 			saveTgz.Title = "Save 7z-Archiv";
 			if(saveTgz.ShowDialog()!= DialogResult.OK) return;
			
			var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = @"D:\Program Files\7-Zip\7z.exe";

            if(!File.Exists(processStartInfo.FileName))
            {
			    MessageBox.Show("This script works only with installation of 7z on your D-Drive. Please change path in code or install 7z first.","Warning" );
                return;
            }
            string directoryPath = parent.GetODBJobDirectory();
            string args =  "a -r "+saveTgz.FileName+" "+directoryPath;
            //example from there faq 7z.exe a -r c:\a.7z "C:\Data"
			
			processStartInfo.Arguments =args;
            
            var process = System.Diagnostics.Process.Start(processStartInfo);
			
			process.WaitForExit();
            }
            catch (Exception ex)
            {
			     MessageBox.Show("Error! "+ex.ToString(),"Error" );
            
            }
		}
		
    }
}

