//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 06.06.2016
// Autor Fabio.Gruber
// 
// Mark all SMD pads with drill inside, this checks both sides of your PCB. Round Pads are ignored.
//-----------------------------------------------------------------------------------
// This script marks all surface mount device (SMD) pads on a PCB layout that have corresponding drill holes.
// The program searches for all SMD pads on both the top and bottom layers of the PCB and checks if there is a corresponding drill hole on any of the layers associated with that pad.
// If a corresponding drill hole is found, the SMD pad is marked with a specific color (LightGoldenrodYellow by default) to make it easier to identify. 
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
            IODBLayer topLayer = step.GetOutsideODBLayer(true);
            if (topLayer != null)
            {
                MarkAllSMDPadsOnLayer(topLayer, matrix.GetAllDrillLayersForThisLayer(topLayer.LayerName), step);
                topLayer.EnableLayer(true); //to see the result for top side immidiatly, it will be enabled
            }
            IODBLayer botLayer = step.GetOutsideODBLayer(false);
            if (botLayer != null)
                MarkAllSMDPadsOnLayer(botLayer, matrix.GetAllDrillLayersForThisLayer(botLayer.LayerName), step);

            parent.UpdateView(); //redraw to show marked smd pads
        }

        void MarkAllSMDPadsOnLayer(IODBLayer layer, List<string> allRelevantDrills, IStep step)
        {
            if (layer == null || allRelevantDrills.Count == 0) return;
            Dictionary<string, IODBLayer> drills = new Dictionary<string, IODBLayer>();

		//look for all pads if there is a drill inside?
            foreach (IODBObject maybePad in layer.GetAllLayerObjects())
            {
                if (maybePad.Type != IObjectType.Pad || maybePad.ObjectColor == MarkerColor) continue;

                Dictionary<PCBI.FeatureAttributeEnum, string> attribs = maybePad.GetAttributesDictionary();

                if (attribs.ContainsKey(PCBI.FeatureAttributeEnum.smd)) //check for smd attribute
                {
                    //check for drills
                     IPadSpecifics pad = (IPadSpecifics)maybePad.GetSpecifics();
 			if (pad.Type == PCBI.Symbol_Type.r) continue; //no round pads
                    foreach (string drillname in allRelevantDrills)
                    {
                        if (!drills.ContainsKey(drillname))
                            drills.Add(drillname, (IODBLayer)step.GetLayer(drillname));

                        List<IObject> relevantDrills = drills[drillname].GetAllObjectInRectangle(maybePad.GetBounds());

                        foreach (IODBObject drill in relevantDrills)
                        {
                            if (maybePad.Bounds.Contains(drill.Bounds)) //simple check with bounds inside
                            {
                                maybePad.ObjectColor = MarkerColor;
                                break;
                            }
                        }
                        if (!maybePad.ObjectColor.IsEmpty) //if color -> found and break
                            break;
                    }
                }
            }
        }

    }
}