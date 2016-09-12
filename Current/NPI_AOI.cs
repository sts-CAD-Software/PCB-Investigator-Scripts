//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 04.03.2016
// Autor Guenther Schindler
// 
// Use AOI images to verify fist assembled PCB
// Display images and report component data on screen
// on screen HMI
//-----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using PCBI.Plugin;
using PCBI.Plugin.Interfaces;
using System.Windows.Forms;
using System.Drawing;
using PCBI.Automation;
using System.Collections;
using System.IO;
using System.Drawing.Drawing2D;
using System.Threading;
using PCBI.MathUtils;

namespace PCBIScript
{
   public class PScript : IPCBIScriptASync
	{
		public PScript()
		{
		}
        IPCBIWindow pcbi_parent;
        public void Execute(IPCBIWindow Parent)
        {
            //fill code here code
            pcbi_parent = Parent;
            pcbi_parent.IColorsetting.DrawOnlySelectedCMPs = true;
            pcbi_parent.IColorsetting.BackgroundColor = Color.Black;
            Parent.PCBIFormGraphicPaneDrawing += new IPCBIWindow.DrawingEventHandler(Parent_PCBIFormGraphicPaneDrawing);
            while (true)
            {
                Thread.Sleep(1000);

                if (isDisposed)
                {
                    Parent.PCBIFormGraphicPaneDrawing -= new IPCBIWindow.DrawingEventHandler(Parent_PCBIFormGraphicPaneDrawing);
                    break;
                }
            }
        }
        void Parent_PCBIFormGraphicPaneDrawing(Graphics g, int ClientWidth, int ClientHeight)
        {
            if (!pcbi_parent.JobIsLoaded) return;


            g.TranslateTransform(1, -1);
            DrawComponentBox(g);
            DrawContact(g);
            DrawComponentInfo(g);

        }
        private void DrawComponentBox(Graphics g)
        {

            foreach (ICMPObject cmp in pcbi_parent.GetCurrentStep().GetSelectedCMPs())
            {
                int inflate = 120;
                RectangleF cmpRect = cmp.GetBounds();
                Point p1 = pcbi_parent.WorldToClient(new PointF(cmpRect.Left, cmpRect.Bottom));
                Point p2 = pcbi_parent.WorldToClient(new PointF(cmpRect.Left + cmpRect.Width, cmpRect.Bottom - cmpRect.Height));
                Rectangle draw = new Rectangle(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
                draw.Inflate(inflate, inflate);
                //Rectangle draw = new Rectangle(p1.X, p1.Y, 111, 111);
                //MessageBox.Show("", draw.Width.ToString() + " " + draw.Height.ToString());
                int boarderWidth = 5;
                Pen p = new Pen(Color.FromArgb(250,255,255,0), 2);
                //left bottom corner
                g.DrawRectangle(p, draw.Left - boarderWidth, draw.Bottom, boarderWidth, boarderWidth);
                // left top corner
                g.DrawRectangle(p, draw.Left - boarderWidth, draw.Top - boarderWidth, boarderWidth, boarderWidth);
                //right bottom corner
                g.DrawRectangle(p, draw.Right, draw.Bottom, boarderWidth, boarderWidth);
                // right top corner
                g.DrawRectangle(p, draw.Right, draw.Top - boarderWidth, boarderWidth, boarderWidth);
                //Pen prect = new Pen(Color.FromArgb(255, 255, 255, 255), 5);
                //prect.DashStyle = DashStyle.DashDotDot;
                //g.DrawRectangle(prect, draw);
                //draw.Inflate(25, 25);
                //prect = new Pen(Color.FromArgb(250, 50, 50, 255), 5);
                //g.DrawRectangle(prect, draw);

                 //supress top rect
                 p1 = pcbi_parent.WorldToClient(new PointF(cmpRect.Left, cmpRect.Bottom));
                 p2 = pcbi_parent.WorldToClient(new PointF(cmpRect.Left + cmpRect.Width, cmpRect.Bottom - cmpRect.Height));
                 Rectangle supressrect = new Rectangle(0, 0, pcbi_parent.MainForm.Size.Width, p1.Y - inflate);
                SolidBrush b = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
                g.FillRectangle(b, supressrect);

                //supress bot rect
                supressrect = new Rectangle(0, p2.Y + inflate, pcbi_parent.MainForm.Size.Width, p2.Y + inflate);
                g.FillRectangle(b, supressrect);

                //supress left rect
                supressrect = new Rectangle(0, p1.Y - inflate, p1.X - inflate, p2.Y - p1.Y + inflate * 2);
                g.FillRectangle(b, supressrect);
                //supress right rect
                supressrect = new Rectangle(p2.X + inflate, p1.Y - inflate, pcbi_parent.MainForm.Size.Width - p2.Y, p2.Y - p1.Y + inflate*2);
                g.FillRectangle(b, supressrect);

                //// Draw Pin 1 ************************************************************************************************

                RectangleF pinRect = cmp.GetPin(0).GetBounds(cmp);
                p1 = pcbi_parent.WorldToClient(new PointF(pinRect.Left, pinRect.Bottom));
                p2 = pcbi_parent.WorldToClient(new PointF(pinRect.Left + pinRect.Width, pinRect.Bottom - pinRect.Height));
                 draw = new Rectangle(p1.X , p1.Y, p2.X - p1.X, p2.Y - p1.Y);
                 draw.Inflate(5, 5);
                //Rectangle draw = new Rectangle(p1.X, p1.Y, 111, 111);
                //MessageBox.Show("", draw.Width.ToString() + " " + draw.Height.ToString());
                 boarderWidth = 5;
                 p = new Pen(Color.FromArgb(255, 150, 150, 255), 5);
                //left bottom corner
                g.DrawRectangle(p, draw.Left - boarderWidth, draw.Bottom, boarderWidth, boarderWidth);
                // left top corner
                g.DrawRectangle(p, draw.Left - boarderWidth, draw.Top - boarderWidth, boarderWidth, boarderWidth);
                //right bottom corner
                g.DrawRectangle(p, draw.Right, draw.Bottom, boarderWidth, boarderWidth);
                //right top corner
                g.DrawRectangle(p, draw.Right, draw.Top - boarderWidth, boarderWidth, boarderWidth);

                ///second
                
                draw.Inflate(2, 2);
                //Rectangle draw = new Rectangle(p1.X, p1.Y, 111, 111);
                //MessageBox.Show("", draw.Width.ToString() + " " + draw.Height.ToString());
                boarderWidth = 4;
                p = new Pen(Color.FromArgb(255, 0, 0, 50), 2);
                //left bottom corner
                g.DrawRectangle(p, draw.Left - boarderWidth, draw.Bottom, boarderWidth, boarderWidth);
                // left top corner
                g.DrawRectangle(p, draw.Left - boarderWidth, draw.Top - boarderWidth, boarderWidth, boarderWidth);
                //right bottom corner
                g.DrawRectangle(p, draw.Right, draw.Bottom, boarderWidth, boarderWidth);
                //right top corner
                g.DrawRectangle(p, draw.Right, draw.Top - boarderWidth, boarderWidth, boarderWidth);
                //prect = new Pen(Color.FromArgb(150, 255, 255, 0), 5);
                //prect.DashStyle = DashStyle.DashDotDot;
                //g.DrawRectangle(prect, draw);


                break;
            }
        }
        private void DrawComponentInfo(Graphics g)
        {

            int xPos = 30;
            int posY = 80;
            foreach (ICMPObject cmp in pcbi_parent.GetCurrentStep().GetSelectedCMPs())
            {
                String Text = cmp.Ref;
                SizeF strSize = g.MeasureString(Text, new Font("Arial", 20));
                Brush TextB = new SolidBrush(Color.FromArgb(180, 250, 250, 0));
                Brush TextBRef = new SolidBrush(Color.FromArgb(180, 250, 250, 240));
                g.FillRectangle(TextBRef, xPos, posY, strSize.Width, strSize.Height);
                g.DrawString(Text, new Font("Arial", 20), Brushes.Black, new PointF(xPos,posY));
                posY += 40;
                Text = "Part Name: " + cmp.PartName;
                strSize = g.MeasureString(Text, new Font("Arial", 10));
                g.FillRectangle(TextB, xPos,  posY, strSize.Width, strSize.Height);
                g.DrawString(Text, new Font("Arial", 10), Brushes.Black, new PointF(xPos, posY));
                posY += 20;
                Text = "Package Name: " + cmp.UsedPackageName;
                strSize = g.MeasureString(Text, new Font("Arial", 10));
                g.FillRectangle(TextB, xPos, posY, strSize.Width, strSize.Height);
                g.DrawString(Text, new Font("Arial", 10), Brushes.Black, new PointF(xPos, posY));
                posY += 20;
                foreach (string attrib in cmp.GetComponentAttributes())
                {
                    Text = attrib;
                    if (Text.ToLower().Contains("name") |
                        Text.ToLower().StartsWith("number") |
                        Text.ToLower().StartsWith("value") |
                        Text.ToLower().StartsWith("ipn"))
                    {
                        strSize = g.MeasureString(Text, new Font("Arial", 10));
                        g.FillRectangle(TextB, xPos, posY, strSize.Width, strSize.Height);
                        g.DrawString(Text, new Font("Arial", 10), Brushes.Black, new PointF(xPos, posY));
                        posY += 20;
                    }
                }
                //foreach (string attrib in cmp.GetGeometryAttributes())
                //{
                //    Text = attrib;
                //    strSize = g.MeasureString(Text, new Font("Arial", 10));
                //    g.FillRectangle(TextB, xPos,posY, strSize.Width, strSize.Height);
                //    g.DrawString(Text, new Font("Arial", 10), Brushes.Black, new PointF(xPos, posY));
                //    posY += 20;
                //}
                break;
            }
        }
        private  void DrawContact(Graphics g)
        {
            String Text = pcbi_parent.GetJobName();// "www.EasyLogix.de";
            SizeF strSize = g.MeasureString(Text, new Font("Arial", 40));
            Brush TextB = new SolidBrush(Color.FromArgb(180, 255, 250, 230));
            g.FillRectangle(TextB, 0, 5, strSize.Width, strSize.Height);
            g.DrawString(Text, new Font("Arial", 40), Brushes.DarkBlue, new PointF(0, 5));
        }
        private  void DrawDate(Graphics g)
        {
            String Text = DateTime.Now.ToLongDateString();
            SizeF strSize = g.MeasureString(Text, new Font("Arial", 20));
            Brush TextB = new SolidBrush(Color.FromArgb(180, 0, 150, 150));
            g.FillRectangle(TextB, 0, 80, strSize.Width, strSize.Height);
            g.DrawString(Text, new Font("Arial", 20), Brushes.DarkBlue, new PointF(0, 80));
        }

        private  void DrawSelectedCompsCount(Graphics g)
        {
            String Text = "Selected Components:  " +  pcbi_parent.GetCurrentStep().GetSelectedCMPsCount().ToString();
            SizeF strSize = g.MeasureString(Text, new Font("Arial", 10));
            Brush TextB = new SolidBrush(Color.FromArgb(180, 0, 250, 250));
            g.FillRectangle(TextB, 0, 140, strSize.Width, strSize.Height);
            g.DrawString(Text, new Font("Arial", 10), Brushes.DarkBlue, new PointF(0, 140));
        }
        private void DrawSelectedFeatureCount(Graphics g)
        {
            String Text = "Selected Features:  " + pcbi_parent.GetCurrentStep().GetSelectedElementsCount().ToString();
            SizeF strSize = g.MeasureString(Text, new Font("Arial", 10));
            Brush TextB = new SolidBrush(Color.FromArgb(180, 0, 250, 250));
            g.FillRectangle(TextB, 0, 160, strSize.Width, strSize.Height);
            g.DrawString(Text, new Font("Arial", 10), Brushes.DarkBlue, new PointF(0, 160));
        }
        private void DrawSelectedRefs(Graphics g)
        {
            int posY = 0;
            foreach (ICMPObject comp in pcbi_parent.GetCurrentStep().GetSelectedCMPs())
            {
                String Text = comp.Ref;
                SizeF strSize = g.MeasureString(Text, new Font("Arial", 10));
                Brush TextB = new SolidBrush(Color.FromArgb(180, 250, 250, 250));
                g.FillRectangle(TextB, 20, 180 + posY, strSize.Width, strSize.Height);
                g.DrawString(Text, new Font("Arial", 10), Brushes.Black, new PointF(20, 180 + posY));
                posY += 20;
                if (posY > 800) break;
            }
        }
        private void DrawSelectedNetList(Graphics g)
        {
            int posY = 0;
           Hashtable netHash = new Hashtable();
            foreach (IODBObject obj in pcbi_parent.GetCurrentStep().GetSelectedElements())
            {
                if (!netHash.ContainsKey(obj.NetName))
                {
                    netHash.Add(obj.NetName, obj.NetName);
                }
            }
            foreach(DictionaryEntry entry in  netHash)
            { 
                String Text = entry.Value.ToString();
                SizeF strSize = g.MeasureString(Text, new Font("Arial", 10));
                Brush TextB = new SolidBrush(Color.FromArgb(180, 250, 250, 0));
                g.FillRectangle(TextB, 120, 180 + posY, strSize.Width, strSize.Height);
                g.DrawString(Text, new Font("Arial", 10), Brushes.Black, new PointF(120, 180 + posY));
                posY += 20;
                if (posY > 800) break;
            }
        }
        bool isDisposed = false;
       public void Dispose()
        {
           isDisposed = true;
        }
		
    }
}