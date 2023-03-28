//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 13.06.2016
// Autor Fabio.Gruber
// 
// Check all elements for overlapping paste elements.
//-----------------------------------------------------------------------------------
// GUID newScript_636011629960977253
//
// This script is used to check all elements on the solder mask for overlaps with paste elements. If such overlaps are found, the script marks the relevant locations on the mask.
// The script traverses the various solder mask and paste layers of the current step and calls the MarkMaskOverPadsOnPaste function to check and mark potential overlaps.
// The function does this using various functions of the IPCBI interface to access the objects in the circuit and manipulate their properties. 
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
        Color MarkerColor = Color.LightGoldenrodYellow; //set marker color, you can change it here
        public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            IStep step = parent.GetCurrentStep();
            IMatrix matrix = parent.GetMatrix();

            if (matrix == null || step == null) return; //if there is no data loaded -> return
            ILayer topMaskLayer = step.GetLayer(matrix.FindSideLayerLayerName(MatrixLayerType.Solder_mask, true));
            ILayer topPasteLayer = step.GetLayer(matrix.FindSideLayerLayerName(MatrixLayerType.Solder_paste, true));
            if (topMaskLayer != null && topPasteLayer != null)
                MarkMaskOverPadsOnPaste((IODBLayer)topMaskLayer, (IODBLayer)topPasteLayer, step);

            ILayer botMaskLayer = step.GetLayer(matrix.FindSideLayerLayerName(MatrixLayerType.Solder_mask, false));
            ILayer botPasteLayer = step.GetLayer(matrix.FindSideLayerLayerName(MatrixLayerType.Solder_paste, false));
            if (botMaskLayer != null && botPasteLayer != null)
                MarkMaskOverPadsOnPaste((IODBLayer)botMaskLayer, (IODBLayer)botPasteLayer, step);


            parent.UpdateView(); //redraw to show marked smd pads
        }
        void MarkMaskOverPadsOnPaste(IODBLayer MaskLayer, IODBLayer PasteLayer, IStep step)
        {
            if (PasteLayer == null || MaskLayer == null) return;

            List<IObject> AllMaskPads = MaskLayer.GetAllLayerObjects();
            //look for all pads if there is a paaste element inside?
            foreach (IODBObject maybePad in AllMaskPads)
            {
                if (maybePad.Type != IObjectType.Pad) continue;
                {
                    //check for paste elements
                    List<IObject> relevantPastePads = PasteLayer.GetAllObjectInRectangle(maybePad.Bounds);

                    foreach (IODBObject pasteObj in relevantPastePads)
                    {
                         bool doesIntersect = maybePad.DoesIntersect(pasteObj);
                            if  (!doesIntersect && maybePad.IsPointOfSecondObjectIncluded(pasteObj) )
                                continue; //paste complete in mask

                            if (!doesIntersect && !pasteObj.IsPointOfSecondObjectIncluded(maybePad))
                                continue;

                            maybePad.ObjectColor = MarkerColor;
                            break;
                    }
                }
            }
        }

    }
}