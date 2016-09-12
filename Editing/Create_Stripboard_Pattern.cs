//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2015-03-23
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Create a stripboard PCB
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
        IPCBIWindow Parent;
        public void CreatePads()
        {
            IFilter filter = new IFilter(Parent);

            IStep curStep = Parent.GetCurrentStep();
            if (curStep == null)
            {
                MessageBox.Show("No Job loaded, please load a job before start this script!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }
            IODBLayer fiducialLayer = filter.CreateEmptyODBLayer("copper", curStep.Name);

            if (fiducialLayer == null)
            {
                MessageBox.Show("Can't create new layer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }

            float outerDiameter = 1800;
            float innerDiameter = 1000;

            int shapeIndex = IFilter.AddToolDefinitionDonut(fiducialLayer, outerDiameter / 25.4f, innerDiameter / 25.4f, 0);

            PCBI.MathUtils.PointD startLine = new PCBI.MathUtils.PointD(0, 0);
            float distance = 2540 / 25.4f;
            for (int y = 0; y < 39; y++)
            {
                for (int i = 0; i < 63; i++)
                {

                    IODBObject pad = filter.CreatePad(fiducialLayer);
                    IPadSpecificsD ps = new IPadSpecificsD();
                    ps.Location = new PCBI.MathUtils.PointD(startLine.X, startLine.Y);
                    ps.ShapeIndex = shapeIndex;
                    ps.Positive = true;
                    pad.SetSpecifics(ps);
                    startLine.X += distance;
                }
                startLine.Y += distance;
                startLine.X = 0;
            }
            IMatrix matrix = Parent.GetMatrix();
            matrix.UpdateDataAndList();
            Parent.UpdateView();
        }
        public void CreateDrills()
        {
            IFilter filter = new IFilter(Parent);

            IStep curStep = Parent.GetCurrentStep();
            if (curStep == null)
            {
                MessageBox.Show("No Job loaded, please load a job before start this script!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }
            IODBLayer fiducialLayer = filter.CreateEmptyODBLayer("drill", curStep.Name);

            if (fiducialLayer == null)
            {
                MessageBox.Show("Can't create new layer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }

            float outerDiameter = 1000;

            int shapeIndex = IFilter.AddToolDefinitionRound(fiducialLayer, outerDiameter / 25.4f, 0);

            PCBI.MathUtils.PointD startLine = new PCBI.MathUtils.PointD(0, 0);
            float distance = 2540 / 25.4f;
            for (int y = 0; y < 39; y++)
            {
                for (int i = 0; i < 63; i++)
                {

                    IODBObject pad = filter.CreatePad(fiducialLayer);
                    IPadSpecificsD ps = new IPadSpecificsD();
                    ps.Location = new PCBI.MathUtils.PointD(startLine.X, startLine.Y);
                    ps.ShapeIndex = shapeIndex;
                    ps.Positive = true;
                    pad.SetSpecifics(ps);
                    startLine.X += distance;
                }
                startLine.Y += distance;
                startLine.X = 0;
            }
            IMatrix matrix = Parent.GetMatrix();
            matrix.UpdateDataAndList();
            Parent.UpdateView();
        }
        public void CreateCopperLines()
        {
            IFilter filter = new IFilter(Parent);

            IStep curStep = Parent.GetCurrentStep();
            if (curStep == null)
            {
                MessageBox.Show("No Job loaded, please load a job before start this script!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }
            IODBLayer fiducialLayer = filter.CreateEmptyODBLayer("copper", curStep.Name);

            if (fiducialLayer == null)
            {
                MessageBox.Show("Can't create new layer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }

            float outerDiameter = 1800;

            int shapeIndex = IFilter.AddToolDefinitionRound(fiducialLayer, outerDiameter / 25.4f, 0);

            PCBI.MathUtils.PointD startLine = new PCBI.MathUtils.PointD(0, 0);
            float distance = 2540 / 25.4f;
            for (int y = 0; y < 39; y++)
            {
                float lastX = 0;
                for (int i = 0; i < 63; i++)
                {

                    IODBObject line = filter.CreateLine(fiducialLayer);
                    ILineSpecificsD ps = new ILineSpecificsD();
                    ps.Start = new PCBI.MathUtils.PointD(startLine.X, startLine.Y);
                    float lineEnd = lastX + distance;
                    ps.End = new PCBI.MathUtils.PointD(lineEnd, startLine.Y);
                    lastX = lineEnd;
                    ps.ShapeIndex = shapeIndex;
                    ps.Positive = true;
                    line.SetSpecifics(ps);
                    startLine.X += distance;
                }
                startLine.Y += distance;
                startLine.X = 0;
            }
            IMatrix matrix = Parent.GetMatrix();
            matrix.UpdateDataAndList();
            Parent.UpdateView();
        }
        public void Execute(IPCBIWindow parent)
        {
            Parent = parent;
            CreatePads();
            CreateDrills();
            CreateCopperLines();
        }
    }
}
