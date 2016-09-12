//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 01.03.2016
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Asynchronous script to highlight selection.
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
using System.Threading;
using PCBI.MathUtils;

namespace PCBIScript
{
    public class PScript : IPCBIScriptASync
    {
        public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            //fill code here code
            IStep step = parent.GetCurrentStep();
            bool onOff = false;
            if (step == null) return;

            while (true)
            {
                Thread.Sleep(1000);
                foreach (IODBObject objSel in step.GetSelectedElements())
                {
                    if (onOff)
                    {
                        parent.IColorsetting.DrawOnlySelectedElements = true;
                        objSel.ObjectColorTemporary(Color.White);
                    }
                    else
                        objSel.ObjectColorTemporary(Color.Empty);
                }
                onOff = !onOff;

		parent.UpdateView();
                if (isDisposed)
                    break;
            }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            isDisposed = true;
            //? set back? parent.IColorsetting.DrawOnlySelectedElements = false;
                       
        }

    }
}