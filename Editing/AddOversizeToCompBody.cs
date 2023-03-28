//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 10.07.2017
// Autor Guenther.Schindler
// 
// Empty template to fill for synchronous script.
//-----------------------------------------------------------------------------------
// GUID newScript_636352916501529681
//
// The script identifies selected components and checks if they already exist in a hash table.
// If not, an entry for the used part is created in the hash table.
// The part is then opened and an oversize of 0.2 mm is added to the package.
// The updated package is then saved and the display is updated. 
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
using System.Collections;

// example line to import dll DLLImport C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\searchedDLL.dll;
//this line includes a custom dll, e.g. your own dll or other microsoft dlls

namespace PCBIScript
{
    public class PScript : IPCBIScript
    {
        public PScript()
        {
            packageListClassifier = new Hashtable();
        }
        public Hashtable packageListClassifier;
        public void Execute(IPCBIWindow parent)
        {
            //your code here
            foreach (ICMPObject cmp in parent.GetCurrentStep().GetSelectedCMPs())
            {
                if (!packageListClassifier.ContainsKey(cmp.UsedPackageName))
                {
                    packageListClassifier.Add(cmp.UsedPackageName, cmp.UsedPackageName);
                    IPackageSpecificsD pack = cmp.GetPackageSpecificsD();
                    ISurfaceSpecificsD ps = pack.GetPackageSurfaceSpecificsD();
                    IPolyClass packPoly = ps.GetIPolyClass();
                    packPoly.AddOversize(IMath.MM2Mils(0.2)); // Oversize can also be negative
                    pack.SetOutline(packPoly);
                }
            }
            parent.UpdateView();
        }
    }
}