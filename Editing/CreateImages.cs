// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Create images of layers.
//-----------------------------------------------------------------------------------
// This script saves an image of each component on each layer of a PCB design as a PNG file in a specified folder.
// The script retrieves the current layer and checks if a component layer exists.
// If so, an image of each component on that layer is created for each layer and saved in PNG format in the specified folder.
// The image is created by creating a bitmap object and saving the resulting image as a PNG file. 
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
        string FileLocation = @"C:\Temp\";
        public PScript()
        {
        }

         public void Execute(IPCBIWindow mainWindowPCBI)
        {
            IStep step = mainWindowPCBI.GetCurrentStep();
            if (step == null) return;

            ICMPLayer layerCMPsTop = step.GetCMPLayer(true);
            //exisits the compnenent layer?
            if (layerCMPsTop == null) return;

            foreach (string layername in step.GetAllLayerNames())
            {
                List<ILayer> layers = new List<ILayer>();
                layers.Add(step.GetLayer(layername));
                SaveImageFromComponent(step, layers, FileLocation);
            }
            //something went wrong?
            string errorLog = IAutomation.GetErrorLog();
            if (errorLog.Length > 0)
                System.Diagnostics.Debug.WriteLine(errorLog);
        }
       
        public void SaveImageFromComponent(  IStep step, List<ILayer> layers, string FileLocation)
        {
            if (layers.Count == 0) return;

            RectangleF boundsRelevantCMP = layers[0].GetBounds();
            boundsRelevantCMP.Inflate(100, 100); //show a little bit of the area around the component

            //create the image and save it as png
            PCBI.Automation.IStep.BitmapResultClass imageClass = step.GetBitmap(layers, boundsRelevantCMP, 500, 500);
            if (imageClass != null)
                imageClass.Image.Save(FileLocation + layers[0].GetLayerName() + ".png", System.Drawing.Imaging.ImageFormat.Png);
        } 

    }
}
