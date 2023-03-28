//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 16.06.2016
// Autor Fabio.Gruber
// 
// Mark all components in max height regions of drc_comp layer.
// This checks each cmp over the surfaces on relevant layer with drc_max_height attribute.
//-----------------------------------------------------------------------------------
// GUID markHeightRegionTransgression_635963030633639805
//
// The script is used to mark all components in areas with maximum height on the drc_comp layer.
// This is done by checking each component above the surfaces on relevant layers with the drc_max_height attribute.
// The script looks for this attribute on the surfaces touching the respective component.
// If the attribute is present and the height of the part is greater than the drc_max_height value on the surface, the part is highlighted and a hint is added. 
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
			IStep step = parent.GetCurrentStep();
            if (step == null) return;

            string layernameTopHeightRestrictions = "drc_comp";
            ICMPLayer topCMPLayer = step.GetCMPLayer(true); //for bottom change here to false
            ILayer HeightRestirctionLayer = step.GetLayer(layernameTopHeightRestrictions);
            // check each cmp for height restrictions
            foreach (ICMPObject cmp in topCMPLayer.GetAllLayerObjects())
            {
                foreach (IODBObject surface in HeightRestirctionLayer.GetAllObjectInRectangle(cmp.Bounds))
                {
                    if (surface.IsPointOfSecondObjectIncluded(cmp, false)) //intersecting is ok?
                    {
                        //check height of cmp
                        IAttributeElement attributeMaxHeight = null;
                        foreach (IAttributeElement attribute in IAttribute.GetAllAttributes(surface))
                        {
                            if (attribute.AttributeEnum == PCBI.FeatureAttributeEnum.drc_max_height)
                            {
                                attributeMaxHeight = attribute;
                                break;
                            }
                        }
                        if (attributeMaxHeight == null) continue;

                        if (cmp.CompHEIGHT > (double)attributeMaxHeight.Value) //we know this attribute has double values
                        {
                            cmp.ObjectColorTemporary(Color.LightCoral); //change color to highlight cmps
                            //surface.ObjectColorTemporary(Color.Wheat);
                            surface.FreeText = "Check Height of " + cmp.Ref;
                            break;
                        }
                    }
                }
            }
            topCMPLayer.EnableLayer(true);
            HeightRestirctionLayer.EnableLayer(true);
            parent.UpdateView();
		}
		
    }
}