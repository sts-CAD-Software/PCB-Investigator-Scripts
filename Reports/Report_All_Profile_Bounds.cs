//ASynchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 24.09.2015
// Autor Guenther
// 
// Empty template to fill for asynchronous script.
//-----------------------------------------------------------------------------------
// GUID C3F100ED-BD22-4322-B634-56CFFD5BF9B6

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
            int pcbCount = 0;
            IStep PanelStep = parent.GetCurrentStep();
            if (PanelStep == null) return;

            RectangleF PanelSize = PanelStep.GetBounds();

            if (PanelSize == RectangleF.Empty) return;
            DrawSingleBoard(g, PanelSize, new PointD(0, 0), pcbCount, PanelStep.Name, "0");
            pcbCount++;
            List<IStep.StepAndRepeatClass> srList = PanelStep.GetChildStepClasses();

            foreach (IStep.StepAndRepeatClass sr in srList)
            {
                IStep step = parent.GetStep(sr.NAME);
                System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
                matrix.Rotate(-(float)sr.ANGLE,System.Drawing.Drawing2D.MatrixOrder.Append);
                matrix.Translate((float)sr.X, (float)sr.Y,System.Drawing.Drawing2D.MatrixOrder.Append);
                RectangleF BoardSize = IMath.RectangleTransformWithMatrix(step.GetBoundsD(), matrix).ToRectangleF();
                PointD originOfBoard = new PointD(sr.X,sr.Y);
                IMath.TransformPoint(matrix, originOfBoard);
                DrawSingleBoard(g, BoardSize, originOfBoard, pcbCount, sr.NAME, sr.ANGLE.ToString("N2"));
                if (sr.NX > 1 || sr.NY > 1)
                {
                    for (int iy = 0; iy < sr.NY; iy++)
                        for (int ix = 0; ix < sr.NX; ix++)
                        {
                            System.Drawing.Drawing2D.Matrix matrixsr = matrix.Clone();
                            double offsetX = sr.DX * ix;
                            double offsetY = sr.DY * iy;
                            matrixsr.Translate((float)offsetX, (float)offsetY, System.Drawing.Drawing2D.MatrixOrder.Append);
                            BoardSize = IMath.RectangleTransformWithMatrix(step.GetBoundsD(), matrixsr).ToRectangleF();
                            originOfBoard = new PointD(sr.X + offsetX, sr.Y + offsetY);
                            DrawSingleBoard(g, BoardSize, originOfBoard, pcbCount, sr.NAME, sr.ANGLE.ToString("N2"));
                            pcbCount++;
                        }
                }
                pcbCount++;
            }
        }
        private void DrawSingleBoard(Graphics g, RectangleF BoardSize, PointD originOfBoard, int pcbCount, string BoardName, string Rotation)
        {
            Point locationBoard = parent.WorldToClient(BoardSize.Location);
            Point rightTopBoard = parent.WorldToClient(new PointF(BoardSize.Right, BoardSize.Bottom));
            
            RectangleF alphaRect = NormalizedRectangleDMinMax(locationBoard.X, locationBoard.Y, rightTopBoard.X, rightTopBoard.Y);
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 100, 100, 100)), alphaRect);


            #region draw down line
            g.DrawLine(Pens.Blue, new Point(locationBoard.X, locationBoard.Y), new Point(rightTopBoard.X, locationBoard.Y));
            #endregion
            #region draw top line
            g.DrawLine(Pens.Green, new Point(locationBoard.X, rightTopBoard.Y), new Point(rightTopBoard.X, rightTopBoard.Y));
            g.DrawLine(Pens.Green, new Point(locationBoard.X, locationBoard.Y), new Point(locationBoard.X, rightTopBoard.Y));
            #endregion
            #region draw right line
            g.DrawLine(Pens.Green, new Point(rightTopBoard.X, locationBoard.Y), new Point(rightTopBoard.X, rightTopBoard.Y));
            #endregion
            Point originBoard = parent.WorldToClient(originOfBoard);
            g.FillEllipse(new SolidBrush(Color.Yellow), new RectangleF((float)originBoard.X - 3, (float)originBoard.Y - 3, 6, 6));
            g.DrawString("PCB_" + pcbCount.ToString() + ": " + BoardName, new Font("Arial", 10), Brushes.AliceBlue, new Point((int)((originBoard.X)), originBoard.Y + 4));
            //do translate for Strings

            string BoaurdSizeWidth = BoardSize.Width.ToString("N3");
            string BoardSizeHeight = BoardSize.Height.ToString("N3");
            string BoaurdSizeLU = BoardSize.X.ToString("N3") + " / " + BoardSize.Y.ToString("N3");
            string BoardSizeRO = BoardSize.Right.ToString("N3") + " / " + BoardSize.Bottom.ToString("N3");
            string OriginBoardPoint = originOfBoard.X.ToString("N3") + " / " + originOfBoard.Y.ToString("N3");

            if (parent.GetUnit())
            {
                float unit = 25.4f / 1000;
                BoaurdSizeWidth = (BoardSize.Width * unit).ToString("N3");
                BoardSizeHeight = (BoardSize.Height * unit).ToString("N3");
                BoaurdSizeLU = (BoardSize.X * unit).ToString("N3") + " / " + (BoardSize.Y * unit).ToString("N3");
                BoardSizeRO = (BoardSize.Right * unit).ToString("N3") + " / " + (BoardSize.Bottom * unit).ToString("N3");
                OriginBoardPoint = (originOfBoard.X * unit).ToString("N3") + " / " + (originOfBoard.Y * unit).ToString("N3");
               // g.TranslateTransform(1, -1);
                g.DrawString(BoaurdSizeWidth + " mm", new Font("Arial", 10), Brushes.YellowGreen, new Point((int)((locationBoard.X + rightTopBoard.X) / 2.1), locationBoard.Y - 20));
                g.DrawString(BoardSizeHeight + " mm", new Font("Arial", 10), Brushes.YellowGreen, new Point(rightTopBoard.X - 70, (int)((rightTopBoard.Y + locationBoard.Y) / 2) - 5));

                g.DrawString(BoaurdSizeLU + " mm", new Font("Arial", 10), Brushes.AliceBlue, new Point((int)((locationBoard.X)), locationBoard.Y - 20));
                g.DrawString(BoardSizeRO + " mm", new Font("Arial", 10), Brushes.AliceBlue, new Point(rightTopBoard.X - 120, (int)((rightTopBoard.Y)) + 10));

                g.DrawString(OriginBoardPoint + " mm Angle:" + Rotation, new Font("Arial", 10), Brushes.AliceBlue, new Point(originBoard.X, (int)((originBoard.Y)) + 20));
            }
            else
            {
              //  g.TranslateTransform(1, -1);
                g.DrawString(BoaurdSizeWidth + " mils", new Font("Arial", 10), Brushes.YellowGreen, new Point((int)((locationBoard.X + rightTopBoard.X) / 2.1), locationBoard.Y - 20));
                g.DrawString(BoardSizeHeight + " mils", new Font("Arial", 10), Brushes.YellowGreen, new Point(rightTopBoard.X - 70, (int)((rightTopBoard.Y + locationBoard.Y) / 2) - 10));


                g.DrawString(BoaurdSizeLU + " mils", new Font("Arial", 10), Brushes.AliceBlue, new Point((int)((locationBoard.X)), locationBoard.Y - 20));
                g.DrawString(BoardSizeRO + " mils", new Font("Arial", 10), Brushes.AliceBlue, new Point(rightTopBoard.X - 120, (int)((rightTopBoard.Y)) + 10));

                g.DrawString(OriginBoardPoint + " mils Angle:" + Rotation, new Font("Arial", 10), Brushes.AliceBlue, new Point(originBoard.X, (int)((originBoard.Y)) + 20));
            }
        }
                private RectangleF NormalizedRectangleDMinMax(float x, float y, float x2, float y2)
        {
            RectangleF rect = RectangleF.FromLTRB(Math.Min(x, x2),
                Math.Min(y, y2),
                Math.Max(x, x2),
                Math.Max(y, y2));
            return rect;
        }
        bool isDisposed = false;
       public void Dispose()
        {
           isDisposed = true;
        }
		
    }
}