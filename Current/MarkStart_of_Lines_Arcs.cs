//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 25.09.2015
// Autor G?nther Schindler
// 
// Show Line and Arc area of selection with start highlighting.
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

            foreach (IODBObject obj in parent.GetCurrentStep().GetSelectedElements())
            {
                if (obj.GetSpecifics() is ILineSpecifics)
                {
                    ILineSpecifics ls = (ILineSpecifics)obj.GetSpecifics();
                    Pen pen = new Pen(Color.Aquamarine, 10);
                    pen.DashOffset = 4;
                    //pen.DashPattern = 0.5f;
                    pen.DashStyle = DashStyle.DashDotDot;
                    pen.StartCap = LineCap.Triangle;
                    pen.EndCap = LineCap.ArrowAnchor;

                    g.DrawLine(pen, parent.WorldToClient(ls.Start), parent.WorldToClient(ls.End));


                    Point locatioStart = parent.WorldToClient( ls.Start);
                    locatioStart = new Point(locatioStart.X - 20, locatioStart.Y - 20);
                    RectangleF SRect = new RectangleF(locatioStart,new Size(40,40));
                    Pen PStart = new Pen(Color.AliceBlue, 4);
                    g.DrawEllipse(PStart, SRect);

                    }
                else if (obj.GetSpecifics() is IArcSpecifics)
                {
                    IArcSpecifics aspec = (IArcSpecifics)obj.GetSpecifics();
                    Pen pen = new Pen(Color.Yellow, 10);
                    pen.DashOffset = 4;
                    //pen.DashPattern = 0.5f;
                    pen.DashStyle = DashStyle.DashDotDot;
                    pen.StartCap = LineCap.Triangle;
                    pen.EndCap = LineCap.ArrowAnchor;

                    g.DrawLine(pen, parent.WorldToClient(aspec.Start), parent.WorldToClient(aspec.End));

                    g.DrawLine(pen, parent.WorldToClient(aspec.Start), parent.WorldToClient(aspec.Center));
                    g.DrawLine(pen, parent.WorldToClient(aspec.End), parent.WorldToClient(aspec.Center));

                    Point locatioStart = parent.WorldToClient(aspec.Start);
                    locatioStart = new Point(locatioStart.X - 20, locatioStart.Y - 20);
                    RectangleF SRect = new RectangleF(locatioStart, new Size(40, 40));
                    Pen PStart = new Pen(Color.AliceBlue, 4);
                    g.DrawEllipse(PStart, SRect);

                }
            }
        }
        bool isDisposed = false;
       public void Dispose()
        {
           isDisposed = true;
        }
		
    }
}