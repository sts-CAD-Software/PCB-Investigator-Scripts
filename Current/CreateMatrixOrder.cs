//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 21.01.2015
// Autor Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
//
// Create Matrix from layer name extentions
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
 			//your code here
            PCBI.Automation.IMatrix matrix = parent.GetMatrix();
	     PCBI.Automation.IStep step = parent.GetCurrentStep();
            SetMatrixOrder(matrix,step);
            matrix.Save();
            parent.UpdateControlsAndResetView();
            parent.UpdateView();			
            parent.UpdateView(); 
		}
        private void SetMatrixOrder(PCBI.Automation.IMatrix Matrix, PCBI.Automation.IStep Step)
        {
            if (Step == null || Matrix == null) return;

            

            string SilkScreenTop = "sst";
            string PastMaskTop = "pto";
            string SolderMaskTop = "mto";
            string copperTop = "top";
            string copperInner1 = "i1";
            string copperInner2 = "i2";
            string copperInner3 = "i3";
            string copperInner4 = "i4";
            string copperInner5 = "i5";
            string copperInner6 = "i6";
            string copperInner7 = "i7";
            string copperInner8 = "i8";
            string copperBottom = "bot";
            string SolderMaskBottom = "mbo";
            string PastMaskBottom = "pbo";
            string SilkScreenBottom = "ssb";

            string drillPlated = "dpl";
            string drillUnPlated = "dup";


            int position = 1;

            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(SilkScreenTop))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Silk_screen, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(PastMaskTop))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Solder_paste, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(SolderMaskTop))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Solder_mask, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
               }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperTop))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperInner1))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperInner2))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperInner3))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperInner4))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperInner5))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperInner6))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperInner7))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperInner8))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(copperBottom))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Signal, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(SolderMaskBottom))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Solder_mask, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(PastMaskBottom))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Solder_paste, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(SilkScreenBottom))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Silk_screen, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
               }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(drillPlated))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Drill, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }
            foreach (string layerName in Step.GetAllLayerNames())
            {
                if (layerName.ToLower().EndsWith(drillUnPlated))
                {
                    Matrix.SetMatrixLayerParameter(layerName, MatrixLayerContext.Board, MatrixLayerPolarity.Positive, MatrixLayerType.Drill, 0, 0);
                    Matrix.SetMatrixLayerIndex(layerName, position);
                    position++;
                }
            }

        }
		
    }
}