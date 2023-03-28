//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Check all elements and calculate bounds.
//-----------------------------------------------------------------------------------
// The script is a PCB Investigator automation script created to check all the elements in a PCB and calculate their boundaries.
// It is used to determine the size of the PCB and provide information about the boundaries of the PCB in mil.
// The script uses the PCB Investigator SDK reference to access the API of the software to calculate the board's boundaries. 
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
		    RectangleF boundsJob = parent.GetJobBounds();
            if (boundsJob.IsEmpty)
            {
                IStep step = parent.GetCurrentStep();
                if (step == null) return;
                boundsJob = step.GetBounds();

                if (boundsJob.IsEmpty)
                {
                    IMatrix matrix = parent.GetMatrix();
                    if (matrix == null) return;

                    RectangleD boundsLayerCombination = new RectangleD();

                    //check signal layers
                    foreach (string layerName in step.GetAllLayerNames())
                    {
                        if (matrix.IsSignalLayer(layerName))
                        {
                            IODBLayer odbLayer = (IODBLayer)step.GetLayer(layerName);

                            RectangleF layerBounds = odbLayer.GetBounds();

                            boundsLayerCombination = IMath.AddRectangleD(boundsLayerCombination, new RectangleD(layerBounds));
                        }
                    }

                    boundsJob = boundsLayerCombination.ToRectangleF();
                }
            }

           
            MessageBox.Show("The Job has following bounds in mils:" + Environment.NewLine + " X " + boundsJob.X.ToString() + " ; Y " + boundsJob.Y.ToString() + " ; width " + boundsJob.Width + " ; height " + boundsJob.Height, "Job Bounds", MessageBoxButtons.OK, MessageBoxIcon.Information);
       
		}
	
   }
}
