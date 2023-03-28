//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2015-09-28
// Author EasyLogix
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 2017-06-13
// Description: Show details of job in graphic pane.
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID Customer_Info_889B2D46-E68D-439D-91D2-A2DCCE2C7385
// ButtonEnabled=1   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)
//-----------------------------------------------------------------------------------
//This script is designed to show the details of the current job in the graphic pane.
//It uses various methods to draw text and shapes on the pane to display information such as the current date, the number of selected components and features, and the references of the selected components. 
//The script listens for the PCBIFormGraphicPaneDrawing event of the IPCBIScriptASync interface and calls various methods to draw on the pane.
//The DrawContact, DrawDate, DrawSelectedCompsCount, DrawSelectedFeatureCount, DrawSelectedRefs, and DrawSelectedNetList methods use the Graphics object passed to them to draw on the pane. 
//The Execute method sets up the event listener and waits until the script is disposed before removing the listener.
//The isDisposed field is not shown in the code but is likely set to true when the script is disposed to break the loop in the Execute method. 
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
			if (parent==null || !parent.JobIsLoaded)
			{
			MessageBox.Show("No job loaded.");
			return;
			}
			
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