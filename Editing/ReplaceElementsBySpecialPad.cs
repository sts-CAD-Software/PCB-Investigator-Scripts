//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 31.07.2014
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Replace a selection by one pad with new special symbol.
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
            if (curStep == null) return;

            List<IODBObject> selectedElements = curStep.GetSelectedElements();

            //create list with all selected elements to make a new symbol of it
            List<IObjectSpecificsD> newSymbolSpecs = new List<IObjectSpecificsD>();
            string relLayerName = "";
            PCBI.MathUtils.RectangleD bounds = new PCBI.MathUtils.RectangleD();
            int indexOfLastElement = 1;
            foreach (IODBObject obj in selectedElements)
            {
                newSymbolSpecs.Add(obj.GetSpecificsD());
                relLayerName = obj.GetParentLayerName();
if(bounds == RectangleD.Empty)
{
bounds = obj.GetBoundsD();
}else
{
                bounds = PCBI.MathUtils.RectangleD.Union(bounds, obj.GetBoundsD());
}
                indexOfLastElement = obj.GetIndexOnLayer();
            }

            IODBLayer relLayer = (IODBLayer)curStep.GetLayer(relLayerName);

            if (relLayer == null) return;

            //create new symbol for pads, the name must be unique. We try it with the index of one of the elements.
            int nr = IFilter.AddToolDefinitionSpecial(relLayer, parent, "testsymbol3" + indexOfLastElement, newSymbolSpecs, -bounds.GetMidPoint().X, -bounds.GetMidPoint().Y);

            if (nr < 0)
            {
                //no new symbol was created, maybe name is already existing
                return;
            }

            //delete old elements
            IAutomation.SuppressUserNotifications = false; //otherwise the delete action will be blocked
            parent.UIAction.Execute(ID_ActionItem.ID_DELETE);

            IODBObject pad = filter.CreatePad(relLayer);

            IPadSpecificsD padSpec = (IPadSpecificsD)pad.GetSpecificsD();
            padSpec.Location = bounds.GetMidPoint();
            pad.SetSpecifics(padSpec, nr);
            pad.SetAttribute("new symbol attribute");

            pad.Select(true);

            parent.UpdateView();
        }
		
    }
}