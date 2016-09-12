//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 29.02.2016
// Autor Guenther easylogix.de
// 
// Toglle through the layer stackup.
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
using System.Windows.Input;

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
            IMatrix matrix = parent.GetMatrix();

            parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);
            List<string> layernames = matrix.GetAllLayerNames();
            int count = layernames.Count;
            int loopLayer = 0;
            loopLayer = layernames.IndexOf(matrix.GetTopSignalLayer());
            while (true)
            {
                Thread.Sleep(1000);
                
                if (isDisposed)
                    break;
                if (pause) continue;
                
                if (loopLayer > 0) curStep.GetLayer(layernames[loopLayer - 1]).DisableLayer();
                curStep.GetLayer(layernames[loopLayer]).EnableLayer(true);
                parent.UpdateView();

                loopLayer++;
                if (loopLayer > count) break;
                if(loopLayer > layernames.IndexOf(matrix.GetBotSignalLayer())) break;
            }
        }

        void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
           // MessageBox.Show(((byte)e.KeyChar).ToString()," char");
            if(((byte)e.KeyChar) == 112) // p
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
