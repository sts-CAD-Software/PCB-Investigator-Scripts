//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 01.03.2016
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Asynchronous script to highlight selection of CMPs.
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

           static List<ICMPObject> firstSelectionToHighlight;
        public void Execute(IPCBIWindow parent)
        {
            //fill code here code
            IStep step = parent.GetCurrentStep();
            bool onOff = false;
            if (step == null) return;

             firstSelectionToHighlight = step.GetSelectedCMPs();
            step.ClearSelection();

            parent.IColorsetting.IfDrawOnlySelectedTransparencyNotSelectedCMPs = 50;
            parent.IColorsetting.ShowPinNumbers = false;
		IAutomation.RealTimeDrawing = false;

            while (true)
            {
                Thread.Sleep(1000);
                parent.IColorsetting.DrawOnlySelectedElements = false;
                parent.IColorsetting.DrawOnlySelectedCMPs = false;

                foreach (ICMPObject objSel in firstSelectionToHighlight)
                {
                    if (onOff)
                    {
                        objSel.ObjectColorTemporary(Color.Red);
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
            if (firstSelectionToHighlight != null)
            {
                foreach (ICMPObject objSel in firstSelectionToHighlight)
                {
                    objSel.ObjectColorTemporary(Color.Empty);

                }
            }
        }
        

    }
}