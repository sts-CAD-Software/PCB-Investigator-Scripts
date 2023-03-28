//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2016-04-27
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Create special info to ODB++ objects and write it in "Freetext" field.
// (Update V2)
//-----------------------------------------------------------------------------------
// Depending on the selected "TextType", the method iterates over the PCB objects and adds free text to the objects describing various properties of the object.
// For example, the script calculates the length of lines and pads and adds the value as free text to the respective object.
// There is also a method that calculates the length of a net connection and adds the value as free text to the objects in question.
// The free text information field is displayed on all PCB layers and the view is updated. 
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
        public enum TextType
        {
            angle,
            lenght,
            NetLength,
            ViaCount,
            NetOnLayer,
            area
        }
        public float distancePointToPoint(PointF a, PointF b)
        {
            float deltaX = a.X - b.X;
            float deltaY = a.Y - b.Y;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
        private double CalculateNetLenth(List<IODBObject> list)
        {

            double distance = 0;
            foreach (IODBObject odbObj in list)
            {
                IObjectSpecifics lineSpec = odbObj.GetSpecifics();
                if (lineSpec.GetType() == typeof(ILineSpecifics))
                {
                    distance += distancePointToPoint(((ILineSpecifics)lineSpec).Start, ((ILineSpecifics)lineSpec).End);
                }
            }
            return distance;
        }
        public void Execute(IPCBIWindow parent)
        {
            if (parent == null) return;
            TextType tt = TextType.angle;
            ShowInputDialog(ref tt);

            // MessageBox.Show( tt.ToString());
            IMatrix m = parent.GetMatrix();
            IStep step = parent.GetCurrentStep();
            if (tt == TextType.angle)
            {


                foreach (string layerName in m.GetAllSignalLayerNames())
                {
                    IODBLayer layer = (IODBLayer)step.GetLayer(layerName);

                    foreach (IODBObject obj in layer.GetAllLayerObjects())
                    {
                        IObjectSpecifics os = obj.GetSpecifics();
                        if (os.GetType() == typeof(ILineSpecifics))
                        {
                            ILineSpecifics lineEdges = (ILineSpecifics)os;
                            double angle = PCBI.MathUtils.IMath.GetAngle(PCBI.MathUtils.PointD.FromPointF(lineEdges.Start), PCBI.MathUtils.PointD.FromPointF(lineEdges.End));
                            obj.FreeText = "? " + angle.ToString();
                        }
                        if (os.GetType() == typeof(IPadSpecifics))
                        {
                            IPadSpecifics PAD = (IPadSpecifics)os;
                            double angle = PAD.Rotation;
                            obj.FreeText = "? " + angle.ToString();
                        }
                    }
                }
            }
            if (tt == TextType.lenght)
            {

                foreach (string layerName in m.GetAllSignalLayerNames())
                {
                    IODBLayer layer = (IODBLayer)step.GetLayer(layerName);

                    foreach (IODBObject obj in layer.GetAllLayerObjects())
                    {
                        IObjectSpecifics os = obj.GetSpecifics();
                        if (os.GetType() == typeof(ILineSpecifics))
                        {
                            ILineSpecifics lineEdges = (ILineSpecifics)os;

                            double length = PCBI.MathUtils.IMath.DistancePointToPoint(PCBI.MathUtils.PointD.FromPointF(lineEdges.Start), PCBI.MathUtils.PointD.FromPointF(lineEdges.End)) / 100;
                            if (parent.GetUnit())
                            {
                                length *= 2.54f;
                            }
                            length = Math.Round(length, 2);
                            obj.FreeText = "L: " + length.ToString();
                        }
                        if (os.GetType() == typeof(IPadSpecifics))
                        {
                            IPadSpecifics PAD = (IPadSpecifics)os;
                            RectangleF bounds = obj.GetBounds();
                            double w = bounds.Width / 100;
                            double h = bounds.Height / 100;

                            if (parent.GetUnit())
                            {
                                w *= 2.54f;
                                h *= 2.54f;
                            }
                            w = Math.Round(w, 2);
                            h = Math.Round(h, 2);
                            obj.FreeText = "w: " + w.ToString() + " h: " + h.ToString();
                        }
                    }
                }
            }
            if (tt == TextType.ViaCount)
            {
                foreach (INet NetList in step.GetNets())
                {
                    int drillCount = 0;
                    foreach (IODBObject obj in NetList.GetAllNetObjects(parent))
                    {
                        if (m.GetMatrixLayerType(obj.GetParentLayerName()) == MatrixLayerType.Drill)
                        {
                            drillCount++;
                        }
                    }
                    foreach (IODBObject obj in NetList.GetAllNetObjects(parent))
                    {
                        obj.FreeText = "DrillCount: " + drillCount.ToString();
                    }
                }
            }
            if (tt == TextType.NetOnLayer)
            {
                foreach (INet NetList in step.GetNets())
                {
                    string LNames = "";
                    foreach (string LayerNames in NetList.GetAllUsedLayers(step))
                    {
                        if (m.GetMatrixLayerType(LayerNames) == MatrixLayerType.Signal)
                            LNames += LayerNames + "; ";

                    }
                    foreach (IODBObject obj in NetList.GetAllNetObjects(parent))
                    {
                        obj.FreeText = "layer: " + LNames;
                    }
                }
            }
            if (tt == TextType.NetLength)
            {

                foreach (INet NetList in step.GetNets())
                {
                    double length = CalculateNetLenth(NetList.GetAllNetObjects(parent)) / 100;
                    if (parent.GetUnit())
                    {
                        length *= 2.54f;
                    }
                    foreach (IODBObject obj in NetList.GetAllNetObjects(parent))
                    {
                        obj.FreeText = "Netlength: " + Math.Round(length, 2);
                    }
                }
            }
            parent.ShowFreeTextInfoOnAllLayer = true;
            parent.UpdateView();
        }

        private static DialogResult ShowInputDialog(ref TextType input)
        {
            System.Drawing.Size size = new System.Drawing.Size(250, 210);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Input";


            System.Windows.Forms.RadioButton radioButtonAngle;
            System.Windows.Forms.RadioButton radioButtonTraceLength;
            System.Windows.Forms.RadioButton radioButtonNetLength;
            System.Windows.Forms.RadioButton radioButtonViaCount;
            System.Windows.Forms.RadioButton radioButtonArea;
            System.Windows.Forms.RadioButton radioButtonNetOnLayer;
            System.Windows.Forms.Button buttonOK;
            radioButtonAngle = new System.Windows.Forms.RadioButton();
            radioButtonTraceLength = new System.Windows.Forms.RadioButton();
            radioButtonNetLength = new System.Windows.Forms.RadioButton();
            radioButtonViaCount = new System.Windows.Forms.RadioButton();
            radioButtonArea = new System.Windows.Forms.RadioButton();
            radioButtonNetOnLayer = new System.Windows.Forms.RadioButton();
            buttonOK = new System.Windows.Forms.Button();
            // 
            // radioButtonAngle
            // 
            radioButtonAngle.AutoSize = true;
            radioButtonAngle.Checked = true;
            radioButtonAngle.Location = new System.Drawing.Point(27, 23);
            radioButtonAngle.Name = "radioButtonAngle";
            radioButtonAngle.Size = new System.Drawing.Size(52, 17);
            radioButtonAngle.TabIndex = 0;
            radioButtonAngle.TabStop = true;
            radioButtonAngle.Text = "Angle";
            radioButtonAngle.UseVisualStyleBackColor = true;
            // 
            // radioButtonTraceLength
            // 
            radioButtonTraceLength.AutoSize = true;
            radioButtonTraceLength.Location = new System.Drawing.Point(27, 46);
            radioButtonTraceLength.Name = "radioButtonTraceLength";
            radioButtonTraceLength.Size = new System.Drawing.Size(89, 17);
            radioButtonTraceLength.TabIndex = 1;
            radioButtonTraceLength.Text = "Trace Length";
            radioButtonTraceLength.UseVisualStyleBackColor = true;
            // 
            // radioButtonNetLength
            // 
            radioButtonNetLength.AutoSize = true;
            radioButtonNetLength.Location = new System.Drawing.Point(27, 69);
            radioButtonNetLength.Name = "radioButtonNetLength";
            radioButtonNetLength.Size = new System.Drawing.Size(78, 17);
            radioButtonNetLength.TabIndex = 2;
            radioButtonNetLength.Text = "Net Length";
            radioButtonNetLength.UseVisualStyleBackColor = true;
            // 
            // radioButtonViaCount
            // 
            radioButtonViaCount.AutoSize = true;
            radioButtonViaCount.Location = new System.Drawing.Point(27, 92);
            radioButtonViaCount.Name = "radioButtonViaCount";
            radioButtonViaCount.Size = new System.Drawing.Size(71, 17);
            radioButtonViaCount.TabIndex = 3;
            radioButtonViaCount.Text = "Via Count";
            radioButtonViaCount.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea
            // 
            radioButtonArea.AutoSize = true;
            radioButtonArea.Location = new System.Drawing.Point(27, 115);
            radioButtonArea.Name = "radioButtonArea";
            radioButtonArea.Size = new System.Drawing.Size(47, 17);
            radioButtonArea.TabIndex = 4;
            radioButtonArea.Text = "Area";
            radioButtonArea.UseVisualStyleBackColor = true;
            // 
            // radioButtonNetOnLayer
            // 
            radioButtonNetOnLayer.AutoSize = true;
            radioButtonNetOnLayer.Location = new System.Drawing.Point(27, 138);
            radioButtonNetOnLayer.Name = "radioButtonNetOnLayer";
            radioButtonNetOnLayer.Size = new System.Drawing.Size(86, 17);
            radioButtonNetOnLayer.TabIndex = 5;
            radioButtonNetOnLayer.Text = "Net on Layer";
            radioButtonNetOnLayer.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(159, 148);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(64, 31);
            buttonOK.TabIndex = 6;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;

            // 
            // Form_FreeText
            // 
            inputBox.Controls.Add(buttonOK);
            inputBox.Controls.Add(radioButtonNetOnLayer);
            inputBox.Controls.Add(radioButtonArea);
            inputBox.Controls.Add(radioButtonViaCount);
            inputBox.Controls.Add(radioButtonNetLength);
            inputBox.Controls.Add(radioButtonTraceLength);
            inputBox.Controls.Add(radioButtonAngle);
            inputBox.Name = "Form_FreeText";
            inputBox.Text = "Free Text";


            DialogResult result = inputBox.ShowDialog();


            if (radioButtonAngle.Checked)
            {
                input = TextType.angle;
            }
            if (radioButtonTraceLength.Checked)
            {
                input = TextType.lenght;
            }
            if (radioButtonNetLength.Checked)
            {
                input = TextType.NetLength;
            }

            if (radioButtonViaCount.Checked)
            {
                input = TextType.ViaCount;
            }
            if (radioButtonNetOnLayer.Checked)
            {
                input = TextType.NetOnLayer;
            }
            if (radioButtonArea.Checked)
            {
                input = TextType.area;
            }

            return result;
        }
    }
}
