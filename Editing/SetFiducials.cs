// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Set fix fiducial markers on extra layer.
//-----------------------------------------------------------------------------------
// GUID C0138764-85A3-4960-B95F-62A4486FF427
//
// This script creates fiducial markers on a PCB. It creates a new layer named "fiducial" and then adds two donut pads with the defined dimensions and positions.
// Each pad is given an attribute name "fiducial_name" and a value of "fid1".
// The script will run when a job is loaded and will display a warning message if no job is loaded or the new layer cannot be created. 
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

            int shapeIndex = IFilter.AddToolDefinitionDonut(fiducialLayer, (float)IMath.MM2Mils(2),(float) IMath.MM2Mils(1), -1);

            IODBObject pad = filter.CreatePad(fiducialLayer);

            PointD FidPoint = new PointD(IMath.MM2Mils(3), IMath.MM2Mils(3));
            pad.SetSpecifics(new IPadSpecificsD() { Location = FidPoint, ShapeIndex = shapeIndex, Positive = true });
            IAttributeElement attribute = new IAttributeElement(PCBI.FeatureAttributeEnum.fiducial_name);
            attribute.Value = "fid1";
            IAttribute.SetAttribute(attribute, pad);

            FidPoint = new PointD(IMath.MM2Mils(3), IMath.MM2Mils(147));
            pad = filter.CreatePad(fiducialLayer);
            pad.SetSpecifics(new IPadSpecificsD() { Location = FidPoint, ShapeIndex = shapeIndex, Positive = true });
            attribute = new IAttributeElement(PCBI.FeatureAttributeEnum.fiducial_name);
            attribute.Value = "fid1";
            IAttribute.SetAttribute(attribute, pad);
            parent.UpdateControlsAndResetView();
		}
    
	}
}
