//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2016-01-29
// Author EasyLogix
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 2017-06-13
// Description: Jump from left down to right up in small areas to check all board parts.
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID Snake_Zoom_06F4BBC8-F3B1-41D3-9E6C-CAD407DE5FB3
// ButtonEnabled=1   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)
//----------------------------------------------------------------------------------
//This script is used to place a grid over the board and zoom in small areas from bottom left to top right to check all parts on the board. Each zoom level is held at a specified interval.
//The script responds to the "p" key to pause or resume the script. 
//---------------------------------------------------------------------------------

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
