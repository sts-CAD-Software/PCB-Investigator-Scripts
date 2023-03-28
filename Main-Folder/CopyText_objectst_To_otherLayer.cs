//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 30.03.2017
// Autor Guenther.Schindler
// 
// Empty template to fill for synchronous script.
//-----------------------------------------------------------------------------------
// GUID newScript_636264684078812126
//
// This script imports several modules and a custom DLL, and then creates a new layer that contains all the IODBObjecte from the active layers of the current step layer that contain the "_string" attribute key.
// These objects are removed from their original layers and placed in the new layer. 
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
        }

        public void Execute(IPCBIWindow parent)
        {

            IFilter filter = new IFilter(parent);
           
            //your code here
            foreach (ILayer layer in parent.GetCurrentStep().GetActiveLayerList())
            {
             List<IODBObject> objectsToMove = new List<IODBObject>();
                foreach (IODBObject obj in layer.GetAllLayerObjects())
                {
                    foreach (DictionaryEntry entry in obj.GetAttributes())
                    {

                        if (entry.Key.ToString() == "_string")
                        {
                            objectsToMove.Add((IODBObject)obj);
                        }
                    }
                }


                IODBLayer newLayer = filter.CreateEmptyODBLayer(layer.GetLayerName() + "_1", parent.GetCurrentStep().Name);
                filter.InsertElementsToLayer(0, newLayer, objectsToMove, new PointD(0, 0), false, false, 0);

                foreach (IODBObject mo in objectsToMove)
                {
                    ((IODBLayer)layer).RemoveObject(mo);

                }
            }

            parent.UpdateControlsAndResetView();
        }

    }
}



