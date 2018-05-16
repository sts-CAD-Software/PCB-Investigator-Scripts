//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script (Synchronous)
// Created on 11.12.2017
// Author Michael.Dorfner
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/sdk-participate
// Updated: 11.12.2017
// Description: Export Seica data to par,nod,inf file
//-----------------------------------------------------------------------------------
// Favorite scripts can be placed in the ribbon menu. Therefore a unique GUID and a ButtonEnabled state is mandatory:
// GUID=Exp-to-Seica_636485805346479961
// ButtonEnabled=5   (Button enabled if: 1=Design is loaded, 2=Always, 4=Design contains components, 8=Loaded step is a panel, 16=Element is selected, 32=Component is selected)

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
        private static int digits = 3;
        private static bool useTHTSearch = true;            // switch if tht component search should be used automatically
		private static int betterToRead = 1;
       
		public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            IMatrix matrix = parent.GetMatrix();
            List<string> errorLogFile = new List<string>();
            IStep step = parent.GetCurrentStep();
            string fileName = string.Empty;
            parFile(parent, ref fileName, step, ref errorLogFile);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try
                {
                    nodFile(parent, fileName, step, matrix, ref errorLogFile);
                }
                catch (Exception exc)
                {
                    fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".txt");
                    writeToFile(fileName, errorLogFile);
                }
            }

            parent.UpdateView();
        }
        private void parFile(IPCBIWindow parent, ref string strFileName, IStep step, ref List<string> errorLogFile)
        {
            List<ICMPObject> lstAllCMPList = step.GetAllCMPObjects();
            Dictionary<string, string> dictMacros = new Dictionary<string, string>();       // you can add and remove dictMacros.Add entries, these entries are used for naming a CMP-Macro correctly 
            List<string> dataList = new List<string>();

            dictMacros.Add("c", "$CAPACITOR");                                              // the first part of the entry, here "c", is the starting character of the reference string on which it is compared 
            dictMacros.Add("c_", "$CAPACITOR");                                             // the second part of the entry, here "$CAPACITATOR", is the new macroentry
            dictMacros.Add("d", "$DIODE");
            dictMacros.Add("fd", "$FIDUCIAL");
            dictMacros.Add("jp", "$JUMPER");
            dictMacros.Add("l", "$INDUCTANCE");
            dictMacros.Add("l_", "$INDUCTANCE");
            dictMacros.Add("ofix", "$OPENFIX");
            dictMacros.Add("pos", "$MOVEPROBE");
            dictMacros.Add("probe", "$IMPCHECK");
            dictMacros.Add("r", "$RESISTOR");

            // some Macros are missing:
            //$CONNECTOR, $FUSE, $CAPACITOR, $LED, $TRANSISTOR, 
            //$MOSFET, $TRANSFORMER, $RELAIS, $ZENER, $DIGITAL, $CLOCK, $OPTOISOLATOR, $VOLTAGEREGULATOR
            // please add more or remove some if necessary on your own.  


            char chrTabSpace = '\t'; //  There must be a tab between the columns to ensure that the columns are also separated for the machine.
            strFileName = FileSaveDialog(parent);
            int iMaxCMPName, iMaxPackName;
            iMaxCMPName = 0;
            iMaxPackName = 0;

            // to get the maximum name length
            if (lstAllCMPList.Count > 0)
            {
                
                foreach (ICMPObject cmp in lstAllCMPList)
                {
            
                    if (iMaxCMPName < cmp.PartName.Length)
                    {
                        iMaxCMPName = cmp.PartName.Length;
                    }
                    if (iMaxPackName < cmp.UsedPackageName.Length)
                    {
                        iMaxPackName = cmp.UsedPackageName.Length;
                    }
                }
            }
            else
            {
                MessageBox.Show("No componentens could be found");
                return;
            }

            iMaxCMPName = iMaxCMPName + 5;
            iMaxPackName = iMaxPackName + 5;


            if (string.IsNullOrEmpty(strFileName))
            {
                MessageBox.Show("Files could not be exported because of an error. Please try it again.", "Error occured", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            dataList.Add("* COMPONENT".PadRight(iMaxCMPName, chrTabSpace) + " DEVICE".PadRight(iMaxPackName, chrTabSpace) + "VALUE".PadRight(15, chrTabSpace) + "UNIT".PadRight(15, chrTabSpace) + "TOL+".PadRight(15, chrTabSpace) + "TOL-".PadRight(15, chrTabSpace) + "NRPIN".PadRight(15, chrTabSpace) + "MACRO");
            dataList.Add("*  ");

            string makroName = string.Empty;
            string cmpRefName = string.Empty;
            string cmpUsedPckName = string.Empty;

            foreach (ICMPObject cmp in lstAllCMPList)
            {
                if (dictMacros != null)
                {
                    makroName = string.Empty;
                    foreach (var item in dictMacros)
                    {
                        if (cmp.Ref.ToLowerInvariant().StartsWith(item.Key))
                        {
                            makroName = item.Value;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(makroName))
                {
                    makroName = "";
                }
                cmpRefName = '"' + cmp.Ref + '"';
                cmpUsedPckName = '"' + cmp.UsedPackageName + '"';

                dataList.Add(cmpRefName.PadRight(iMaxCMPName, chrTabSpace) + cmpUsedPckName.PadRight(iMaxPackName, chrTabSpace) + "0".PadRight(15, chrTabSpace) + "0".PadRight(15, chrTabSpace) + "0".PadRight(15, chrTabSpace) + "0".PadRight(15, chrTabSpace) + "0".PadRight(15, chrTabSpace) + makroName);
            }
            writeToFile(strFileName, dataList);

        }
        private void nodFile(IPCBIWindow parent, string fileName, IStep step, IMatrix matrix, ref List<string> errorLogFile)
        {

            List<string> lstStrNetNames = step.GetAllNetNames();
            List<ICMPObject> lstAllCMPList = step.GetAllCMPObjects();

            List<IODBObject> lstListOfAllDrillObjects = creatingDrillList(matrix, step);
            if (lstListOfAllDrillObjects == null)
            {
                errorLogFile.Add("Method: nodFile + lstListOfAllDrillObjects is null");
                return;
            }
            List<ThtPin> topTHTPinList, botTHTPinList = new List<ThtPin>();
            List<INet> lstINetINetList = new List<INet>();
            List<string> dataList;


            fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".nod");

            foreach (string netName in lstStrNetNames)
            {
                lstINetINetList.Add(step.GetNet(netName));
            }


            checkPinLaysOverHole(step, lstListOfAllDrillObjects, out topTHTPinList, out botTHTPinList, matrix);

            fillNODDataList(parent, step, matrix, lstAllCMPList, topTHTPinList, botTHTPinList, out dataList, lstINetINetList, lstListOfAllDrillObjects, ref errorLogFile);

            if (dataList != null && !string.IsNullOrEmpty(fileName))
            {
                writeToFile(fileName, dataList);
            }
        }
        private void fillNODDataList(IPCBIWindow parent, IStep step, IMatrix matrix, List<ICMPObject> lstAllCMPList, List<ThtPin> topTHTPinList, List<ThtPin> botTHTPinList, out List<string> dataList, List<INet> lstINetINetList, List<IODBObject> lstListOfAllDrillObjects, ref List<string> errorLogFile)
        {
            int inCounter4NONENetName = 0;
            dataList = new List<string>();
            string strTopSigLay = matrix.GetTopSignalLayer();
            string strBotSigLay = matrix.GetBotSignalLayer();
            IODBLayer iodblayTopSig, iodblayBotSig, iodblayCurrSigLay;
            string strPosition = string.Empty;
            string strTecn = string.Empty;
            string strNetName = string.Empty;
            string strCmpRef = string.Empty;
            string strInfo = string.Empty;
            ICMPObject iCmOCMPObject;
            char chrWhiteSpa = ' ';
            int minAbstand = 15;
            int maxAbstand = 52;
			int dist = betterToRead;



            if (lstAllCMPList == null)
            {
                errorLogFile.Add("Method: fillNODDataList + lstAllCMPList is null");
                return;
            }


            // to get the maximum name length
            /*if (lstAllCMPList.Count > 0)
            {
                foreach (ICMPObject cmp in lstAllCMPList)
                {
                    if (cmp != null && !string.IsNullOrEmpty(cmp.PartName))
                    {
                        if (maxAbstand < cmp.PartName.Length)
                        {
                            maxAbstand = cmp.PartName.Length;
                        }
                    }
                    else
                    {
                        errorLogFile.Add("Method: fillNODDataList + lstAllCMPList is null");
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("No componentens could be found");
                return;
            }



            if (lstINetINetList.Count > 0)
            {
                foreach (INet tempNet in lstINetINetList)
                {
                    if (tempNet != null && !string.IsNullOrEmpty(tempNet.NetName))
                    {
                        if (maxAbstand < tempNet.NetName.Length)
                        {
                            maxAbstand = tempNet.NetName.Length;
                        }
                    }
                    else
                    {
                        errorLogFile.Add("Method: fillNODDataList + lstINetInetList is null");
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("No componentens could be found");
                return;
            }*/

            maxAbstand = maxAbstand + 2;


            if (!string.IsNullOrEmpty(strTopSigLay))
            {
                iodblayTopSig = (IODBLayer)step.GetLayer(strTopSigLay);
            }
            else
            {
                errorLogFile.Add("Method: fillNODDataList + strTopSigLay is null");
                iodblayTopSig = null;
            }

            if (!string.IsNullOrEmpty(strBotSigLay))
            {
                iodblayBotSig = (IODBLayer)step.GetLayer(strBotSigLay);
            }
            else
            {
                errorLogFile.Add("Method: fillNODDataList + strBotSigLay is null");
                iodblayBotSig = null;
            }

            dataList.Add("* " + "NetName".PadRight(minAbstand, chrWhiteSpa) + "CMP-Ref".PadRight(minAbstand, chrWhiteSpa) + "PinNr.".PadRight(minAbstand, chrWhiteSpa) + "X".PadRight(minAbstand, chrWhiteSpa) + "Y".PadRight(minAbstand, chrWhiteSpa) + "PadSize".PadRight(minAbstand, chrWhiteSpa) + "Pinposition".PadRight(minAbstand, chrWhiteSpa) + "CMP-Typ".PadRight(minAbstand, chrWhiteSpa) + "TEST".PadRight(minAbstand, chrWhiteSpa) + "CHANNEL".PadRight(minAbstand, chrWhiteSpa) + "USER");
            dataList.Add("* ");

            foreach (INet net in lstINetINetList)
            {
                if (net == null)
                {
                    errorLogFile.Add("Method: fillNODDataList + net is null");
                    return;
                }
                List<INetObject> lstNetComponentList = net.ComponentList;

                if (lstNetComponentList == null)
                {
                    errorLogFile.Add("Method: fillNODDataList + lstNetComponentList is null");
                    return;
                }
                foreach (INetObject inetObj in lstNetComponentList)
                {
                    iCmOCMPObject = inetObj.ICMP;
                    if (iCmOCMPObject == null)
                    {
                        errorLogFile.Add("Method: fillNODDataList + iCmOCMPObject is null");
                        continue;
                    }
                    strTecn = string.Empty;


                    bool boPinIsTopSoldered = true;
                    bool boPinIsPartOfTHT = false;

                    if (!useTHTSearch)
                    {
                        boPinIsTopSoldered = iCmOCMPObject.PlacedTop;
                    }
                    else
                    {
                        if (iCmOCMPObject.PlacedTop)        // true is top layer
                        {
                            boPinIsTopSoldered = true;

                            foreach (var entry in topTHTPinList)
                            {
                                if (entry == null)
                                {

                                    errorLogFile.Add("Method: fillNODDataList + entry is null");
                                    return;

                                }
                                if (entry.Cmp.Ref == iCmOCMPObject.Ref)
                                {
                                    boPinIsPartOfTHT = true;
                                    boPinIsTopSoldered = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            boPinIsTopSoldered = false;

                            foreach (var entry in botTHTPinList)
                            {
                                if (entry == null)
                                {

                                    errorLogFile.Add("Method: fillNODDataList + entry is null");
                                    return;

                                }
                                if (entry.Cmp.Ref == iCmOCMPObject.Ref)
                                {
                                    boPinIsPartOfTHT = true;
                                    boPinIsTopSoldered = true;
                                    break;
                                }
                            }
                        }
                    }


                    if (boPinIsPartOfTHT == true)
                    {
                        strTecn = "T";
                    }
                    else
                    {
                        strTecn = "S";
                    }

                    if (boPinIsTopSoldered)
                    {
                        strPosition = "T";
                        if (iodblayTopSig != null)
                        {


                            iodblayCurrSigLay = iodblayTopSig;
                        }
                        else
                        {

                            errorLogFile.Add("Method: fillNODDataList + iodblayTopSig is null");
                            return;

                        }
                    }
                    else
                    {
                        strPosition = "B";
                        if (iodblayBotSig != null)
                        {


                            iodblayCurrSigLay = iodblayBotSig;
                        }
                        else
                        {

                            errorLogFile.Add("Method: fillNODDataList + iodblayBotSig is null");
                            return;

                        }
                    }

                    IPin pin = inetObj.GetIPin();
                    {
                        if (pin == null)
                        {
                            errorLogFile.Add("Method: fillNODDataList + pin is null");
                            continue;
                        }
                        IODBObject iodbobPinPad = null;
                        if (iodblayCurrSigLay != null)
                        {
                            iodbobPinPad = pin.GetIPinPad(iodblayCurrSigLay, iCmOCMPObject);
                        }

                        if (net.NetName == null)
                        {
                            errorLogFile.Add("Method: fillNODDataList + net.NetName is null");
                            return;
                        }
                        if (net.NetName == "$NONE$")
                        {
                            strNetName = '"' + net.NetName + inCounter4NONENetName.ToString(System.Globalization.CultureInfo.InvariantCulture) + '"';
                            inCounter4NONENetName++;
                        }
                        else
                        {
                            strNetName = '"' + net.NetName + '"';
                        }
                        strCmpRef = '"' + iCmOCMPObject.Ref + '"';

                        string strPadString = string.Empty;
                        double holeDiameter = 0;
                        bool rotationExists = false;
                        bool holeExists = false;
                        double doWidth = 0, doHeight = 0, doDegree = 0;


                        if (iodbobPinPad != null)
                        {
                            IObjectSpecificsD spec = iodbobPinPad.GetSpecificsD();
                            if (spec == null)
                            {
                                errorLogFile.Add("Method: fillNODDataList + spec is null");
                                return;
                            }

                            if (spec is IPadSpecificsD)
                            {
                                IPadSpecificsD pSpec = (IPadSpecificsD)spec;
                                //pSpec.ShapeIndex
                                if (pSpec == null)
                                {
                                    errorLogFile.Add("Method: fillNODDataList + pSpec is null");
                                    return;
                                }
                                IFilter filter = new IFilter(parent);

                                if (filter == null)
                                {
                                    errorLogFile.Add("Method: fillNODDataList + filter is null");
                                    return;
                                }

                                IFilter.ToolDefinition toolDef = filter.GetSymbolByShapeIndex(pSpec.ShapeIndex, iodblayCurrSigLay);
                                if (toolDef == null)
                                {
                                    errorLogFile.Add("Method: fillNODDataList + toolDef is null");
                                    return;
                                }

                                switch (toolDef.Type)
                                {
                                    case PCBI.Symbol_Type.r:            //round

                                        foreach (IODBObject drill in lstListOfAllDrillObjects)
                                        {
                                            if (drill == null)
                                            {
                                                errorLogFile.Add("Method: fillNODDataList + drill is null");
                                                return;
                                            }
                                            if (drill.IsPointOfSecondObjectIncluded(iodbobPinPad))
                                            {
                                                holeExists = true;
                                                holeDiameter = drill.GetDiameter();
                                            }

                                        }
                                        if (holeExists)
                                        {
                                            strPadString = "R" + Math.Round(toolDef.Diameter, 0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "H" + Math.Round(holeDiameter,0).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            strPadString = "R" + Math.Round(toolDef.Diameter, 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        break;

                                    case PCBI.Symbol_Type.s:            // square
                                                                        //toolDef.Diameter;
                                        RectangleD bounds = iodbobPinPad.GetBoundsD();

                                        if (bounds == null)
                                        {
                                            errorLogFile.Add("Method: fillNODDataList + bounds are null");
                                            return;
                                        }

                                        if (pSpec.Rotation != 0)
                                        {
                                            IPolyClass padPoly = iodbobPinPad.GetPolygonOutline();
                                            if (padPoly == null)
                                            {
                                                errorLogFile.Add("Method: fillNODDataList + padPoly is null");
                                                return;
                                            }
                                            padPoly.Rotate(-pSpec.Rotation);
                                            padPoly.UpdateBounds();
                                            bounds = padPoly.GetBounds();
                                        }
                                        strPadString = "X" + Math.Round(bounds.Height, 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                        break;

                                    case PCBI.Symbol_Type.rect:
                                    default:
                                        RectangleD rectBounds = iodbobPinPad.GetBoundsD();
                                        if (rectBounds == null)
                                        {
                                            errorLogFile.Add("Method: fillNODDataList + bounds are null");
                                            return;
                                        }

                                        if (pSpec.Rotation != 0)
                                        {
                                            doDegree = pSpec.Rotation;
                                            rotationExists = true;
                                            IPolyClass padPoly = iodbobPinPad.GetPolygonOutline();
                                            if (padPoly == null)
                                            {
                                                errorLogFile.Add("Method: fillNODDataList + padPoly is null");
                                                return;
                                            }
                                            padPoly.Rotate(-pSpec.Rotation);
                                            padPoly.UpdateBounds();
                                            rectBounds = padPoly.GetBounds();
                                        }
                                        doWidth = rectBounds.Width;
                                        doHeight = rectBounds.Height;

                                        if (rotationExists)
                                        {
                                            strPadString = "X" + Math.Round(doWidth, 0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "Y" + Math.Round(doHeight,0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "A" + Math.Round(doDegree,0).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            foreach (IODBObject drill in lstListOfAllDrillObjects)
                                            {
                                                if (drill == null)
                                                {
                                                    errorLogFile.Add("Method: fillNODDataList + drill is null");
                                                    return;
                                                }
                                                if (drill.IsPointOfSecondObjectIncluded(iodbobPinPad))
                                                {
                                                    holeExists = true;
                                                    holeDiameter = drill.GetDiameter();
                                                }

                                            }
                                            if (holeExists)
                                            {
                                                strPadString = "X" + Math.Round(doWidth, 0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "Y" + Math.Round(doHeight, 0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "H" + Math.Round(holeDiameter, 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                            }
                                            else
                                            {
                                                strPadString = "X" + Math.Round(doWidth, 0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "Y" + Math.Round(doHeight, 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                            }
                                        }


                                        break;
                                }
                            }
                        }
                        else
                        {
                            strPadString = "0";
                            errorLogFile.Add("Method: fillNODDataList + iodbobPinPad is null");
                            return;
                        }
						string strXValue = (Math.Round(IMath.Mils2MM(pin.GetIPinPositionD(iCmOCMPObject).X), digits)).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        string strYValue = (Math.Round(IMath.Mils2MM(pin.GetIPinPositionD(iCmOCMPObject).Y), digits)).ToString(System.Globalization.CultureInfo.InvariantCulture);
						
						
						
                        strInfo = string.Concat(strNetName.PadRight(strNetName.Length+dist, chrWhiteSpa),
                            strCmpRef.PadRight(strCmpRef.Length+dist, chrWhiteSpa),
                            pin.PinNumber.PadRight(pin.PinNumber.Length+dist, chrWhiteSpa),
                            strXValue.PadRight(strXValue.Length+dist, chrWhiteSpa),
                            strYValue.PadRight(strYValue.Length+dist, chrWhiteSpa),
                            strPadString.PadRight(strPadString.Length+dist, chrWhiteSpa),
                            strPosition.PadRight(strPosition.Length+dist, chrWhiteSpa),
                            strTecn.PadRight(strTecn.Length+dist, chrWhiteSpa),
                            "Y".PadRight(2, chrWhiteSpa),
                            "0".PadRight(2, chrWhiteSpa), " ");

                        if (!dataList.Contains(strInfo))
                        {

                            dataList.Add(strInfo);
                        }
                    }
                }
            }
        }

        #region THT FINDING
        internal List<IODBObject> creatingDrillList(IMatrix matrix, IStep step)                   // that method is used for finding all drillobjects which are plated and not SBU-drills                               
        {
            if (step == null)                                        // A job has to be loaded first
            {
                PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("No job loaded.", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return null;
            }

            List<IODBObject> listOfAllDrillObjects = new List<IODBObject>();
            foreach (string drillLayerName in matrix.GetAllDrillLayerNames(true))                   // iterates through all drill layers
            {
                if (!matrix.IsSBUDrill(drillLayerName))                                         // if drill layer not a sbu drill layer(sequentiell build up layer), that means only layer with drills that are drilled through the whole board are accepted
                {
                    ILayer drillLayer = step.GetLayer(drillLayerName);
                    if (drillLayer != null)
                    {
                        foreach (IODBObject drill in drillLayer.GetAllLayerObjects())
                        {
                            Dictionary<PCBI.FeatureAttributeEnum, string> attribs = drill.GetAttributesDictionary();
                            if (attribs.ContainsKey(PCBI.FeatureAttributeEnum.drill) && attribs[PCBI.FeatureAttributeEnum.drill] == "non_plated")       // if the dictionary of all drill attributes contains that it is a drill and has to be non plated
                            {                                                                                                                           // then this foreach loopstep can be skipped
                                continue;
                            }
                            listOfAllDrillObjects.Add(drill);                                           // else that drill has to be added to the relevant drills
                        }
                    }
                }
            }
            return listOfAllDrillObjects;               // return the list with all important drills
        }
        internal bool checkPinLaysOverHole(IStep step, List<IODBObject> listOfAllDrillObjects, out List<ThtPin> topTHTPinList, out List<ThtPin> botTHTPinList, IMatrix matrix)
        {
            // this method tries to find THT components and return a list with these components

            topTHTPinList = new List<ThtPin>();
            botTHTPinList = new List<ThtPin>();
            List<ThtPin> topTemporaryTHTPinList = new List<ThtPin>();           // thats a list of the ThtPin class of the Top component pins that are relevant to be checked
            List<ThtPin> botTemporaryTHTPinList = new List<ThtPin>();           // thats a list of the ThtPin class of the Bot component pins that are relevant to be checked


            if (step == null)
            {
                MessageBox.Show("No job loaded!");
                return false;
            }
            else
            {
                List<ICMPObject> allComponents = step.GetAllCMPObjects();         //  thats how you get all components and filling a list with it
                try
                {
                    foreach (ICMPObject cmpObj in allComponents)                  // now iterating through the list with all components
                    {
                        List<IPin> pinList = cmpObj.GetPinList();                   // now creating a list with all of pins of the currently checked component of the foreach loop which is iterating components
                        if (pinList.Count == 1) { continue; } //TODO Sollte wegen Fiducials geändert werden wenn benötigt

                        foreach (IPin pin in pinList)
                        {
                            int counterTop = 0;
                            int counterBot = 0;

                            IPolyClass pinPoly = pin.GetPolygonOutline(cmpObj);         // this method is creating a new polygon of the polygon outline of the checked component
                            RectangleD pinRect = pin.GetBoundsD(cmpObj);                // creating a new rectangle by getting the bounds of the current checked component
                            foreach (IODBObject holeObj in listOfAllDrillObjects)       // finally the list with all relevant holes is going be checked
                            {
                                if (!pinRect.IntersectsWith(holeObj.GetBoundsD())) continue;        // first the pins rectangle is checked if it is intersecting with a hole. If this condition is true this pin can not be a THT component pin
                                IPolyClass holePoly = holeObj.GetPolygonOutline();
                                if (holePoly.DoesIntersect(pinPoly) || holePoly.IsPointOfSecondObjectIncluded(pinPoly) || pinPoly.IsPointOfSecondObjectIncluded(holePoly))  // this condition is a further check if this pin could be a THT component pin
                                {
                                    RectangleD holeRect = holePoly.GetBounds();
                                    //additional checks can be implemented here
                                    double pinRectArea = pinRect.GetArea();
                                    double holeRectArea = holeRect.GetArea();


                                    if (holeRectArea <= pinRectArea)            // if the hole area is to small this loop iteration will be skipped
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (cmpObj.PlacedTop == true)                                       // placed on the top component layer
                                        {
                                            counterTop++;                                                                // this variable is counting how many holes are below the current checked pin
                                            topTemporaryTHTPinList.Add(new ThtPin(cmpObj, pin, holeObj, holePoly));     // add the current component to a temporary list. 
                                        }
                                        else
                                        {
                                            counterBot++;                                                               // this variable is counting how many holes are below the current checked pin
                                            botTemporaryTHTPinList.Add(new ThtPin(cmpObj, pin, holeObj, holePoly));     // add the current component to a temporary list. 
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            if (counterTop == 1 && pinList.Count == 1)      // for the top THT components
                            {                                                           // if the component, which has been added to the temporary list before, do has only one hole (counterTop == 1) below one pin
                                                                                        // do not add to list                                      // and the component do only posses one pin, this component can't be a THT component.
                            }
                            else
                            {
                                topTHTPinList.AddRange(topTemporaryTHTPinList);         // else the temporary list can be added to the permanent list
                                topTemporaryTHTPinList.Clear();
                            }
                            if (counterBot == 1 && pinList.Count == 1) { }  // for the bot THT components
                            else
                            {
                                botTHTPinList.AddRange(botTemporaryTHTPinList);
                                botTemporaryTHTPinList.Clear();
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)                    // if something goes wrong and a exception have to be catched
                {
                    PCB_Investigator.Localization.PCBILocalization.ShowMsgBox(ex.Message, "Error in checkPinLaysOverHole method", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }
        #endregion
        private void writeToFile(string strFileName, List<string> dataList)
        {
            try
            {
                // Create a new file 
                using (StreamWriter sw = File.CreateText(strFileName))
                {
                    foreach (string entry in dataList)
                    {
                        sw.WriteLine(entry);
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
                MessageBox.Show("Error occured while writing data to file.");
            }

        }
        public string FileSaveDialog(IPCBIWindow parent)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = parent.GetODBJobDirectory();
            saveFileDialog1.FileName = parent.GetJobName();
            saveFileDialog1.Title = "Save Seica Files";
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "par";
            saveFileDialog1.Filter = "PAR files (*.par)|*.par|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog1.FileName;
            }
            return string.Empty;
        }
        #region InternalClass
        internal class ThtPin                               // this class contains 
        {
            public ICMPObject Cmp;                          // a component object
            public IPin Pin;                                // the related pins to the component
            public IODBObject Drill;                        // the drills which are below the pins         
            public IPolyClass ThtPolyDrill;                 // and a polygon of  each the drill below the pins
            public ThtPin(ICMPObject cmp, IPin pin, IODBObject drill, IPolyClass thtPoly)                   // constructor definition
            {
                this.Cmp = cmp; this.Pin = pin; this.Drill = drill; this.ThtPolyDrill = thtPoly;
            }
        }

        #endregion
    }
}
