//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2017-03-07
// Author EasyLogix
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 2020-08-13
// Updated: 2022-05-09
// Description: Example script how to save TGZ archiv (using tgz.tar.gz).
// There is one parameter to create a sub directory to compress the tgz.
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID Export_TGZ_636154949275554301
// ButtonEnabled=1   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)


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
        public bool CreateCompressedTARGZFile = true;
        public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            if (parent == null || !parent.JobIsLoaded)
            {
                MessageBox.Show("No job loaded.");
                return;
            }

            try
            {
                SaveFileDialog saveTgz = new SaveFileDialog();
                saveTgz.Filter = "TGZ-Archiv(*.tgz)|*.tgz";
                saveTgz.Title = "Save TGZ-Archiv";
                if (saveTgz.ShowDialog() != DialogResult.OK) return;

                var processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = @"C:\Program Files\7-Zip\7z.exe";

                if (!File.Exists(processStartInfo.FileName))
                {
                    MessageBox.Show("This script works only with installation of 7z on your C-Drive. Please change path in code or install 7z first.", "Warning");
                    return;
                }
                string directoryPath = parent.GetODBJobDirectory();
                string tempDirectory = Path.Combine(Path.GetDirectoryName(saveTgz.FileName), Path.GetFileNameWithoutExtension(saveTgz.FileName));
                string fileName = saveTgz.FileName;

                if (CreateCompressedTARGZFile)
                {
                    try
                    {

                        Directory.CreateDirectory(tempDirectory);
                        fileName = Path.Combine(tempDirectory, Path.GetFileName(saveTgz.FileName));
                    }
                    catch
                    {
                        IAutomation.AddToErrorLog("Cannot create directory for tgz compression");
                    }
                }

                string args = "a -ttar " + (CreateCompressedTARGZFile ? fileName : saveTgz.FileName) + " " + directoryPath;

                processStartInfo.Arguments = args;
                var process = System.Diagnostics.Process.Start(processStartInfo);
                process.WaitForExit();

                if (CreateCompressedTARGZFile)
                {
                    string argstwo = "a -tgzip -saa -mx9 " + fileName + ".tar" + " " + fileName;

                    processStartInfo.Arguments = argstwo;
                    var processtwo = System.Diagnostics.Process.Start(processStartInfo);
                    processtwo.WaitForExit();

                    File.Move(fileName + ".tar.gz", saveTgz.FileName);
                    Directory.Delete(tempDirectory, true);
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! " + ex.Message, "Error");
            }

        }

    }
}








