//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script (Synchronous)
// Created on 28.06.2018
// Author Guenther.Schindler
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 28.06.2018
// Description: Enter your description here
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID=MatrixReport_636657914018588414
// ButtonEnabled=1   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)

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
using PCB_Investigator.PCBIWindows;

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
			if(parent.JobIsLoaded)
			{
			List<string> columheader = new List<string>();
            columheader.Add("Layer Name");
            columheader.Add("Layer Thickness");
            columheader.Add("Layer Position");
            PCB_Investigator.PCBIWindows.PCBIResultDialog layerDlg = new PCBIResultDialog(columheader,"Matrix");
			IStep step = parent.GetCurrentStep();
			IMatrix matrix = parent.GetMatrix();
		//	bool toggle = true;
            foreach (string layername in matrix.GetAllLayerNames())
            {
                ListViewItem lvi = new ListViewItem(layername);
               // if (toggle)
              //  {
              //      lvi.BackColor = Color.LightBlue;
             //   }
              //  toggle = !toggle;
               if(matrix.GetMatrixLayerType(layername) == MatrixLayerType.Signal)
                {
                                    lvi.BackColor = Color.Orange;
                }
               if(matrix.GetMatrixLayerType(layername) == MatrixLayerType.Solder_mask)
                {
                                    lvi.BackColor = Color.LightGreen;
                }
               if(matrix.GetMatrixLayerType(layername) == MatrixLayerType.Solder_paste)
                {
                                    lvi.BackColor = Color.LightYellow;
                }
               if(matrix.GetMatrixLayerType(layername) == MatrixLayerType.Dielectric)
                {
                                    lvi.BackColor = Color.GreenYellow;
                }
               if(matrix.GetMatrixLayerType(layername) == MatrixLayerType.Component)
                {
                                    lvi.BackColor = Color.LightSteelBlue;
                }
                double Layerpos = IMath.Mils2MM(MatrixHelpers.GetLayerPositionInStackUp(layername, step, matrix));
                double layerthickness = IMath.Mils2MM(step.GetHeightOfLayer(layername));
                lvi.SubItems.Add(layerthickness.ToString("N4"));
                lvi.SubItems.Add(Layerpos.ToString("N4"));
                layerDlg.AddListViewItem(lvi);
            }
            layerDlg.ShowDlg(PCBIResultDialog.WindowType.Modal);
			parent.UpdateView(); 
			}
		}
	}
   internal static class MatrixHelpers
    {
        internal static double GetLayerPositionInStackUp(string layerName, IStep step, IMatrix matrix)
        {
            double posZ = 0;
            foreach (string name in matrix.GetAllLayerNames())
            {
                if (name.ToUpperInvariant() == layerName.ToUpperInvariant())
                {
                    return posZ;
                }
                if (matrix.GetMatrixLayerType(name) != MatrixLayerType.Component)
                {
                    posZ += step.GetHeightOfLayer(name);
                }
            }
            return posZ;
        }
        internal static List<string> GetInnerSignalLayerNames(IStep step, IMatrix matrix)
        {
            List<string> layerNames = new List<string>();
            string topLayer = matrix.GetTopSignalLayer();
            string botLayer = matrix.GetBotSignalLayer(true);
            if (string.IsNullOrWhiteSpace(topLayer) || string.IsNullOrWhiteSpace(botLayer))
                return layerNames;
            topLayer = topLayer.ToLowerInvariant();
            botLayer = botLayer.ToLowerInvariant();

            foreach (string name in matrix.GetAllSignalLayerNames(true))
            {
                if (name != topLayer && name != botLayer)
                {
                    layerNames.Add(name);
                }
            }
            return layerNames;
        }
    }	
}