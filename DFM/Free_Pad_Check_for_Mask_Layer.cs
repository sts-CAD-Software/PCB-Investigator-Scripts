//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script for DFM Check
// Created on 23.09.2015
// Autor Fabio Gruber
// support@easylogix.de
// 
// Script for checking top and bot solder mask for free areas above pads.
// If component's available only pin pads are checked, the solder mask should be >= signal pads area.
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
        public void Execute(IPCBIWindow parent)
        {
            //check all pads on top and bot signal layers and mask layers for open space in mask...
            
            IMatrix matrix = parent.GetMatrix();
            IStep step = parent.GetCurrentStep();

            if (step == null || matrix == null) return;

            PCB_Investigator.PCBIWindows.PCBIWorkingDialog working = new PCB_Investigator.PCBIWindows.PCBIWorkingDialog();
            working.CanCancel ( false);
            working.SetStatusText("Checking for free pads on Solder Mask...");
            working.SetAnimationStatus(true);

            working.ShowWorkingDlgAsThread();

            IODBLayer SMTLayerTop = null;
            IODBLayer SMTLayerBot = null;
            IODBLayer SignalTop = (IODBLayer)step.GetLayer(matrix.GetTopSignalLayer());
            IODBLayer SignalBot = (IODBLayer)step.GetLayer(matrix.GetBotSignalLayer(false));
            bool signalBlockFound = false;
            foreach (string layername in step.GetAllLayerNames())
            {
                if (!signalBlockFound && matrix.GetMatrixLayerType(layername) == MatrixLayerType.Solder_mask && SMTLayerTop == null)
                {
                    SMTLayerTop = (IODBLayer)step.GetLayer(layername);
                }
                else if (signalBlockFound && matrix.GetMatrixLayerType(layername) == MatrixLayerType.Solder_mask && SMTLayerBot == null)
                {
                    SMTLayerBot = (IODBLayer)step.GetLayer(layername);
                }
            }
            //check signal pads free?

            bool topPadsFree = true;
            bool botPadsFree = true;
            if (SignalTop != null && SMTLayerTop != null)
            {
                ICMPLayer topCMPLayer = step.GetCMPLayer(true);
                if(topCMPLayer==null)
                {
                foreach (IODBObject checkingPad in SignalTop.GetAllLayerObjects())
                {
                    bool? foundFreeArea = CheckPadsForFreeMask(SMTLayerTop, checkingPad );
                    if (foundFreeArea == false)
                    {
                        checkingPad.ObjectColorTemporary(Color.DarkBlue);
                        topPadsFree = false;
                    }
                }}
                else{
                    topPadsFree = CheckPadsForFreeMask(SMTLayerTop, SignalTop, topCMPLayer);
                }
            }
            if (SignalBot != null && SMTLayerBot != null)
            {
                ICMPLayer botCMPLayer = step.GetCMPLayer(false);
                if (botCMPLayer == null)
                {
                    foreach (IODBObject checkingPad in SignalBot.GetAllLayerObjects())
                    {
                        bool? foundFreeArea = CheckPadsForFreeMask(SMTLayerBot, checkingPad);
                        if (foundFreeArea == false)
                        {
                            checkingPad.ObjectColorTemporary(Color.DarkBlue);
                            botPadsFree = false;
                        }
                    }
                }
                else
                {
                    botPadsFree = CheckPadsForFreeMask(SMTLayerTop, SignalBot, botCMPLayer);
                }
            }

            working.DoClose();
            
            if (topPadsFree && botPadsFree)
            {
                PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("Top and Bot Signal Layer Pads free on mask layers.", "Both Sides free", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (!topPadsFree && !botPadsFree)
            {
                PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("Please check both sides manualy, there are closed pads found.", "No Side free", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                if (topPadsFree)
                    PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("Top Side is ok, all pads free on mask layer top.  Check bot layer for blue elements!", "Top OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("Bot Side is ok, all pads free on mask layer bot.  Check top layer for blue elements!", "Bot OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            parent.UpdateView();
        }

        private static bool? CheckPadsForFreeMask(IODBLayer SMLayer, IODBObject checkingPad)
        {
            if (checkingPad.Type == IObjectType.Pad)
            {
                bool foundFreeArea = false;
                //check is it free on mask layer?

                    #region without cmps
                    foreach (IODBObject maskObjects in SMLayer.GetAllObjectInRectangle(checkingPad.GetBoundsD()))
                    {
                        if (maskObjects.Bounds.Contains(checkingPad.Bounds)) //bounds included in mask object?
                        {
                            if (!maskObjects.DoesIntersect(checkingPad) || maskObjects.GetPolygonOutline().isEqualsTol(checkingPad.GetPolygonOutline())) //no intersection?
                            {
                                if (maskObjects.Type == IObjectType.Surface)
                                {
                                    //surface is special with holes and convex or concave elements -> one more test
                                    if (maskObjects.IsPointOfSecondObjectIncluded(checkingPad))
                                    {
                                        foundFreeArea = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    //mask object should be free!
                                    foundFreeArea = true;
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                
                return foundFreeArea;
            }
            return null;
        }
        private static bool CheckPadsForFreeMask(IODBLayer SMLayer, IODBLayer SignalLayer, ICMPLayer compLayer)
        {
            bool foundFreeArea = true;

            #region with cmp check
            foreach (ICMPObject comp in compLayer.GetAllLayerObjects())
            {
                foreach (IPin pin in comp.GetPinList()) //check each pin for free signal pad
                {
                    bool foundForThisPin = false;
                    #region one pin
                    foreach (IODBObject checkingPad in SignalLayer.GetAllObjectInRectangle(pin.GetBounds(comp)))
                    {
                        if (checkingPad.Type != IObjectType.Pad) continue;

                        foreach (IODBObject maskObjects in SMLayer.GetAllObjectInRectangle(pin.GetBounds(comp)))
                        {
                            if (maskObjects.Bounds.Contains(checkingPad.Bounds)) //bounds included in mask object?
                            {
                                if (!maskObjects.DoesIntersect(checkingPad) || maskObjects.GetPolygonOutline().isEqualsTol(checkingPad.GetPolygonOutline())) //no intersection?
                                {
                                    if (maskObjects.Type == IObjectType.Surface)
                                    {
                                        //surface is special with holes and convex or concave elements -> one more test
                                        if (maskObjects.IsPointOfSecondObjectIncluded(checkingPad))
                                        {
                                            foundForThisPin = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        //mask object should be free!
                                        foundForThisPin = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (foundForThisPin) break;
                    }
                    #endregion
                    if (!foundForThisPin) //add color to find issue on layer
                    {
                        foreach (IODBObject checkingPad in SignalLayer.GetAllObjectInRectangle(pin.GetBounds(comp)))
                        {
                            if (checkingPad.Type != IObjectType.Pad) continue;
                            checkingPad.ObjectColorTemporary(Color.DarkBlue);
                        }
                    }

                    if (!foundForThisPin) foundFreeArea = false;
                }
            }
            #endregion
            return foundFreeArea;
        }

    }
}
