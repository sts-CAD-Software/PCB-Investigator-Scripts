//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2015-03-23
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Set Elements to Grid

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
            //your code here
            SetToGrid(100f, parent); // 100 mils

            parent.UpdateView();
        }
        private float correction(float X, float grid)
        {
            float restStartX = X % grid;
            if (restStartX <= grid / 2)
            {
                return X - restStartX;
            }
            else
            {
                return X + (grid - restStartX);
            }
        }
        private void SetToGrid(float grid, IPCBIWindow parent)
        {
            IStep step = parent.GetCurrentStep();
            foreach (IODBObject obj in step.GetSelectedElements())
            {
                IObjectSpecifics obs = obj.GetSpecifics();
                if (obs.GetType() == typeof(ILineSpecifics))
                {
                    ILineSpecifics lines = (ILineSpecifics)obs;
                    lines.Start.X = correction(lines.Start.X, grid);
                    lines.Start.Y = correction(lines.Start.Y, grid);

                    lines.End.X = correction(lines.End.X, grid);
                    lines.End.Y = correction(lines.End.Y, grid);
                    obj.SetSpecifics(lines);
                }
                else
                    if (obs.GetType() == typeof(IPadSpecifics))
                    {
                        IPadSpecifics pads = (IPadSpecifics)obs;

                        pads.Location.X = correction(pads.Location.X, grid);
                        pads.Location.Y = correction(pads.Location.Y, grid);
                        obj.SetSpecifics(pads);
                    }
            }
        }
    }
}