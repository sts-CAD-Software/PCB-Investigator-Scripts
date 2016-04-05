//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 05.04.2016
// Autor Fabio.Gruber
// 
// Import gerber and excellon file output from Cadence Allegro (tools defined as comments beginning with ; and much more information like tolerance and quantity).
//-----------------------------------------------------------------------------------
// GUID newScript_635954556024364499

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

        IPCBIWindow Parent;
        bool stop = false;

        public void Execute(IPCBIWindow parent)
        {
            //import excellon file output from Cadence Allegro (tools defined as comments)

            Parent = parent;
            IFilter filter = new IFilter(parent);
            if (!parent.JobIsLoaded)
            {
                filter.CreateAndLoadEmptyJob(Path.GetTempPath(), "GerberImport", "step");

            }
            stop = false;

            string DrillHeader = "Format  : 5.5 / Absolute / INCH / Trailing*" + Environment.NewLine;
            DrillHeader += "Contents: Thru / Drill / Plated*" + Environment.NewLine;
            //   DrillHeader += "M48**" + Environment.NewLine;
            DrillHeader += "FMAT,1*" + Environment.NewLine;
            DrillHeader += "INCH,TZ*" + Environment.NewLine;
            DrillHeader += "ICI,OFF*";

            System.Windows.Forms.OpenFileDialog of = new System.Windows.Forms.OpenFileDialog();
            of.Multiselect = true;
            if (of.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PCB_Investigator.PCBIWindows.PCBIWorkingDialog working = new PCB_Investigator.PCBIWindows.PCBIWorkingDialog();
                working.CanCancel(true);
                working.CancelPressed += working_CancelPressed;
                working.SetStatusText("Parsing files...");
                working.ShowWorkingDlgAsThread();
                double counter = 1;
                double stepOneFile = 100f / of.FileNames.Length;
                foreach (string FileName in of.FileNames)
                {
                    if (stop) break;
                    working.SetStatusPercent((int)counter);
                    FileStream gfs = new System.IO.FileStream(FileName, FileMode.Open);
                    StreamReader gsr = new System.IO.StreamReader(gfs, Encoding.UTF8);
                    string GerberFile = gsr.ReadToEnd();
                    gsr.Close();
                    gfs.Close();
                    string fullPath = Path.GetTempPath() + Path.GetFileName(FileName);
                    counter += stepOneFile;

                    if (File.Exists(fullPath))
                    {
                        try
                        {
                            File.Delete(fullPath);
                            System.Threading.Thread.Sleep(1000); //file delete takes some time...
                        }
                        catch
                        { }
                    }

                    FileStream writeGerberFS = new System.IO.FileStream(fullPath, FileMode.OpenOrCreate);
                    StreamWriter GerberWriterSW = new StreamWriter(writeGerberFS, Encoding.UTF8);
                    if (GerberFile.StartsWith("G"))
                    {
                        GerberWriterSW.WriteLine(GerberFile);
                        GerberWriterSW.Flush();
                        GerberWriterSW.Close();
                        IPCBIWindow.LoadInformation loadInfo = new IPCBIWindow.LoadInformation();
                        Parent.LoadData(fullPath, out loadInfo);
                    }
                    else if (GerberFile.StartsWith(";")) //most files out of cadance start with ;
                    {
                        GerberWriterSW.WriteLine(DrillHeader);

                        foreach (string s in GerberFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            //z.B. Tooldef. bzw. commentarte
                            //z.B. ;   Holesize 1. = 16.000000 Tolerance = +0.000000/-0.000000 PLATED MILS Quantity = 57
                            if (s.StartsWith(";")) //nur wenn keine tooldef dann check Holesize def
                            {
                                #region check for tool definition in comment
                                int indexHolesize = s.IndexOf("Holesize");
                                if (indexHolesize > 0)
                                {

                                    //check = nach holesize
                                    StringBuilder digitsOfDiameter = new StringBuilder();
                                    bool started = false;
                                    for (int i = indexHolesize + 9; i < s.Length; i++)
                                    {
                                        #region get diameter of holesize
                                        if (s[i] == '=')
                                        {
                                            if (started) break;
                                            started = true;
                                        }
                                        else
                                            if (started)
                                            {
                                                if (char.IsNumber(s[i]) || s[i] == '.')
                                                {
                                                    digitsOfDiameter.Append(s[i]);
                                                }
                                                else if (s[i] == ' ' && digitsOfDiameter.Length == 0)
                                                { }
                                                else //anderer Parameter gestartet...
                                                    break;
                                            }
                                        #endregion
                                    }

                                    if (digitsOfDiameter.Length > 0)
                                    {
                                        GerberWriterSW.WriteLine(s.Substring(1, 3) + "C" + digitsOfDiameter.ToString());
                                    }
                                }
                                else GerberWriterSW.WriteLine(s);
                                #endregion
                            }
                        }

                        GerberWriterSW.WriteLine(GerberFile);
                        GerberWriterSW.Flush();
                        GerberWriterSW.Close();
                        IStep step = parent.GetCurrentStep();

                        string drillLayername = step.AddGerberLayer(fullPath, true, PCBI.ImportOptions.FormatTypes.Excellon1, 2, 3, true, true);

                        ILayer drillLayer = step.GetLayer(drillLayername);
                        Dictionary<double, int> diameterShapeList = new Dictionary<double, int>();
                    }
                }
                working.DoClose();

            }
            parent.UpdateControlsAndResetView();
        }
        void working_CancelPressed()
        {
            stop = true;
        }

    }
}