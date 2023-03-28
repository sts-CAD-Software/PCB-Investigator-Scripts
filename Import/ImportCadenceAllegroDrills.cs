//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 11.04.2016
// Autor Fabio.Gruber
// 
// Import gerber and excellon file output from Cadence Allegro (tools defined as comments beginning with ; and much more information like tolerance and quantity).
// To use the script configurate the leading and trailing digits at the script begin.
//-----------------------------------------------------------------------------------
// GUID newScript_635954556024364499
//
// This script can be used to import Gerber and Exzellenon files from Cadence Allegro.
// The files are analyzed and it is checked if the unit MM or MILS is used.
// The user can specify the leading and trailing digits in the script.
// If both Gerber and bore files are loaded, automatic scaling is performed.
// The script then outputs a drill file in INCH format. 
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

        IPCBIWindow Parent;
        bool stop = false;

        public void Execute(IPCBIWindow parent)
        {
            //import excellon file output from Cadence Allegro (tools defined as comments)
            //insert digits for reading inner values of drill file (most no information in drill files, you can have a look on your gerber files to identify it).
            //If you load gerber and drill together there is a auto scaling implemented.
            int LeadingDigits = 2;
            int TrailingDigits = 4;
		bool UseMils = true;


            Parent = parent;
            IFilter filter = new IFilter(parent);
            if (!parent.JobIsLoaded)
            {
                filter.CreateAndLoadEmptyJob(Path.GetTempPath(), "GerberImport", "step");

            }
            stop = false;
		bool TryAutoSize = false;

            string DrillHeader = "Format  : " + LeadingDigits + "." + TrailingDigits + " / Absolute / INCH / Trailing*" + Environment.NewLine;
            DrillHeader += "Contents: Thru / Drill / Plated*" + Environment.NewLine;
            //   DrillHeader += "M48**" + Environment.NewLine;
            DrillHeader += "FMAT,1*" + Environment.NewLine;
            DrillHeader += "INCH,TZ*" + Environment.NewLine;
            DrillHeader += "ICI,OFF*";

		ILayer drillLayer =null;
                
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
                                    bool containsMil = s.Contains("MILS");
                                    bool containsMM = s.Contains("MM");

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
                                        if (containsMM && !containsMil)
                                        {
                                            double diameterTool = ParseHeader(digitsOfDiameter.ToString(), true) / 1000;
                                            GerberWriterSW.WriteLine(s.Substring(1, 3) + "C" + diameterTool.ToString().Replace(",", "."));
                                        }
                                        else
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
				working.DoClose();
			    MessageBox.Show("Parameter"+  Environment.NewLine+"Leading "+LeadingDigits+" Trailing "+TrailingDigits+  Environment.NewLine+"Unit "+(UseMils?"mils":"mm")+  Environment.NewLine ,"Drill Setup");
				IMatrix matrix = parent.GetMatrix();

            		   if (matrix != null) matrix.DelateLayer(System.IO.Path.GetFileName( fullPath), false);
                       
			   string drillLayername = step.AddGerberLayer(fullPath, true, PCBI.ImportOptions.FormatTypes.Excellon1, LeadingDigits, TrailingDigits, UseMils , TryAutoSize );

                        drillLayer = step.GetLayer(drillLayername);
                        Dictionary<double, int> diameterShapeList = new Dictionary<double, int>();
                        
  }
                }
                working.DoClose();

            }
            parent.UpdateControlsAndResetView();

		if(drillLayer!=null)
		drillLayer.EnableLayer(true);

        }
        void working_CancelPressed()
        {
            stop = true;
        }
        private double ParseHeader(string s, bool inMM)
        {
            try
            {
                double num = double.Parse(s, System.Globalization.NumberStyles.Any );
                if (inMM)
                {
                    return IMath.MM2Mils(num)/1000;
                }
                return num;
            }
            catch
            {
                return 1;//default size
            }
        }
    }
}