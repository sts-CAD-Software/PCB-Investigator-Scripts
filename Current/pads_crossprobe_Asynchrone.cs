// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// Export component properties to Excel.
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
using System.Reflection;
using System.Threading;


namespace PCBIScript
{
public enum PPcbObjectType
{
	ppcbObjectTypeUnknown = 0,
        ppcbObjectTypeComponent = 1,
        ppcbObjectTypeNet = 2,
        ppcbObjectTypePin = 3,
        ppcbObjectTypeVia = 4,
        ppcbObjectTypeConnection = 5,
        ppcbObjectTypeRouteSegment = 6,
        ppcbObjectTypeJumper = 7,
        ppcbObjectTypePartType = 8,
        ppcbObjectTypeCBP = 9,
        ppcbObjectTypeSBP = 10,
        ppcbObjectTypeWirebond = 11,
        ppcbObjectTypeNetClass = 12,
        ppcbObjectTypeDrawing = 13,
        ppcbObjectTypeText = 14,
        ppcbObjectTypeLabel = 15,
        ppcbObjectTypePolyline = 16,
        ppcbObjectTypeCircle = 17,
        ppcbObjectTypeLibrary = 18,
        ppcbObjectTypeLibraryItem = 19,
        ppcbObjectTypeApplication = 20,
        ppcbObjectTypeAttribute = 21,
        ppcbObjectTypeAttributeType = 22,
        ppcbObjectTypeDocument = 23,
        ppcbObjectTypeMeasure = 24,
        ppcbObjectTypeView = 25,
        ppcbObjectTypeAssemblyOptions = 26,
        ppcbObjectTypeAttributes = 27,
        ppcbObjectTypeAttributeTypes = 28,
        ppcbObjectTypeDecal = 29,
        ppcbObjectTypeObjects = 30,
        ppcbObjectTypePadStackLayer = 31,
        ppcbObjectTypePad = 32,
        ppcbObjectTypeThermalPad = 33,
        ppcbObjectTypeAntiPad = 34,
        ppcbObjectTypeLayer = 35,
        ppcbObjectTypeError = 36,
        ppcbObjectTypeErrorConflict = 37,
        ppcbObjectTypeAssociatedNet = 38,
        ppcbObjectTypeAll = 9999,
}
	public class PScript : IPCBIScriptASync
	{
		public PScript()
		{
		}
	public	dynamic padsApp = null;
	public IStep step;
		public void Execute(IPCBIWindow parent)
		{
            step = parent.GetCurrentStep();
            if (step == null) return;
            IFilter filter = new IFilter(parent);



		try
		{
			padsApp = System.Runtime.InteropServices.Marshal.GetActiveObject("PowerPCB.Application");
		}
		catch (Exception)
		{
		}

		if (padsApp == null)
		{
			var excelType = Type.GetTypeFromProgID("PowerPCB.Application");
			padsApp = Activator.CreateInstance(excelType);
		}

		padsApp.UnlockServer();
		
		    parent.PCBISelectionChanged += new EventHandler(parent_PCBISelectionChanged);
            
            while (true)
            {
                Thread.Sleep(1000);

                if (isDisposed)
                {
                    parent.PCBISelectionChanged -= parent_PCBISelectionChanged;
                    break;
				}
			}
            //release the object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(padsApp);			
            }
			
        void parent_PCBISelectionChanged(object sender, EventArgs e)
        {
		padsApp.Visible = true;
		padsApp.ActiveDocument.SelectObjects(PPcbObjectType.ppcbObjectTypeAll, "", false);
            string CMPref = "";
            List<ICMPObject> selectedCMP = step.GetSelectedCMPs();
            if (selectedCMP != null && selectedCMP.Count > 0)
            {
                CMPref = selectedCMP[0].Ref;
            }
		padsApp.ActiveDocument.SelectObjects(PPcbObjectType.ppcbObjectTypeComponent, CMPref , true);
	    padsApp.ActiveDocument.ActiveView.SetExtentsToSelection();	
        }
        bool isDisposed = false;
        public void Dispose()
        {
            isDisposed = true;
        }		
    }
}
