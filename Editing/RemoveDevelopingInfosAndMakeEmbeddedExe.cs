//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 06.08.2014
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Helper to remove components and make net names unreadable.
// This script only hide some information, it is possible to recreate the components and document layers.
//-----------------------------------------------------------------------------------
// This script is a helper for removing components and making net names unreadable in a PCB-Investigator project.
// It works by hiding some information, but it is possible to recreate the components and document layers.
// It uses the IPCBIWindow interface to access and manipulate the data in the current PCB-Investigator project.
// The script executes by changing net names, deleting layers that contain components and documents, and destroying packages.
// It saves the modified project as an embedded ZIP file. 
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
			//remove all informations of developing
            string pathForWorkingODB = System.IO.Path.GetTempPath() + "\\example\\WorkingJob"+DateTime.Now.DayOfYear;
            parent.SaveJob(pathForWorkingODB, false);

            if (!parent.LoadODBJob(pathForWorkingODB))
            {
                MessageBox.Show("Can't create a temporary working copy of ODB-Data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            IStep step = parent.GetCurrentStep();
            if (step == null) return;

            int counter = 1;
            foreach (INet net in step.GetNets()) //change netnames
            {
                net.NetName = "Net_" + counter;
                net.ComponentList = new List<INetObject>();
                counter++;
            }

            IMatrix matrix = parent.GetMatrix();
            if (matrix == null) return;

            foreach (string layername in step.GetAllLayerNames())
            {
                if(matrix.GetMatrixLayerType(layername) == MatrixLayerType.Component || matrix.GetMatrixLayerType(layername) == MatrixLayerType.Document)
                    matrix.DelateLayer(layername, true); //remove components and document from the matrix.
            }
			foreach (IPackageSpecifics pack in step.GetAllPackages()) //destroy packages
            {
                pack.SetGraphicsPath(new System.Drawing.Drawing2D.GraphicsPath());
                pack.SetOutline(pack.CreatePolygonPinSurface());
            }

			parent.SaveJobImperative();
            parent.UpdateControlsAndResetView();

			SaveFileDialog saveEmbedded = new SaveFileDialog();
            saveEmbedded.FileName = "embeddedJob.zip";

            if (saveEmbedded.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                parent.SaveEmbeddedAsZIP(saveEmbedded.FileName);
		}
		
    }
}