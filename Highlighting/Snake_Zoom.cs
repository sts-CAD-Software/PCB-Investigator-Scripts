//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 29.01.2016
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Jump from left down to right up in small areas to check all board parts.
//-----------------------------------------------------------------------------------
// This script allows you to automatically scan a PCB by jumping from bottom left to top right.
// The goal is to ensure that all parts of the PCB are checked to make sure they were created correctly. 
// The script uses the PCBI Automation API to determine the current step and then calculate the size of the PCB. 
// The script then runs a loop that incrementally increases the size of the section of the PCB and checks that all parts are placed correctly. 
// When all areas have been checked, it returns to the original size of the PCB. 
// The script also includes the ability to pause and resume the run by pressing the 'P' key.
//-----------------------------------------------------------------------------------

using PCBI.Automation;
using System;
using System.Collections.Generic;
using System.Text;
using PCBI.Plugin;
using PCBI.Plugin.Interfaces;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using PCBI.MathUtils;
using System.Reflection;
using System.Threading;

namespace PCBIScript
{
    public class PScript : IPCBIScriptASync
    {
        private bool pause = false;
        public void Execute(PCBI.Automation.IPCBIWindow parent)
        {
            parent.MainForm.KeyPress += MainForm_KeyPress;    
            parent.ZoomHome();
            //example work to highlight pins of gnd net
            IStep curStep = parent.GetCurrentStep();
            if (curStep == null) return;

            RectangleF rect = curStep.GetBounds();
            //do  work here
            int loopX = 0;
            int loopY = 0;
            float tileX = 300; // 300 / 25,4 entspicht 11 mm
            float tileY = 300; // 300 / 25,4 entspicht 11 mm

            while (true)
            {
                Thread.Sleep(500);

                if (isDisposed)
                    break;
                if (pause) continue;
                RectangleF zoomrect = new RectangleF(rect.X + tileX * loopX, rect.Y + tileY * loopY, tileX, tileY);
                loopX++;
                parent.ZoomRect(zoomrect);

                if (tileX * loopX > rect.Width)
                {
                    loopY++;
                    loopX = 0;
                    if (tileY * loopY > rect.Height)
                    {
                        parent.ZoomHome();
                        break;
                    }
                }
            }
        }
        void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // MessageBox.Show(((byte)e.KeyChar).ToString()," char");
            if (((byte)e.KeyChar) == 112) // p
            {
                pause = !pause;
            }
        }
        bool isDisposed = false;
        public void Dispose()
        {
            isDisposed = true;
        }
    }
}
