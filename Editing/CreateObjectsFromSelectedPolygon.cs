//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-10-01
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Create lines and arcs out of selectet polygons.
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
       /// <summary>
       /// Helper to create Symbols only once.
       /// </summary>
        private static int CheckShapeIndexRound(IFilter pcbiFilter, IODBLayer parentLayer, float thickness)
        {
            int shapeIndex = -1;
            Dictionary<int, PCBI.Automation.IFilter.ToolDefinition> symbolList = pcbiFilter.GetUsedSymbolList(parentLayer);

            foreach (KeyValuePair<int, PCBI.Automation.IFilter.ToolDefinition> symbs in symbolList)
            {
                if (symbs.Value.Diameter == thickness
                    && symbs.Value.Type == PCBI.Symbol_Type.r)
                    return symbs.Key;
            }

            shapeIndex = IFilter.AddToolDefinitionRound(parentLayer, thickness);

            return shapeIndex;
        }

		public void Execute(IPCBIWindow parent)
		{
                IStep step = parent.GetCurrentStep();
                if (step == null) return;

                IFilter PCBI_filter = new IFilter(parent);
                IODBLayer newOutlineLayer = PCBI_filter.CreateEmptyODBLayer("poly_to_line", step.Name);

                float LineWith = 10; //diameter symbol

                foreach (IODBObject obj in step.GetSelectedElements())
                {
                    foreach (IObjectSpecifics os in obj.GetOutline())
                    {
                        if (os.GetType() == typeof(ILineSpecifics))
                        {
                            ILineSpecifics lineEdges = (ILineSpecifics)os;
                            IODBObject line = PCBI_filter.CreateLine(newOutlineLayer);
                            lineEdges.ShapeIndex = CheckShapeIndexRound(PCBI_filter, newOutlineLayer, LineWith);
                            lineEdges.Type = PCBI.Symbol_Type.r;
                            line.SetSpecifics(lineEdges, lineEdges.ShapeIndex);

                        }
                        else if (os.GetType() == typeof(IArcSpecifics))
                        {
                            IArcSpecifics arcEdges = (IArcSpecifics)os;
                            IODBObject arc = PCBI_filter.CreateArc(newOutlineLayer);
                            arcEdges.ShapeIndex = CheckShapeIndexRound(PCBI_filter, newOutlineLayer, LineWith);
                            arcEdges.Type = PCBI.Symbol_Type.r;
                            arc.SetSpecifics(arcEdges, arcEdges.ShapeIndex);
                        }
                    }
                }           
            parent.UpdateView();
            IMatrix matrix = parent.GetMatrix();
            matrix.UpdateDataAndList();
		}
		
    }
}