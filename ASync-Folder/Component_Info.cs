//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2015-09-28
// Author EasyLogix
// // www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 2017-06-13
// Description: Show all important infos of a selected component directly in graphic pane without extra dialog.
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID Component_Info_F3D0D40A-6889-4662-8F48-041949197D69
// ButtonEnabled=5   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)


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
            DrawComponentInfo(g);
        }
        private void DrawComponentInfo(Graphics g)
        {

            int xPos = 30;
            int posY = 80;
            foreach (ICMPObject cmp in parent.GetCurrentStep().GetSelectedCMPs())
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
                    strSize = g.MeasureString(Text, new Font("Arial", 10));
                    g.FillRectangle(TextB, xPos, posY, strSize.Width, strSize.Height);
                    g.DrawString(Text, new Font("Arial", 10), Brushes.Black, new PointF(xPos, posY));
                    posY += 20;
                }
                foreach (string attrib in cmp.GetGeometryAttributes())
                {
                    Text = attrib;
                    strSize = g.MeasureString(Text, new Font("Arial", 10));
                    g.FillRectangle(TextB, xPos,posY, strSize.Width, strSize.Height);
                    g.DrawString(Text, new Font("Arial", 10), Brushes.Black, new PointF(xPos, posY));
                    posY += 20;
                }
                break;
            }
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