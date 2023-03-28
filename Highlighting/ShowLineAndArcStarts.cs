//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 25.09.2015
// Autor Fabio Gruber
// 
// Show start of lines and arcs.
//-----------------------------------------------------------------------------------
// This script is used to improve the visibility of lines and arcs to see them more clearly.
// The script draws a triangle shape at each starting point of lines and arcs to mark the starting point.
// It is used in PCB layout software to improve the visibility and accuracy of lines and arcs on the board. 
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

            IStep step = parent.GetCurrentStep();
            if (step == null) return;

            foreach (IODBObject selectedObject in step.GetSelectedElements())
            {
                if (selectedObject.Type == IObjectType.Line)
                {
                    ILineSpecifics line = (ILineSpecifics)selectedObject.GetSpecifics();
                    Point startOfLine = parent.WorldToClient(line.Start);
                    float dimaeter = (float)line.Diameter;

                    g.DrawLine(Pens.Bisque, new PointF(startOfLine.X, startOfLine.Y + dimaeter), new PointF(startOfLine.X + dimaeter, startOfLine.Y));
                    g.DrawLine(Pens.Bisque, new PointF(startOfLine.X + dimaeter, startOfLine.Y), new PointF(startOfLine.X, startOfLine.Y - dimaeter));
                    g.DrawLine(Pens.Bisque, new PointF(startOfLine.X + dimaeter, startOfLine.Y), new PointF(startOfLine.X - dimaeter, startOfLine.Y));
                }
                else if (selectedObject.Type == IObjectType.Arc)
                {
                    IArcSpecifics arc = (IArcSpecifics)selectedObject.GetSpecifics();
                    Point startOfArc = parent.WorldToClient(arc.Start);
                    float dimaeter = (float)arc.PenWidth;

                    g.DrawLine(Pens.Bisque, new PointF(startOfArc.X, startOfArc.Y + dimaeter), new PointF(startOfArc.X + dimaeter, startOfArc.Y));
                    g.DrawLine(Pens.Bisque, new PointF(startOfArc.X + dimaeter, startOfArc.Y), new PointF(startOfArc.X, startOfArc.Y - dimaeter));
                    g.DrawLine(Pens.Bisque, new PointF(startOfArc.X + dimaeter, startOfArc.Y), new PointF(startOfArc.X - dimaeter, startOfArc.Y));
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
