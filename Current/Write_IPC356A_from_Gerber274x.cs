//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 01.02.2015
// Autor Guenther
// 
// Write IPC356 A from Gerber274X
//-----------------------------------------------------------------------------------
// This script is used to convert Gerber 274X files to IPC356A format.
// It does so by using the PCB Investigator software to read the Gerber files and extract the necessary information.
// The script then creates a new file in IPC356A format with the extracted data.
// The IPC356A format is commonly used for netlist testing in PCB manufacturing. 
// The script also includes some comments explaining the allowed operation codes for the format. 
//----------------------------------------------------------------------------------

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
using PCBI.Automation.NetCreation;
namespace PCBIScript
{
   public class PScript : IPCBIScript
	{
		public PScript()
		{
		}

		public void Execute(IPCBIWindow parent)
		{
 			//your code here
                   if (parent == null) return;
            System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = "";
            IStep step = parent.GetCurrentStep();
            IFilter filter = new IFilter(parent);
            StringBuilder sb = new StringBuilder();
            IMatrix m = parent.GetMatrix();
            List<int> netnumbers = step.GetAllNetNrs();

            Dictionary<int, List<PCBIObject>> allNetNrsWithItems = PCBI.Automation.NetCreation.NetCreator.CreateGlobalNet(step, m, true);
            foreach (int netlist in allNetNrsWithItems.Keys)
            {
                foreach (PCBIObject oObject in allNetNrsWithItems[netlist])
                {
                    oObject.iObj.PcbNetNumber = oObject.NetNrGlobal;
                }
            }
            List<string> names = step.GetAllLayerNames();
            foreach (string layername in names)
            {
                if (step.GetLayer(layername).GetType() == typeof(IODBLayer))
                {
                    IODBLayer layer = (IODBLayer)step.GetLayer(layername);
                    if (m.GetMatrixLayerType(layername) == MatrixLayerType.Signal || m.GetMatrixLayerType(layername) == MatrixLayerType.Rout || m.GetMatrixLayerType(layername) == MatrixLayerType.Drill || m.GetMatrixLayerType(layername) == MatrixLayerType.Mask)
                    {
                        foreach (IODBObject obj in layer.GetAllLayerObjects())
                        {
                            int PCB_NetNum = obj.PcbNetNumber;
                            int LayerNetNum = obj.LayerNetNumber;
                            obj.FreeText = PCB_NetNum.ToString();
                            //sb.Append("P: " + PCB_NetNum.ToString() + "L: " + LayerNetNum.ToString() + Environment.NewLine);

                        }
                    }
                }
            }
            sb.Append("P VER IPC D 356" + Environment.NewLine);
            sb.Append("P IMAGE PRIMARY" + Environment.NewLine);
            sb.Append("C Created with PCB-Investigator" + Environment.NewLine);
            sb.Append("P Creation date:" + DateTime.Now.ToShortDateString() + Environment.NewLine);
            sb.Append("C " + step.Name + "  " + parent.GetJobName() + Environment.NewLine);
            sb.Append("C" + Environment.NewLine);
            sb.Append("P CUST 0" + Environment.NewLine);
            #region descriptions
            //            Units of measurement :
            //SI Metric
            //CUST 0 or CUST Inches and degrees
            //CUST 1 Millimeters and degrees
            //CUST 2 Inches and radians

            //ALLOWED OPERATION CODES (columns 1-3)
            //317 Through hole. Alternatively it can represent a feature and through hole at a point.
            //017 Continuation record that defines a through hole associated with the previous record
            //367 Non-plated tooling hole
            //327 Surface mount feature
            //027 Continuation record that defines a through hole associated with the previous record
            //099 Test point location of the feature described in the previous record
            //088 Solder mask clearance of the feature described in the previous record
            //307 Blind or buried via
            //309 Image 2 through NNNN offset data
            //370 In-board resistor, capacitor or inductor
            //070 Continuation of In-board resistor, capacitor or inductor
            //378 Conductor segment data
            //078 Continuation record of conductor segment data
            //379 Adjacency data record
            //079 Continuation of adjacency data record
            //380 On-board resistor, capacitor or inductor
            //080 Continuation of on-board resistor, capacitor or inductor
            //389 Board, panel or sub-panel , scoring or other fabrication outline data
            //089 Continuation of outline data
            //390 Non-test feature such as fiducials , targets , test status marking location , etc …
            //090 Reference for high-voltage isolation , impedance and other specified tests
            //999 End of Job data file 
            #endregion
            List<string> drillNames = m.GetAllDrillLayerNames();
            foreach (string drilName in drillNames)
            {
                IODBLayer layer = (IODBLayer)step.GetLayer(drilName);
                string LayerNumber = m.GetRawIndexByName(layer.LayerName).ToString();
                foreach (IODBObject via in layer.GetAllLayerObjects())
                {
                    string NetName = via.PcbNetNumber.ToString(); ;
                    IObjectSpecifics viaSpec = via.GetSpecifics();
                    if (viaSpec.GetType() == typeof(IPadSpecifics))
                    {
                        float x = ((IPadSpecifics)viaSpec).Location.X;
                        float y = ((IPadSpecifics)viaSpec).Location.Y;
                        double diameter = ((IPadSpecifics)viaSpec).Diameter;
                        string currentLine = new string(' ', 255);
                        currentLine = currentLine.Insert(0, "317");
                        currentLine = currentLine.Insert(4, NetName);
                        currentLine = currentLine.Insert(21, "VIA");
                        currentLine = currentLine.Insert(27, "-");
                        //currentLine = currentLine.Insert(28, LayerNumber);
                        currentLine = currentLine.Insert(33, "D");
                        currentLine = currentLine.Insert(34, diameter.ToString("N2", nfi));
                        currentLine = currentLine.Insert(38, "P   ");
                        currentLine = currentLine.Insert(42, "X");
                        if(Math.Sign(x) == 1)
                             currentLine = currentLine.Insert(43,"+");
                        else
                             currentLine = currentLine.Insert(43,"-");
                        currentLine = currentLine.Insert(44, x.ToString("N2",nfi));
                        currentLine = currentLine.Insert(50, "Y");
                        if (Math.Sign(y) == 1)
                            currentLine = currentLine.Insert(51, "+");
                        else
                            currentLine = currentLine.Insert(51, "-");
                        currentLine = currentLine.Insert(52, y.ToString("N2", nfi));

                        currentLine = currentLine.Insert(58, "X");
                        currentLine = currentLine.Insert(59, diameter.ToString("N2", nfi));

                        currentLine = currentLine.Insert(63, "Y");
                        currentLine = currentLine.Insert(64, diameter.ToString("N2", nfi));
                        currentLine = currentLine.Insert(73, "S");
                        currentLine = currentLine.Insert(74, "3");
                        sb.Append(currentLine + Environment.NewLine);
                    }
                }
            }
            IODBLayer LayerPTop = (IODBLayer)step.GetLayer(m.GetTopSignalLayer());
            if (LayerPTop != null)
            {
                string LayerNumber = m.GetRawIndexByName(LayerPTop.LayerName).ToString();
                foreach (IODBObject padTop in LayerPTop.GetAllLayerObjects())
                {
                    string NetName = padTop.PcbNetNumber.ToString(); ;
                    IObjectSpecifics os = padTop.GetSpecifics();
                    if (os.GetType() == typeof(IPadSpecifics))
                    {
                        IPadSpecifics oPad = (IPadSpecifics)os;
                        PointF PadMidPoint = new PointF(padTop.GetBounds().X + padTop.GetBounds().Width / 2, padTop.GetBounds().Y + padTop.GetBounds().Height / 2);
                        string currentLine = new string(' ', 255);
                        currentLine = currentLine.Insert(0, "327");
                        currentLine = currentLine.Insert(4, NetName);
                        currentLine = currentLine.Insert(21, "PAD");
                        currentLine = currentLine.Insert(27, " ");
                        currentLine = currentLine.Insert(28, LayerNumber);
                        currentLine = currentLine.Insert(33, "D");
                        currentLine = currentLine.Insert(34, padTop.GetBounds().Width.ToString("N2", nfi));
                        currentLine = currentLine.Insert(38, "P   ");
                        currentLine = currentLine.Insert(42, "X");
                        if (Math.Sign(PadMidPoint.X) == 1)
                            currentLine = currentLine.Insert(43, "+");
                        else
                            currentLine = currentLine.Insert(43, "-");
                        currentLine = currentLine.Insert(44, PadMidPoint.X.ToString("N2", nfi));
                        currentLine = currentLine.Insert(50, "Y");
                        if (Math.Sign(PadMidPoint.Y) == 1)
                            currentLine = currentLine.Insert(51, "+");
                        else
                            currentLine = currentLine.Insert(51, "-");
                        currentLine = currentLine.Insert(52, PadMidPoint.Y.ToString("N2", nfi));

                        currentLine = currentLine.Insert(58, "W");
                        currentLine = currentLine.Insert(59, padTop.GetBounds().Width.ToString("N2", nfi));

                        currentLine = currentLine.Insert(63, "H");
                        currentLine = currentLine.Insert(64, padTop.GetBounds().Height.ToString("N2", nfi));
                        currentLine = currentLine.Insert(73, "S");
                        currentLine = currentLine.Insert(74, "1");
                        sb.Append(currentLine + Environment.NewLine);
                    }
                }
            }
            IODBLayer LayerPBot = (IODBLayer)step.GetLayer(m.GetBotSignalLayer());
            if (LayerPBot != null)
            {
                string LayerNumber = m.GetRawIndexByName(LayerPBot.LayerName).ToString();
                foreach (IODBObject padBot in LayerPBot.GetAllLayerObjects())
                {
                    string NetName = padBot.PcbNetNumber.ToString(); ;
                    IObjectSpecifics os = padBot.GetSpecifics();
                    if (os.GetType() == typeof(IPadSpecifics))
                    {
                        IPadSpecifics oPad = (IPadSpecifics)os;
                        PointF PadMidPoint = new PointF(padBot.GetBounds().X + padBot.GetBounds().Width / 2, padBot.GetBounds().Y + padBot.GetBounds().Height / 2);
                        string currentLine = new string(' ', 255);
                        currentLine = currentLine.Insert(0, "327");
                        currentLine = currentLine.Insert(4, NetName);
                        currentLine = currentLine.Insert(21, "PAD");
                        currentLine = currentLine.Insert(27, " ");
                        currentLine = currentLine.Insert(28, LayerNumber);
                        currentLine = currentLine.Insert(33, "D");
                        currentLine = currentLine.Insert(34, padBot.GetBounds().Width.ToString("N2", nfi));
                        currentLine = currentLine.Insert(38, "P   ");
                        currentLine = currentLine.Insert(42, "X");
                        if (Math.Sign(PadMidPoint.X) == 1)
                            currentLine = currentLine.Insert(43, "+");
                        else
                            currentLine = currentLine.Insert(43, "-");
                        currentLine = currentLine.Insert(44, PadMidPoint.X.ToString("N2", nfi));
                        currentLine = currentLine.Insert(50, "Y");
                        if (Math.Sign(PadMidPoint.Y) == 1)
                            currentLine = currentLine.Insert(51, "+");
                        else
                            currentLine = currentLine.Insert(51, "-");
                        currentLine = currentLine.Insert(52, PadMidPoint.Y.ToString("N2", nfi));

                        currentLine = currentLine.Insert(58, "W");
                        currentLine = currentLine.Insert(59, padBot.GetBounds().Width.ToString("N2", nfi));

                        currentLine = currentLine.Insert(63, "H");
                        currentLine = currentLine.Insert(64, padBot.GetBounds().Height.ToString("N2", nfi));
                        currentLine = currentLine.Insert(73, "S");
                        currentLine = currentLine.Insert(74, "1");
                        sb.Append(currentLine + Environment.NewLine);
                    }
                }
            }
            sb.Append("999" + Environment.NewLine); // End of File

            string text = sb.ToString();
            Directory.CreateDirectory(parent.GetODBJobDirectory() + "\\user\\");
            FileStream fs = new FileStream(parent.GetODBJobDirectory() + "\\user\\IPC356.txt", FileMode.OpenOrCreate);
            StreamWriter sr = new StreamWriter(fs, Encoding.UTF8);
            fs.SetLength(0);
            sr.WriteLine(text);
            sr.Close();
            fs.Close();
            parent.UpdateView(); 		
	}
		
    }
}
