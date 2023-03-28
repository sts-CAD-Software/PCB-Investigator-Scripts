//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 12.09.2016
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Create an image to get an overview of all signal layers (including drills and components).
//-----------------------------------------------------------------------------------
// GUID newScript_635900140114543121
// EnableOn JobOpen
// Category Overview
//
//The script creates an image that gives an overview of all signal lines (including holes and components).
//The user is prompted for the number of rows desired for the signal line image arrangement. Then, for each signal layer, the corresponding image is created.
//In this process, the signal layer is activated and all other layers are deactivated. Holes and components can also be integrated into the image.
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
using Microsoft.VisualBasic;
using System.Drawing.Imaging;
namespace PCBIScript
{
    public class PScript : IPCBIScript
    {
        public PScript()
        {
        }

          public void Execute(IPCBIWindow parent)
        {
            //set all signal layers in one image
            string Rows = Microsoft.VisualBasic.Interaction.InputBox("How many rows do you wish?", "Setup", "2");
            int rows = 2;

            try
            {
                rows = int.Parse(Rows);
            }
            catch (Exception re)
            {
                MessageBox.Show("Please provide a number\n" + re.ToString());
            }

            string pathForImage = System.IO.Path.GetTempPath() + "imageComplete.png"; //output somewhere
            stopImaging = false;            //internal bool to use cancel button
            float DPI = 1000;               //in pixel foreach layer
            int maxWidtBMP = 40000;         //image class is dangerous for big RAM usage!

            int countImagesEachLine = rows;    //maximum of image in one line
            bool AddDrillsToImage = true;  //see where are the connections to other layers
            bool AddCompToImageAsSeperateLayer = true;  //see where are the connections to other layers
            IStep step = parent.GetCurrentStep();
            IMatrix matrix = parent.GetMatrix();
            if (step == null || matrix == null) return;
            PCB_Investigator.PCBIWindows.PCBIWorkingDialog working = null;
            try
            {
                working = new PCB_Investigator.PCBIWindows.PCBIWorkingDialog(); //show waiting dialog, because for some projects it is very slow
                working.CanCancel(true);
                working.CancelPressed += working_CancelPressed;
                working.SetAnimationStatus(false);
                working.ShowWorkingDlgAsThread();
                int curValPercentage = 0;
                working.SetStatusPercent(curValPercentage);

                List<string> allSignalLayerNames = matrix.GetAllSignalLayerNames();
                if (!AddCompToImageAsSeperateLayer)
                {
                    allSignalLayerNames.Insert(0, matrix.GetTopComponentLayer());
                    allSignalLayerNames.Add(matrix.GetBotComponentLayer());
                }
                List<ILayer> activeLayerList = step.GetActiveLayerList(); //remember active layer list to set them later back
                step.TurnOffAllLayer();

                int maxHeight = 0;
                int maxWidth = 0;
                int completeWidth = 0;
                int lines = 1;
                bool setBackHatch = parent.IColorsetting.UseHatchMode;
                Color setBackColor = parent.IColorsetting.SelectionColor;

                parent.IColorsetting.SelectionColor = Color.White; //highlight selection better with plane color
                parent.IColorsetting.UseHatchMode = false;

                RectangleF boundsStep = step.GetBounds();
                boundsStep.Inflate(20, 20);

                int height = (int)(boundsStep.Height * DPI / 1000);
                int width = (int)(boundsStep.Width * DPI / 1000); //set the step bounds with DPI
                if (height > maxHeight)
                    maxHeight = height;

                curValPercentage = 10;
                working.SetStatusPercent(curValPercentage);

                Dictionary<string, System.Drawing.Bitmap> allLayerImages = new Dictionary<string, System.Drawing.Bitmap>();
                int counterLayers = 0;
                foreach (string layername in allSignalLayerNames)
                {
                    if (stopImaging) break;
                    counterLayers++;
                    working.SetStatusText("working on " + layername);
                    curValPercentage += 80 / allSignalLayerNames.Count; //we want only the part in the middle of the progress bar...
                    working.SetStatusPercent(curValPercentage);
                    Bitmap resultImage;
                    if (step.GetLayer(layername) is IODBLayer)
                    {

                        IODBLayer signalLayer = (IODBLayer)step.GetLayer(layername);
                        signalLayer.EnableLayer(true);
                        if (AddDrillsToImage)
                        {
                            List<ILayer> drawList = new List<ILayer>();
                            foreach (string layerDrillName in matrix.GetAllDrillLayersForThisLayer(layername))
                            {
                                drawList.Add(step.GetLayer(layerDrillName));
                            }
                            drawList.Add(signalLayer);
                            if (AddCompToImageAsSeperateLayer)
                            {
                                if (matrix.GetTopSignalLayer() == layername)
                                {
                                    ILayer layer = step.GetLayer(matrix.GetTopComponentLayer());
                                    if(layer!= null)
                                        drawList.Add(layer);
                                }
                                if (matrix.GetBotSignalLayer() == layername)
                                {
                                    ILayer layer = step.GetLayer(matrix.GetBotComponentLayer());
                                    if(layer!= null)
                                        drawList.Add(layer);
                                }
                            }
                            resultImage = step.GetBitmap(drawList, boundsStep, width, height, true, false, false, false);
                            // public Bitmap GetBitmap(	List<ILayer> Layers,RectangleF DetailRectangle,	int Width,	int Height,	bool DrawPCBOutline,	bool FillBoardOutline,	bool ShowComponentDetails,	bool IgnoreSelection,	bool DrawOnlySelection,	out RectangleF DrawnRectangle
                      
                            Bitmap copyBitmap = new Bitmap(resultImage); //do copy the image before the old one will be disposed

                            resultImage = copyBitmap;
                        }
                        else
                        {
                            resultImage = step.GetBitmap(new List<ILayer>() { signalLayer }, boundsStep, width, height, true, false, false, false);
                        }
                        if(resultImage!= null)
                         allLayerImages.Add(layername, resultImage); //remember the layer image later
                        signalLayer.DisableLayer();
                    }
                    else if (!AddCompToImageAsSeperateLayer && step.GetLayer(layername) is ICMPLayer)
                    {
                        ICMPLayer cmpLayer = (ICMPLayer)step.GetLayer(layername);
                        resultImage = step.GetBitmap(new List<ILayer>() { cmpLayer }, boundsStep, width, height, true, false, true, false);

                        allLayerImages.Add(layername, resultImage); //remember the layer image later
                    }

                    if (completeWidth + width > maxWidtBMP || counterLayers > countImagesEachLine) //max image size
                    {
                        lines++;
                        completeWidth = width;
                        counterLayers = 1;
                    }
                    else
                        completeWidth += width;

                    if (completeWidth > maxWidth)
                        maxWidth = completeWidth;


                }
                working.SetStatusText("built new image...");

                working.SetStatusPercent(85);
                Bitmap bitComplete = null;
                bool needDownScale = false;

                try
                {
                    bitComplete = new Bitmap(maxWidth, lines * (maxHeight + 100) + 60); //depending on your design this constructor sometimes throw an exception
                }
                catch
                {
                    bitComplete = new Bitmap(maxWidth / 10, lines * (maxHeight + 1000) / 10 + 60);
                    needDownScale = true; //depending on your machine settings and RAM it can make problems by creating big images.
                }

                parent.IColorsetting.UseHatchMode = setBackHatch;
                parent.IColorsetting.SelectionColor = setBackColor;

                Graphics g = Graphics.FromImage(bitComplete);
                g.Clear(Color.Transparent);
                int Xoff = 0;
                int Yoff = 0;
                working.SetStatusText("set images in one... ");

                Font f = new Font("Arial", needDownScale ? 20 : 40); //we need a font to write layernames
                int counterImagesInLine = 0;
                foreach (string layer in allLayerImages.Keys)
                {
                    counterImagesInLine++;
                    if (needDownScale && (Xoff + allLayerImages[layer].Width / 10 > bitComplete.Width || counterImagesInLine > countImagesEachLine))
                    {
                        Yoff += maxHeight / 10 + 100; //space for text
                        Xoff = 0;
                        counterImagesInLine = 1;
                    }
                    else if (Xoff + allLayerImages[layer].Width > maxWidtBMP || counterImagesInLine > countImagesEachLine)
                    {
                        Yoff += maxHeight + 100; //space for text
                        Xoff = 0;
                        counterImagesInLine = 1;
                    }
                    if (needDownScale)
                        g.DrawImage(allLayerImages[layer], new RectangleF(Xoff, Yoff + 100, width / 10, height / 10), new RectangleF(0, 0, allLayerImages[layer].Width, allLayerImages[layer].Height), GraphicsUnit.Pixel);
                    else
                        g.DrawImage(allLayerImages[layer], new Point(Xoff, Yoff + 100));

                    g.DrawString(layer, f, Brushes.Red, new PointF(Xoff + 50, Yoff + 30)); //name of layer

                    if (needDownScale)
                        Xoff += allLayerImages[layer].Width / 10;
                    else
                        Xoff += allLayerImages[layer].Width;


                }
                string textForFooter = parent.GetJobName() + " - creation date: " + DateTime.Now.ToString() + "    www.PCB-Investigator.com";

                SizeF size = g.MeasureString(textForFooter, f);

                g.DrawString(textForFooter, f, Brushes.White, new PointF((bitComplete.Width - size.Width) / 2, bitComplete.Height - 60)); //name of layer

                working.SetStatusPercent(91);

                foreach (ILayer layerActive in activeLayerList)
                    layerActive.EnableLayer(true);
                working.SetStatusPercent(95);

                try
                {
                    working.SetStatusText("saving result...");
                    bitComplete.Save(pathForImage, ImageFormat.Png);

                    //open image with standard viewer
                    System.Diagnostics.Process.Start(pathForImage);
                }
                catch
                { }
                working.DoClose();

            }
            catch (Exception ex)
            {
                if (working != null) working.DoClose();

                MessageBox.Show("Can't create image, because " + Environment.NewLine + ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
              bool stopImaging = false;
        void working_CancelPressed()
        {
            stopImaging = true;
        }
    }
}
