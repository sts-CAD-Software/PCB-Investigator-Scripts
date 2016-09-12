//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 13.10.2015
// Autor Guenther
// 
// Empty template to fill for synchronous script.
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
using PCBI.MathUtils;

namespace PCBIScript
{
   public class PScript : IPCBIScript
	{
		public PScript()
		{
		}
        public IPCBIWindow Parent;
		public void Execute(IPCBIWindow parent)
		{
 			//your code here
            Parent = parent;
            Report_Via_Pad();
            parent.UpdateView(); 
		}

       public void Report_Via_Pad()
        {
            dynamic wordApp = Activator.CreateInstance(Type.GetTypeFromProgID("Word.Application"));
            wordApp.Visible = true;

            dynamic wordDoc = wordApp.Documents.Add();
           //////

            StringBuilder sb = new StringBuilder();

            IStep step = Parent.GetCurrentStep();
            IMatrix m = Parent.GetMatrix();

            foreach (INet NetList in step.GetNets())
            {
                
                foreach (IODBObject obj in NetList.GetAllNetObjects(Parent))
                {
                    if (m.GetMatrixLayerType(obj.GetParentLayerName()) == MatrixLayerType.Drill)
                    {
                        if(obj.Type == IObjectType.Pad)
                        {
                            IPadSpecifics ps = (IPadSpecifics)obj.GetSpecifics();
                            float x = ps.Location.X;
                            float Y = ps.Location.Y;
                            sb.Append("Via;" + obj.NetName + ";" + x.ToString("N3") +";" + Y.ToString("N3") + Environment.NewLine);        
                        }
                    }
                }

            }
           //////
            string LVText = "Type\tNetName\tposx\tposy" + Environment.NewLine + sb.ToString();

            dynamic rng = wordApp.ActiveDocument.Range(0, 0);
            rng.Text = LVText;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
        }
		
    }
}