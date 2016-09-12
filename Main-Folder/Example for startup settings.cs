//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor 
// 
// Set setup variants for batch file.
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
            parent.ShowStatusText("User Settings defined");
            parent.UseColoredTextInLayerList = false;
            parent.ShowTransparentSurfaces = false;
            parent.StepControlVisible = false;
            parent.ShowNetInfoOnAllLayer = false;
            parent.ShowInfoColumn = false;
            parent.ShowIcon = false;
            parent.ShowFreeTextInfoOnAllLayer = false;
            parent.DragNDropCMPsActiv = false;
            parent.ControlBox = false;
            parent.ActivateCMPsOnNetSelect = true;
            parent.UpdateView(); 
		}
		
    }
}
