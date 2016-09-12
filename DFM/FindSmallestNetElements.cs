//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script for DFM Check
// Created on 29.10.2015
// Autor Fabio Gruber
// support@easylogix.de
// 
// Check all nets and find smallest elements of each net.
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
            IStep curStep =  parent.GetCurrentStep();

            if (curStep == null) return;

            Dictionary<string, double> smallestDiameterList = new Dictionary<string, double>();
            StringBuilder sbResult = new StringBuilder();

            foreach (string netName in curStep.GetAllNetNames())
            {
                INet net = curStep.GetNet(netName);

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
                sbResult.AppendLine(netName + ": " + smallestDiameter.ToString()+" mils");
            }

            PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("All smallest Net Diameters:" + Environment.NewLine + sbResult.ToString(), "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
	}	
}