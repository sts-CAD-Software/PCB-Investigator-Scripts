//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 11.11.2015
// Autor Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
//
// Create Matrix from layer name extentions like PCB-Investigator gerber output create it.
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
            PCBI.Automation.IMatrix matrix = parent.GetMatrix();
            PCBI.Automation.IStep step = parent.GetCurrentStep();
            SetMatrixOrder(matrix, step);
            matrix.Save();
            parent.UpdateControlsAndResetView();
            parent.UpdateView();
            parent.UpdateView();
        }
        private void SetMatrixOrder(PCBI.Automation.IMatrix Matrix, PCBI.Automation.IStep Step)
        {
            if (Step == null || Matrix == null) return;

            List<string> MatrixOrder = new List<string>();
            List<string> TopLayers = new List<string>();
            List<string> BotLayers = new List<string>();
            List<string> EndLayers = new List<string>();

            foreach (string LayerName in Step.GetAllLayerNames(true))
            {
			string layerName =LayerName.Replace(".ger","");
			layerName =LayerName.Replace(".gbr","");

                if (layerName.EndsWith("silk_screen"))
                {
                    Matrix.SetMatrixLayerParameter(LayerName , MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Silk_screen, 0, 0);
                }
                else if (layerName.EndsWith("solder_paste"))
                {
                    Matrix.SetMatrixLayerParameter(LayerName , MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Solder_paste, 0, 0);
                }
                else if (layerName.EndsWith("solder_mask"))
                {
                    Matrix.SetMatrixLayerParameter(LayerName , MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Solder_mask, 0, 0);
                }
		  else if (layerName.EndsWith("power_ground"))
                {
                    Matrix.SetMatrixLayerParameter(LayerName , MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Power_ground, 0, 0);
                }
                else if (layerName.EndsWith("signal"))
                {
                    Matrix.SetMatrixLayerParameter(LayerName , MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    //if (layerName.Contains("top")) //this do not work, because it will be sorted at the end
                    //    MatrixOrder.Insert(0, layerName);
                    //else
                        MatrixOrder.Add(LayerName ); //always in main list
                    continue;
                }
                else if (layerName.EndsWith("drill"))
                {
                    Matrix.SetMatrixLayerParameter(LayerName , MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Drill, 0, 0);
                    EndLayers.Add(LayerName );
                    continue;
                }
                else if (layerName.EndsWith("rout"))
                {
                    Matrix.SetMatrixLayerParameter(LayerName , MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Rout, 0, 0);
                    EndLayers.Add(LayerName );
                    continue;
                }
                else if(layerName.EndsWith("document"))
                {
                    EndLayers.Add(LayerName );
                    continue;
                }

                if (layerName.Contains("top"))// || layerName.Contains("t")) //second check is dangerous depending on your layer naming
                    TopLayers.Add(LayerName );
                else if (layerName.Contains("bot") || layerName.Contains("b"))
                    BotLayers.Add(LayerName );
                else
                    MatrixOrder.Add(LayerName );
            }
            MatrixOrder.Sort();
            TopLayers.Sort();
            BotLayers.Sort();
            EndLayers.Sort();
            MatrixOrder.InsertRange(0, TopLayers);
            MatrixOrder.AddRange(BotLayers);
            MatrixOrder.AddRange(EndLayers);
            Matrix.SetMatrixOrder(MatrixOrder);
        }
    }
}