// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Set fix fiducial markers on extra layer.
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
			IFilter filter = new IFilter(parent);

			IStep curStep = parent.GetCurrentStep();
            if (curStep == null)
            {
                MessageBox.Show("No Job loaded, please load a job before start this script!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }

            IODBLayer fiducialLayer = filter.CreateEmptyODBLayer("fiducial", curStep.Name);

            if (fiducialLayer == null)
            {
                MessageBox.Show("Can't create new layer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }

            int shapeIndex = IFilter.AddToolDefinitionDonut(fiducialLayer, 157.48f, 78.74f, 0);

            IODBObject pad = filter.CreatePad(fiducialLayer);
            pad.SetSpecifics(new IPadSpecificsD() { Location = new PCBI.MathUtils.PointD(0236.22, 9803), ShapeIndex = shapeIndex, Positiv = true });

            pad = filter.CreatePad(fiducialLayer);
            pad.SetSpecifics(new IPadSpecificsD() { Location = new PCBI.MathUtils.PointD(14665, 9803), ShapeIndex = shapeIndex, Positiv = true });

            pad = filter.CreatePad(fiducialLayer);
            pad.SetSpecifics(new IPadSpecificsD() { Location = new PCBI.MathUtils.PointD(0236.22, 0236.22), ShapeIndex = shapeIndex, Positiv = true });

            pad = filter.CreatePad(fiducialLayer);
            pad.SetSpecifics(new IPadSpecificsD() { Location = new PCBI.MathUtils.PointD(14665, 0236.22), ShapeIndex = shapeIndex, Positiv = true });

            IMatrix matrix = parent.GetMatrix();
            matrix.UpdateDataAndList();
            parent.UpdateView();
		}
    
	}
}
