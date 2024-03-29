//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 25.09.2015
// Autor Guenther Schindler
// 
// Show Board size of current job.
//-----------------------------------------------------------------------------------
// The script is used to visualize information about a loaded board in PCB production. 
// The script is called when a board is loaded and then runs in an infinite loop with a 1-second pause. 
// The script then draws lines and text to indicate the size of the board, which is displayed in the graphical user interface. 
// The drawn lines and texts are formatted with different colors and fonts depending on the units of measurement used for the board (millimeters or mils). 
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

            RectangleF BoardSize = parent.GetJobBounds();

            if (BoardSize == RectangleF.Empty) return;

            Point locationBoard = parent.WorldToClient(BoardSize.Location);
            //Be careful with top and bottom because the display is vertical mirrored!
            Point rightTopBoard = parent.WorldToClient(new PointF(BoardSize.Right, BoardSize.Bottom));
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

            string BoaurdSizeWidth = BoardSize.Width.ToString("N3");
            string BoardSizeHeight = BoardSize.Height.ToString("N3");
            string BoaurdSizeLU = BoardSize.X.ToString("N3") + " / " + BoardSize.Y.ToString("N3");
            string BoardSizeRO = BoardSize.Right.ToString("N3") + " / " + BoardSize.Bottom.ToString("N3");

            if (parent.GetUnit())
            {
                float unit = 25.4f / 1000;
                BoaurdSizeWidth = (BoardSize.Width * unit).ToString("N3");
                BoardSizeHeight = (BoardSize.Height * unit).ToString("N3");
                BoaurdSizeLU = (BoardSize.X * unit).ToString("N3") + " / " + (BoardSize.Y * unit).ToString("N3");
                BoardSizeRO = (BoardSize.Right * unit).ToString("N3") + " / " + (BoardSize.Bottom * unit).ToString("N3");
                g.TranslateTransform(1, -1);
                g.DrawString(BoaurdSizeWidth + " mm", new Font("Arial", 10), Brushes.YellowGreen, new Point((int)((locationBoard.X + rightTopBoard.X) / 2.1), locationBoard.Y + 20));
                g.DrawString(BoardSizeHeight + " mm", new Font("Arial", 10), Brushes.YellowGreen, new Point(rightTopBoard.X + 20, (int)((rightTopBoard.Y + locationBoard.Y) / 2) - 5));

                g.DrawString(BoaurdSizeLU + " mm", new Font("Arial", 10), Brushes.AliceBlue, new Point((int)((locationBoard.X)), locationBoard.Y + 40));
                g.DrawString(BoardSizeRO + " mm", new Font("Arial", 10), Brushes.AliceBlue, new Point(rightTopBoard.X + 20, (int)((rightTopBoard.Y)) - 5));

            }
            else
            {
                g.TranslateTransform(1, -1);
                g.DrawString(BoaurdSizeWidth + " mils", new Font("Arial", 10), Brushes.YellowGreen, new Point((int)((locationBoard.X + rightTopBoard.X) / 2.1), locationBoard.Y + 20));
                g.DrawString(BoardSizeHeight + " mils", new Font("Arial", 10), Brushes.YellowGreen, new Point(rightTopBoard.X + 20, (int)((rightTopBoard.Y + locationBoard.Y) / 2) - 5));


                g.DrawString(BoaurdSizeLU + " mils", new Font("Arial", 10), Brushes.AliceBlue, new Point((int)((locationBoard.X)), locationBoard.Y + 40));
                g.DrawString(BoardSizeRO + " mils", new Font("Arial", 10), Brushes.AliceBlue, new Point(rightTopBoard.X + 20, (int)((rightTopBoard.Y)) - 5));
            }
        }
		bool isDisposed = false;
       public void Dispose()
        {
           isDisposed = true;
        }
		
    }
}