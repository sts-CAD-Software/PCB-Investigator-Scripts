//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script to show layer overview.
// Created on 21.12.2015
// Autor Fabio Gruber
// 
// Show details of all active layers, it will calculate eht area and show matrix information.
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

        public void Execute(IPCBIWindow parentPCBIWindow)
        {
            if (parentPCBIWindow == null) { parentPCBIWindow = IAutomation.CreateNewPCBIWindow(true); return; }

            FormLayerDetails detailsOfActiveLayer = new FormLayerDetails();
            detailsOfActiveLayer.SetInfos(parentPCBIWindow);
            detailsOfActiveLayer.Show();
        }

        public class FormLayerDetails : Form
        {
            public FormLayerDetails()
            {
                InitializeComponent();
                this.Icon = PCB_Investigator.PCBIStyleLibrary.PCBIResources.AreaCalculationIcon();
            }
            #region designer
            /// <summary>
            /// Required designer variable.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary>
            /// Clean up any resources being used.
            /// </summary>
            /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Windows Form Designer generated code

            /// <summary>
            /// Required method for Designer support - do not modify
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
                this.richTextBoxMain = new System.Windows.Forms.RichTextBox();
                this.SuspendLayout();
                // 
                // richTextBoxMain
                // 
                this.richTextBoxMain.Dock = System.Windows.Forms.DockStyle.Fill;
                this.richTextBoxMain.Location = new System.Drawing.Point(0, 0);
                this.richTextBoxMain.Name = "richTextBoxMain";
                this.richTextBoxMain.Size = new System.Drawing.Size(521, 261);
                this.richTextBoxMain.TabIndex = 0;
                this.richTextBoxMain.Text = "";
                // 
                // FormLayerDetails
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(521, 261);
                this.Controls.Add(this.richTextBoxMain);
                this.Name = "FormLayerDetails";
                this.Text = "Layer Details";
                this.ResumeLayout(false);

            }

            #endregion

            private System.Windows.Forms.RichTextBox richTextBoxMain;
            #endregion
            public void SetInfos(IPCBIWindow parentPCBI)
            {
                if (!parentPCBI.JobIsLoaded) return;

                IStep curStep = parentPCBI.GetCurrentStep();
                IMatrix matrix = parentPCBI.GetMatrix();

                if (curStep == null || matrix == null) return; //sollte nicht vorkommen wenn job geladen ist

                PCB_Investigator.PCBIWindows.PCBIWorkingDialog working = new PCB_Investigator.PCBIWindows.PCBIWorkingDialog();
                working.CanCancel(false);
                working.SetAnimationStatus(true);
                working.ShowWorkingDlgAsThread();
                richTextBoxMain.Clear();

                bool mm = parentPCBI.GetUnit();
                richTextBoxMain.Font = new System.Drawing.Font("Verdana", 10);

                working.SetStatusText("Working on Step Information " + curStep.Name + "...");

                float boardArea = curStep.CalculateBoardArea();
                if (mm)
                    boardArea = (float)Math.Round(boardArea * (25.4 * 25.4) / 1000000, 3);

                foreach (ILayer activeLayer in curStep.GetActiveLayerList())
                {
                    richTextBoxMain.AppendText("Layer " + activeLayer.GetLayerName() + Environment.NewLine);
                    richTextBoxMain.AppendText(Environment.NewLine);

                    working.SetStatusText("Working on Layer " + activeLayer.GetLayerName() + "...");

                    #region size of layer
                    RectangleF bounds = activeLayer.GetBounds();

                    richTextBoxMain.AppendText("Bounds: " + Environment.NewLine +
                        "XMin " + (mm ? (IMath.Mils2MM(bounds.Left).ToString("F3") + " mm") : (bounds.Left.ToString("F3") + " mil")) + Environment.NewLine +
                        "YMin " + (mm ? (IMath.Mils2MM(bounds.Top).ToString("F3") + " mm") : (bounds.Top.ToString("F3") + " mil")) + Environment.NewLine +
                        "XMax " + (mm ? (IMath.Mils2MM(bounds.Right).ToString("F3") + " mm") : (bounds.Right.ToString("F3") + " mil")) + Environment.NewLine +
                        "YMax " + (mm ? (IMath.Mils2MM(bounds.Bottom).ToString("F3") + " mm") : (bounds.Bottom.ToString("F3") + " mil")) + Environment.NewLine
                        );
                    richTextBoxMain.AppendText(Environment.NewLine);
                    richTextBoxMain.AppendText("Width  " + (mm ? (IMath.Mils2MM(bounds.Width).ToString("F3") + " mm") : (bounds.Width.ToString("F3") + " mil")) + Environment.NewLine);
                    richTextBoxMain.AppendText("Height " + (mm ? (IMath.Mils2MM(bounds.Height).ToString("F3") + " mm") : (bounds.Height.ToString("F3") + " mil")) + Environment.NewLine);

                    richTextBoxMain.AppendText(Environment.NewLine);
                    #endregion

                    #region area calculation
                    if (activeLayer is IODBLayer)
                    {
                        IODBLayer odbActiveLayer = (IODBLayer)activeLayer;

                        float areaOfLayer = odbActiveLayer.CalculateArea(true);
                        if (mm)
                            richTextBoxMain.AppendText("Used Area   " + (Math.Round(areaOfLayer * (25.4 * 25.4) / 1000000, 3).ToString("F3")) + " of " + boardArea.ToString("F3") + " mm?" + Environment.NewLine);
                        else
                            richTextBoxMain.AppendText("Used Area   " + Math.Round(areaOfLayer, 2).ToString("F2") + " of " + (boardArea.ToString("F2")) + " mils?" + Environment.NewLine);
                    }
                    #endregion

                    #region count elements
                    int counterArc = 0;
                    int counterCMPs = 0;
                    int counterLines = 0;
                    int counterSurfaces = 0;
                    int counterText = 0;
                    int counterPads = 0;

                    foreach (IObject obj in activeLayer.GetAllLayerObjects()) //count all elements
                    {
                        if (obj.Type == IObjectType.Arc)
                            counterArc++;
                        else if (obj.Type == IObjectType.Component)
                            counterCMPs++;
                        else if (obj.Type == IObjectType.Line)
                            counterLines++;
                        else if (obj.Type == IObjectType.Pad)
                            counterPads++;
                        else if (obj.Type == IObjectType.Surface)
                            counterSurfaces++;
                        else if (obj.Type == IObjectType.Text)
                            counterText++;
                    }
                    richTextBoxMain.AppendText(Environment.NewLine);

                    if (counterArc > 0)
                        richTextBoxMain.AppendText("Count Arcs:         " + counterArc.ToString("N0") + Environment.NewLine);
                    if (counterCMPs > 0)
                        richTextBoxMain.AppendText("Count Components:   " + counterCMPs.ToString("N0") + Environment.NewLine);
                    if (counterLines > 0)
                        richTextBoxMain.AppendText("Count Lines:        " + counterLines.ToString("N0") + Environment.NewLine);
                    if (counterPads > 0)
                        richTextBoxMain.AppendText("Count Pads:         " + counterPads.ToString("N0") + Environment.NewLine);
                    if (counterSurfaces > 0)
                        richTextBoxMain.AppendText("Count Area Fills:   " + counterSurfaces.ToString("N0") + Environment.NewLine);
                    if (counterText > 0)
                        richTextBoxMain.AppendText("Count Texts:        " + counterText.ToString("N0") + Environment.NewLine);
                    richTextBoxMain.AppendText(Environment.NewLine);
                    #endregion

                    #region matrix infos
                    MatrixLayerContext context = matrix.GetMatrixLayerContext(activeLayer.GetLayerName());
                    MatrixLayerPolarity polarity = matrix.GetMatrixLayerPolarity(activeLayer.GetLayerName());
                    MatrixLayerType type = matrix.GetMatrixLayerType(activeLayer.GetLayerName());
                    int index = matrix.GetRawIndexByName(activeLayer.GetLayerName());

                    richTextBoxMain.AppendText("Matrix Index " + index + ", Context " + context + ", Polarity " + polarity + ", Type " + type + Environment.NewLine);
                    #endregion

                    richTextBoxMain.AppendText(Environment.NewLine);
                    richTextBoxMain.AppendText(Environment.NewLine);
                    richTextBoxMain.AppendText(Environment.NewLine);
                }

                working.DoClose();
            }
        }
    }
}