//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script for DFM Check
// Created on 29.10.2015
// Autor Fabio Gruber
// Updated: 29.09.2016 (Progessbar)
// support@easylogix.de
// 
// Check all nets and find smallest elements of each net.
//-----------------------------------------------------------------------------------
// This script is used to determine the smallest diameter of all elements of a net on a PCB.
// In the Execute method of the script, a progress bar is created and all nets on the current board are traversed.
// For each net, a list of all elements in the net is created and the smallest diameter is determined.
// The results are stored in a StringBuilder and displayed in a MessageBox at the end of the execution. 
//----------------------------------------------------------------------------------

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
        private PCB_Investigator.PCBIWindows.PCBIWorkingDialog wdlg;

        public PScript()
        {
        }


        public void Execute(IPCBIWindow parent)
        {

            wdlg = new PCB_Investigator.PCBIWindows.PCBIWorkingDialog();
            wdlg.SetAnimationStatus(false);
            wdlg.SetStatusPercent(0);
            wdlg.SetStatusText("Working");
            wdlg.CanCancel(true);

            IStep curStep = parent.GetCurrentStep();

            if (curStep == null) return;

            Dictionary<string, double> smallestDiameterList = new Dictionary<string, double>();
            StringBuilder sbResult = new StringBuilder();
            wdlg.ShowWorkingDlgAsThread();

            List<string> netNames = curStep.GetAllNetNames();
            double value = 0;
            double valueStep = ((100.0 / netNames.Count));


            foreach (string netName in curStep.GetAllNetNames())
            {
                INet net = curStep.GetNet(netName);

                wdlg.SetStatusText("Working on " + netName + "...");
                value += valueStep;
                wdlg.SetStatusPercent((int)(value));

                List<IODBObject> allNetElements = net.GetAllNetObjects(parent);
                if (allNetElements.Count == 0) continue;

                double smallestDiameter = allNetElements[0].GetDiameter();
                foreach (IODBObject netElement in allNetElements)
                {

                    double currentDiameter = netElement.GetDiameter();
                    if (currentDiameter < 0) continue; //e.g. surfaces have no diameter 

                    if (currentDiameter < smallestDiameter)
                    {
                        smallestDiameter = currentDiameter;
                    }
                }

                smallestDiameterList.Add(netName, smallestDiameter);
                sbResult.AppendLine(netName + ": " + smallestDiameter.ToString() + " mils");
            }
            wdlg.Dispose();
            PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("All smallest Net Diameters:" + Environment.NewLine + sbResult.ToString(), "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}