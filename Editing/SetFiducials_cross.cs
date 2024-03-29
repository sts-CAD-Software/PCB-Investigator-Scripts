﻿// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Set fix fiducial markers on extra layer.
//-----------------------------------------------------------------------------------
// GUID C0138764-85A3-4960-B95F-62A4486FF427
//
// This script creates two fiducial markers defined as rectangles in the current layer of an open project.
// These markers are placed on the "fiducial" layer and assigned the attributes "fid1" and "fid2". 
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

          //  int shapeIndex = IFilter.AddToolDefinitionDonut(fiducialLayer, (float)IMath.MM2Mils(2),(float) IMath.MM2Mils(1), -1);
           // int shapeIndex = IFilter.AddToolDefinitionRound(fiducialLayer, (float)IMath.MM2Mils(2) -1);
            int shapeIndex = IFilter.AddToolDefinitionRect(fiducialLayer,(float)IMath.MM2Mils(2),-1, (float)IMath.MM2Mils(2), (float)IMath.MM2Mils(1),false);
            int shapeIndex2 = IFilter.AddToolDefinitionRect(fiducialLayer, (float)IMath.MM2Mils(1), -1, (float)IMath.MM2Mils(1), (float)IMath.MM2Mils(2), false);

            // if a cross we can use to rects to create a cross or make a surface
            
            IODBObject pad = filter.CreatePad(fiducialLayer);
            PointD FidPoint = new PointD(IMath.MM2Mils(3), IMath.MM2Mils(3));
            pad.SetSpecifics(new IPadSpecificsD() { Location = FidPoint, ShapeIndex = shapeIndex, Positive = true });
            IAttributeElement attribute = new IAttributeElement(PCBI.FeatureAttributeEnum.fiducial_name);
            attribute.Value = "fid1";
            IAttribute.SetAttribute(attribute, pad);
            
            pad = filter.CreatePad(fiducialLayer);
            pad.SetSpecifics(new IPadSpecificsD() { Location = FidPoint, ShapeIndex = shapeIndex2, Positive = true });
             attribute = new IAttributeElement(PCBI.FeatureAttributeEnum.fiducial_name);
            attribute.Value = "fid1";
            IAttribute.SetAttribute(attribute, pad);


            FidPoint = new PointD(IMath.MM2Mils(3), IMath.MM2Mils(147));
            pad = filter.CreatePad(fiducialLayer);
            pad.SetSpecifics(new IPadSpecificsD() { Location = FidPoint, ShapeIndex = shapeIndex, Positive = true });
            attribute = new IAttributeElement(PCBI.FeatureAttributeEnum.fiducial_name);
            attribute.Value = "fid2";
            IAttribute.SetAttribute(attribute, pad);

            pad = filter.CreatePad(fiducialLayer);
            pad.SetSpecifics(new IPadSpecificsD() { Location = FidPoint, ShapeIndex = shapeIndex2, Positive = true });
            attribute = new IAttributeElement(PCBI.FeatureAttributeEnum.fiducial_name);
            attribute.Value = "fid2";
            IAttribute.SetAttribute(attribute, pad);
            parent.UpdateControlsAndResetView();
		}
    
	}
}
