//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 09.06.2015
// Autor Guenther and Fabio
// 
// Copy elements on extra layer and make objects negative to subtract mask form top/bot copper, than calculate area.
//-----------------------------------------------------------------------------------
// The script aims to calculate the copper area.
// It creates a new layer called "copper" on the current PCB.
// Then retrieves the top and bottom copper layers and their masks, and makes the mask objects negative to subtract them from the copper layer.
// The script copies and pastes the mask objects onto the "copper" layer and polygonizes the top and bottom copper layers.
// Finally, the script calculates the area of the copper layer. 
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
        	public IPCBIWindow Parent;
		public void Execute(IPCBIWindow parent)
		{
			Parent = parent;
            		CalculateCopperFinish();
		}
        
       private void SelectAllObjectsOnLayer(IODBLayer layer)        
       {
            foreach (IODBObject obj in layer.GetAllLayerObjects())
            {

                obj.Select(true);
            }
        } 
        private void CalculateCopperFinish()
        {
            IFilter filter = new IFilter(Parent);

            IStep curStep = Parent.GetCurrentStep();
            if (curStep == null)
            {
                MessageBox.Show("No Job loaded, please load a job before start this script!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }
            IODBLayer fiducialLayer = filter.CreateEmptyODBLayer("copper", curStep.Name);

            if (fiducialLayer == null)
            {
                MessageBox.Show("Can't create new layer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                return;
            }

            IMatrix matrix = Parent.GetMatrix();

            string topLayerName = matrix.GetTopSignalLayer();
            string topMaskName = matrix.GetNameByRowIndex(matrix.GetRawIndexByName(topLayerName) - 1);
            string botLayerName = matrix.GetBotSignalLayer();
            string botMaskName = matrix.GetNameByRowIndex(matrix.GetRawIndexByName(botLayerName) + 1);

            IODBLayer topLayer = (IODBLayer)curStep.GetLayer(topLayerName);
            IODBLayer topMaskLayer = (IODBLayer)curStep.GetLayer(topMaskName);
            IODBLayer botLayer = (IODBLayer)curStep.GetLayer(botLayerName);
            IODBLayer botMaskLayer = (IODBLayer)curStep.GetLayer(botMaskName);

            //topLayer.PolygonizeLayer();
            //botLayer.PolygonizeLayer();

            matrix.MakeLayerPositive(topMaskName);
            matrix.MakeLayerPositive(botMaskName);

            Parent.UpdateControlsAndResetView();

            MakeObjectsNegative(topMaskLayer);
            MakeObjectsNegative(botMaskLayer);

            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            SelectAllObjectsOnLayer(topMaskLayer);
            Parent.UIAction.Execute(ID_ActionItem.ID_COPY_SELECTED_ODBOBJECTS);
            topLayer.EnableLayer(true);
            Parent.UIAction.Execute(ID_ActionItem.ID_PASTE_COPIED_ITEMS);

            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            SelectAllObjectsOnLayer(botMaskLayer);
            Parent.UIAction.Execute(ID_ActionItem.ID_COPY_SELECTED_ODBOBJECTS);
            botLayer.EnableLayer(true);
            Parent.UIAction.Execute(ID_ActionItem.ID_PASTE_COPIED_ITEMS);

            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);

            topLayer.PolygonizeLayer();
            botLayer.PolygonizeLayer();
            Parent.UpdateControlsAndResetView();

            Parent.UIAction.Execute(ID_ActionItem.ID_CALCULATE_AREA);
        }        
        private void MakeObjectsNegative(IODBLayer Layer)
        {
            foreach (IODBObject obj in Layer.GetAllLayerObjects())
            {
                
                IObjectSpecifics os = obj.GetSpecifics();
               
                if (os.GetType() == typeof(IPadSpecifics))
                {
                    ((IPadSpecifics)os).Positive = false;
                }
                if (os.GetType() == typeof(IArcSpecifics))
                {
                    ((IArcSpecifics)os).Positive = false;
                }
                if (os.GetType() == typeof(ILineSpecifics))
                {
                    ((ILineSpecifics)os).Positive = false;
                }
                if (os.GetType() == typeof(ISurfaceSpecifics))
                {
                    ((ISurfaceSpecifics)os).Positive = false;
                }
                if (os.GetType() == typeof(ITextSpecifics))
                {
                    ((ITextSpecifics)os).Positive = false;
                }
                obj.SetSpecifics(os);
            }
        }		
    }
}