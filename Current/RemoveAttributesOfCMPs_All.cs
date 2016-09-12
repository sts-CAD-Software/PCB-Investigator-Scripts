//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 27.05.2015
// Autor Gruber Fabio
// 
// Clear Attributes of components. 
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
            IMatrix m = parent.GetMatrix();
            IStep step = parent.GetCurrentStep();


            foreach (ICMPObject comp in step.GetAllCMPObjects())
            {
                Dictionary<string ,string> attv = comp.GetComponentAttributeDictionary();

                foreach (string att in attv.Keys)
                {
                    comp.RemoveAttribute(att);
                }
            }
            parent.UpdateView(); 
         }
    }
}