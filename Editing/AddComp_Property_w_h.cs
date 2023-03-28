//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 18.08.2016
// Autor Guenther
// 
// Empty template to fill for synchronous script.
//-----------------------------------------------------------------------------------
// GUID newScript_636071173879829987
//
// This can be used to extract properties of components on a PCB and write them to a file.
// The script goes through all components on the current PCB layer and extracts information such as body width, body height, pin width, pin height and pin area.
// The size of the pins is used to calculate the geometry names of the components.
// The script also creates custom attribute values for the extracted information, which can then be saved in a table format.
// The script also has a custom function for adding rectangles. 
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
// _DLLImport C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\Microsoft.VisualBasic.dll;
//this line includes the same reference as last line: using Microsoft.VisualBasic;

namespace PCBIScript
{
   public class PScript : IPCBIScript
	{
		public PScript()
		{
		}

		public void Execute(IPCBIWindow parent)
		{
            IMatrix m = parent.GetMatrix();
            IStep step = parent.GetCurrentStep();


            foreach (ICMPObject comp in step.GetAllCMPObjects())
            {
            
            IPackageSpecificsD pack = comp.GetPackageSpecificsD();
            RectangleD rectBody =  pack.GetBodyBounds();
            RectangleD rect = pack.GetBounds();
                comp.AddComponentAttribute("Body_Width", rectBody.Width.ToString() );
                comp.AddComponentAttribute("Body_Height", rectBody.Height.ToString() );
               // rect = GetPinBounds(comp);
                comp.AddComponentAttribute("Pin_Width", rect.Width.ToString() );
                comp.AddComponentAttribute("Pin_Height", rect.Height.ToString() );
                comp.AddComponentAttribute("Pin_Area", (rect.Height * rect.Width).ToString() );
             //   if(comp.GetPinList().Count == 2)
               // {
             //   string shouldBe = GetGeometrieName(rect.Height * rect.Width);
              //  comp.AddComponentAttribute("Geometry_Calculated", shouldBe );
               // }
            }
			
            parent.UpdateView(); 
		}
		private RectangleF GetPinBounds(ICMPObject comp)
		{
		                        RectangleF b_all = RectangleF.Empty;
		      foreach(IPin pin in comp.GetPinList())
		      {
		              pin.GetBounds(comp);
		                                          RectangleF b1 = pin.GetBounds(comp);
                            b_all = AddRectangleF(b1, b_all);
		      }
		      return b_all;
		}
		private string GetGeometrieName(float area)
		{
		  if(area > 1500 & area < 1800)
		  {
		      return "0402";
		  }
		  else  if(area > 4000 & area < 4200)
		  {
		      return "0603";
		  }
	        else  if(area > 5800 & area < 6900)
		  {
		      return "0805";
		  }  
		 	else  if(area > 10000 & area < 13000)
		  {
		      return "1206";
		  }  
		  
		  return "unknown";
		}
        internal RectangleF AddRectangleF(RectangleF r1, RectangleF r2)
        {
            if (float.IsNaN(r2.X) || float.IsNaN(r2.Y) || float.IsNaN(r2.Width) || float.IsNaN(r2.Height))
                return r1;

            if (r1 == RectangleF.Empty && r2 != RectangleF.Empty)
            {
                return new RectangleF(r2.X, r2.Y, r2.Width, r2.Height);
                //r1 = r2;
            }
            else if (r1 != RectangleF.Empty && r2 != RectangleF.Empty)
            {
                r1 = RectangleF.FromLTRB(Math.Min(r1.Left, r2.Left),
                    Math.Min(r1.Top, r2.Top),
                    Math.Max(r1.Right, r2.Right),
                    Math.Max(r1.Bottom, r2.Bottom));
            }
            return new RectangleF(r1.X, r1.Y, r1.Width, r1.Height);
        }		
    }
}







