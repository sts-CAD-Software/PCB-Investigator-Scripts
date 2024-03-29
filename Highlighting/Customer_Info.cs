//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 28.09.2015
// Autor G?nther Schindler
// 
// Show details of job in graphic pane.
//-----------------------------------------------------------------------------------
// This C# script is intended for automatically displaying details of a PCB layout job in the PCB-Investigator graphical window.
// It uses the IPCBIScriptASync interface to execute asynchronous code.
// The script draws information like selected parts, selected elements, selected reference designers and netlist names of the current step.
// It uses some graphics methods to draw text in different colors and fonts to represent the details of the job. 
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
        IPCBIWindow parent;
        public void Execute(IPCBIWindow Parent)
        {
            //fill code here code
            parent = Parent;
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
            if (!parent.JobIsLoaded) return;


            g.TranslateTransform(1, -1);

            DrawContact(g);

            DrawDate(g);

            DrawSelectedCompsCount(g);

            DrawSelectedFeatureCount(g);

            DrawSelectedRefs(g);

            DrawSelectedNetList(g);
        }

        private  void DrawContact(Graphics g)
        {
            String Text = "www.EasyLogix.de";
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
            String Text = "Selected Components:  " +  parent.GetCurrentStep().GetSelectedCMPsCount().ToString();
            SizeF strSize = g.MeasureString(Text, new Font("Arial", 10));
            Brush TextB = new SolidBrush(Color.FromArgb(180, 0, 250, 250));
            g.FillRectangle(TextB, 0, 140, strSize.Width, strSize.Height);
            g.DrawString(Text, new Font("Arial", 10), Brushes.DarkBlue, new PointF(0, 140));
        }
        private void DrawSelectedFeatureCount(Graphics g)
        {
            String Text = "Selected Features:  " + parent.GetCurrentStep().GetSelectedElementsCount().ToString();
            SizeF strSize = g.MeasureString(Text, new Font("Arial", 10));
            Brush TextB = new SolidBrush(Color.FromArgb(180, 0, 250, 250));
            g.FillRectangle(TextB, 0, 160, strSize.Width, strSize.Height);
            g.DrawString(Text, new Font("Arial", 10), Brushes.DarkBlue, new PointF(0, 160));
        }
        private void DrawSelectedRefs(Graphics g)
        {
            int posY = 0;
            foreach (ICMPObject comp in parent.GetCurrentStep().GetSelectedCMPs())
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
            foreach (IODBObject obj in parent.GetCurrentStep().GetSelectedElements())
            {
                if (!netHash.ContainsKey(obj.NetName))
                {
                    netHash.Add(obj.NetName, obj.NetName);
                }
            }
            foreach(DictionaryEntry entry in  netHash)
            { 
                String Text = "-+- " + entry.Value.ToString();
                SizeF strSize = g.MeasureString(Text, new Font("Tahoma", 10));
                Brush TextB = new SolidBrush(Color.FromArgb(180, 250, 250, 0));
                g.FillRectangle(TextB, 120, 180 + posY, strSize.Width, strSize.Height);
                g.DrawString(Text, new Font("Tahoma", 10), Brushes.Black, new PointF(120, 180 + posY));
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