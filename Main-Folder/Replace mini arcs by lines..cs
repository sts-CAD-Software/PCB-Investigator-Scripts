//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Updated on 26.05.2015
// Autor Fabio Gruber
// www.pcb-investigator.com
// 
// Remove mini arcs by replaceing them as lines on all IODB layers.
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
 			//this script replace mini arcs by lines
            try
            {
                IStep step = parent.GetCurrentStep();

                if (step == null) return;

                double minLenghtArcToReplaceByLine = IMath.MM2Mils(0.02); //20?m maximum for arcs
                IFilter objectCreator = new IFilter(parent);

                foreach (string layerName in step.GetAllLayerNames()) //check all layer
                {
                    ILayer gerberLayer = step.GetLayer(layerName);

                    if (gerberLayer == null) continue;

                    if (gerberLayer is ICMPLayer) continue; //should not be for gerberlayers
                    if (gerberLayer is IPictureLayer) continue; //should not be for gerberlayers

                    //gerber layers always contain IODBObjects
                    foreach (IODBObject obj in gerberLayer.GetAllLayerObjects())
                    {
                        if (obj.Type == IObjectType.Arc)
                        {
                            #region one arc
                            IArcSpecificsD arc = (IArcSpecificsD)obj.GetSpecificsD();

                            double radius = IMath.DistancePointToPoint(arc.Start, arc.Center);
                            double angle = IMath.GetAngle(arc.Start, arc.End, arc.Center, arc.ClockWise); //always >=0
							
                            if (angle > 0) //ODB++ spec define arcs with Start == End as full circles (angle = 0 in this calculation)
                            {  
							double bogen = 2 * Math.PI * radius * angle / 360;

                            if (bogen < minLenghtArcToReplaceByLine && arc.PenWidth > bogen)//simple condition to identify mini arcs
                            {
                                //replace by line
                                IODBObject lineObj = objectCreator.CreateLine((IODBLayer)gerberLayer); //we know it's a gerber layer -> it must be a IODBLayer

                                lineObj.SetSpecifics(new ILineSpecificsD() { Start = arc.Start, End = arc.End, ShapeIndex = arc.ShapeIndex, Positive = arc.Positive });

                                if (!obj.ReplaceItemBy(lineObj)) //replace it
                                    Console.WriteLine("Could not replace item!");
                            } 
							}
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error! "+ex.ToString());
                
            }
            parent.UpdateView(); //to show the replaced elements
	}
		
    }
}