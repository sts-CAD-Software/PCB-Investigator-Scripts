//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor 
// 
// Set setup variants for batch file.
//-----------------------------------------------------------------------------------
//EXAMPLE_FOR_STARTUP_SETTINGS.CS 

// The script is used to set a special configuration for the batch mode of the software. 
// It defines various settings for the software,
// e.g. whether colored text is displayed in the layer list, whether transparent surfaces are displayed, whether the control is visible and whether information about nets is displayed on all layers. 
// The script is executed when the batch mode is activated and then sets the specified settings. 
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
