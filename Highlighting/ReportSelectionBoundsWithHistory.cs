//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 24.09.2015
// Autor Guenther
// 
// Empty template to fill for asynchronous script.
//-----------------------------------------------------------------------------------
// GUID=ReportSelection_Bounds_History637208351977593189

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
		List<RectangleF> listToShow =new List<RectangleF>();
		public void Execute(IPCBIWindow Parent)
		{
			//fill code here code
            parent = Parent;
            Parent.PCBIFormGraphicPaneDrawing += new IPCBIWindow.DrawingEventHandler(Parent_PCBIFormGraphicPaneDrawing);
            Parent.PCBISelectionChanged+= new EventHandler(Parent_PCBISelectionChanged);
           while (true)
            {
                Thread.Sleep(1000);

                if (isDisposed)
				{
			       Parent.PCBIFormGraphicPaneDrawing -= new IPCBIWindow.DrawingEventHandler(Parent_PCBIFormGraphicPaneDrawing);		
                   Parent.PCBISelectionChanged-= new EventHandler(Parent_PCBISelectionChanged);			       
                   break;
				}
			}
		}
	void Parent_PCBISelectionChanged(object sender, EventArgs e)
        {
  if (!parent.JobIsLoaded) return;
              if(parent.GetCurrentStep().GetSelectedElements().Count < 1)
              {
                listToShow.Clear();
                return;
              }

            RectangleF ObjSize = RectangleF.Empty;

            foreach(IObject obj in parent.GetCurrentStep().GetSelectedElements())
            {
          
            
                if(ObjSize == RectangleF.Empty)
                {
                    ObjSize = obj.GetBounds();
                }
                else 
                {
                    ObjSize = RectangleF.Union(ObjSize, obj.GetBounds());
                }
            }
			
            foreach (ICMPObject obj in parent.GetCurrentStep().GetSelectedCMPs())
            {
               PCBI.Automation.DrawingParameters.IComponentParameters dp = new PCBI.Automation.DrawingParameters.IComponentParameters();
               dp.ShowPins = false;
                if (ObjSize == RectangleF.Empty)
                {
                     if(dp.ShowPins)
                    {
                       ObjSize = obj.GetBounds();
                    } else
                    {
                      ObjSize = new RectangleF((float) obj.GetBodyBoundsD().X,(float)  obj.GetBodyBoundsD().Y,(float) obj.GetBodyBoundsD().Width,(float) obj.GetBodyBoundsD().Height);
                    }
                }
                else
                {
                    ObjSize = RectangleF.Union(ObjSize, obj.GetBounds());
                }
            }
			listToShow.Add(ObjSize);
        }
        
        void Parent_PCBIFormGraphicPaneDrawing(Graphics g, int ClientWidth, int ClientHeight)
        {
          
			foreach(RectangleF BoardSize in listToShow)
			{
            if (BoardSize == RectangleF.Empty) return;

            Point locationBoard = parent.WorldToClient(BoardSize.Location);
            //Be careful with top and bottom because the display is vertical mirrored!
            Point rightTopBoard = parent.WorldToClient(new PointF(BoardSize.Right, BoardSize.Bottom));
            Point middleTopBoard = parent.WorldToClient(new PointF(BoardSize.Left, BoardSize.Bottom));

            #region draw down line
            g.DrawLine(Pens.Green, new Point(locationBoard.X, locationBoard.Y + 15), new Point(rightTopBoard.X, locationBoard.Y + 15));
            g.DrawLine(Pens.Green, new Point(locationBoard.X, locationBoard.Y + 10), new Point(locationBoard.X, locationBoard.Y + 20));
            g.DrawLine(Pens.Green, new Point(rightTopBoard.X, locationBoard.Y + 10), new Point(rightTopBoard.X, locationBoard.Y + 20));
            #endregion
            #region draw right line
            g.DrawLine(Pens.Green, new Point(rightTopBoard.X + 15, locationBoard.Y), new Point(rightTopBoard.X + 15, rightTopBoard.Y));
            g.DrawLine(Pens.Green, new Point(rightTopBoard.X + 10, locationBoard.Y), new Point(rightTopBoard.X + 20, locationBoard.Y));
            g.DrawLine(Pens.Green, new Point(rightTopBoard.X + 10, rightTopBoard.Y), new Point(rightTopBoard.X + 20, rightTopBoard.Y));
            #endregion
            //do translate for Strings

            string BoardSizeWidth = BoardSize.Width.ToString("N3");
            string BoardSizeHeight = BoardSize.Height.ToString("N3");
            string BoardSizeLU = BoardSize.X.ToString("N3") + " / " + BoardSize.Y.ToString("N3");
            string BoardSizeRO = BoardSize.Right.ToString("N3") + " / " + BoardSize.Bottom .ToString("N3");
            PointF center =new PointF( BoardSize.X + BoardSize.Width/2,BoardSize.Y + BoardSize.Height/2);
            Point centerBoard = parent.WorldToClient(center);
            string BoardCenter = center.X.ToString("N3") + " / " + center.Y .ToString("N3");
            if(parent.GetUnit())
            {
                float unit =  25.4f / 1000;
                BoardSizeWidth = (BoardSize.Width * unit).ToString("N3");
                BoardSizeHeight = (BoardSize.Height * unit).ToString("N3");
                BoardSizeLU = (BoardSize.X * unit).ToString("N3") + " / " + (BoardSize.Y * unit).ToString("N3");
                BoardSizeRO = (BoardSize.Right * unit).ToString("N3") + " / " + (BoardSize.Bottom * unit).ToString("N3");
                BoardCenter = (center.X * unit).ToString("N3") + " / " + (center.Y * unit).ToString("N3");

                g.TranslateTransform(1, -1);
                g.DrawString(BoardSizeWidth + " mm", new Font("Arial", 10), Brushes.Green, new Point((int)((locationBoard.X + rightTopBoard.X) / 2.1), locationBoard.Y + 20));
                g.DrawString(BoardSizeHeight + " mm", new Font("Arial", 10), Brushes.Green, new Point(rightTopBoard.X + 20, (int)((rightTopBoard.Y + locationBoard.Y) / 2) - 5));

                g.DrawString(BoardSizeLU + " mm", new Font("Arial", 10), Brushes.Aqua, new Point((int)((locationBoard.X)), locationBoard.Y + 40));
                g.DrawString(BoardSizeRO + " mm", new Font("Arial", 10), Brushes.Aqua, new Point(rightTopBoard.X + 20, (int)((rightTopBoard.Y)) - 5));
                g.DrawString("C:" + BoardCenter + " mm", new Font("Arial", 10), Brushes.Yellow, new Point(middleTopBoard.X, (int)((middleTopBoard.Y)) - 15));

            }
            else
            {
                g.TranslateTransform(1, -1);
                g.DrawString(BoardSizeWidth + " mils", new Font("Arial", 10), Brushes.Aqua, new Point((int)((locationBoard.X + rightTopBoard.X) / 2.1), locationBoard.Y + 20));
                g.DrawString(BoardSizeHeight + " mils", new Font("Arial", 10), Brushes.Aqua, new Point(rightTopBoard.X + 20, (int)((rightTopBoard.Y + locationBoard.Y) / 2) - 5));


                g.DrawString(BoardSizeLU + " mils", new Font("Arial", 10), Brushes.Aqua, new Point((int)((locationBoard.X)), locationBoard.Y + 40));
                g.DrawString(BoardSizeRO + " mils", new Font("Arial", 10), Brushes.Aqua, new Point(rightTopBoard.X + 20, (int)((rightTopBoard.Y)) - 5));
                g.DrawString("C:" + BoardCenter + " mils", new Font("Arial", 10), Brushes.Yellow, new Point(middleTopBoard.X, (int)((middleTopBoard.Y)) - 15));

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