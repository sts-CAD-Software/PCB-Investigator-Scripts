//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 21.09.2015
// Autor Guenther
// This script prepares ODB++ V8 to load in NBI
// A copy of the job will be made to the tmp directory 
// Soldermask layers will be made positive
// Coppyer layers are cut out with drills
// Job opens in NBI Catia
// Prepare Data for real 3D.
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
using System.Diagnostics;

namespace PCBIScript
{
    public class PScript : IPCBIScript
    {
        public PScript()
        {
        }
        private IPCBIWindow Parent;
        public void Execute(IPCBIWindow parent)
        {
            //your code here
            Parent = parent;

            if (MakeWorkingCopy() == DialogResult.OK)
            {
                PrepareFor3D_inNBI(parent);
            }
            parent.UpdateView();
        }
        private DialogResult MakeWorkingCopy()
        {
            DialogResult res = DialogResult.Abort;
            string stepName = Parent.GetCurrentStep().Name;
            string pathForWorkingODB = Path.GetTempPath() + "NBI\\" + stepName + "\\";
            //  MessageBox.Show(pathForWorkingODB);
            if (Directory.Exists(pathForWorkingODB))
            {
                Directory.Delete(pathForWorkingODB, true);
                System.Threading.Thread.Sleep(400);
            }
            Parent.SaveJob(pathForWorkingODB, false);

            if (!Parent.LoadODBJob(pathForWorkingODB))
                MessageBox.Show("Can't create a temporary working copy of ODB-Data.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            else
            {
                res = MessageBox.Show("Continue", "Workingcopy is loaded", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            }
            return res;
        }
        private void PrepareFor3D_inNBI(IPCBIWindow Parent)
        {

            IMatrix matrix = Parent.GetMatrix();
            IStep step = Parent.GetCurrentStep();

            MakeSolderMaskPositive(matrix);
            CoppyDrillsToBoardOutline(Parent, matrix, step);
            MakeDrillCutOutOnCopperLayers(Parent, matrix, step);

            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);
            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_SIGNAL_LAYER_ON);
            Parent.UpdateView();
            //
            Parent.UIAction.Execute(ID_ActionItem.ID_SAVE);

            String JobLocation = Parent.GetODBJobDirectory();
            Process.Start(@"C:\Program Files (x86)\NativeBoardImport\NBI Catia V5\NBI_Catia_V5.exe" + "\"" + JobLocation + "\"");
        }

        private static void MakeDrillCutOutOnCopperLayers(IPCBIWindow Parent, IMatrix matrix, IStep step)
        {
            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);
            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            foreach (string layerName in matrix.GetAllDrillLayerNames())
            {
                ILayer drill_layer = step.GetLayer(layerName);
                foreach (string sigLayer in matrix.GetAllSignalLayerNames())
                {
                    Parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);
                    Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
                    Parent.UpdateSelection();
                    Parent.UpdateView();
                    IODBLayer odb_sig_layer = (IODBLayer)step.GetLayer(sigLayer);
                    IFilter filter = new IFilter(Parent);
                    if (drill_layer.GetType() == typeof(IODBLayer))
                    {
                        foreach (IODBObject obj in drill_layer.GetAllLayerObjects())
                        {
                            obj.Select(true);
                            if (obj.Type == IObjectType.Pad)
                            {
                                IPadSpecifics ops = (IPadSpecifics)obj.GetSpecifics();
                                ops.Positive = false;
                                obj.SetSpecifics(ops);
                            }
                            else if (obj.Type == IObjectType.Line)
                            {
                                ILineSpecifics ops = (ILineSpecifics)obj.GetSpecifics();
                                ops.Positive = false;
                                obj.SetSpecifics(ops);
                            }
                        }
                        Parent.UpdateSelection();
                        Parent.UIAction.Execute(ID_ActionItem.ID_COPY_SELECTED_ODBOBJECTS);
                        drill_layer.EnableLayer(false);
                        odb_sig_layer.EnableLayer(true);
                        Parent.UpdateSelection();
                        Parent.UpdateView();
                        Parent.UIAction.Execute(ID_ActionItem.ID_PASTE_COPIED_ITEMS);
                        odb_sig_layer.EnableLayer(false);
                    }
                }
            }
            // reset drills positive
            foreach (string layerName in matrix.GetAllDrillLayerNames())
            {
                ILayer drill_layer = step.GetLayer(layerName);

                if (drill_layer.GetType() == typeof(IODBLayer))
                {
                    foreach (IODBObject obj in drill_layer.GetAllLayerObjects())
                    {
                        if (obj.Type == IObjectType.Pad)
                        {
                            IPadSpecifics ops = (IPadSpecifics)obj.GetSpecifics();
                            ops.Positive = true;
                            obj.SetSpecifics(ops);
                        }
                        else if (obj.Type == IObjectType.Line)
                        {
                            ILineSpecifics ops = (ILineSpecifics)obj.GetSpecifics();
                            ops.Positive = true;
                            obj.SetSpecifics(ops);
                        }
                    }
                }
            }
        }

        private static void CoppyDrillsToBoardOutline(IPCBIWindow Parent, IMatrix matrix, IStep step)
        {
            float DrillMinSzize = 15; // mills/1000
            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            foreach (string layerName in matrix.GetAllDrillLayerNames())
            {

                ILayer layer = step.GetLayer(layerName);
                layer.EnableLayer(true);
                if (layer.GetType() == typeof(IODBLayer))
                {
                    foreach (IODBObject obj in layer.GetAllLayerObjects())
                    {
                        if (obj.GetDiameter() > DrillMinSzize)
                        {
                            obj.Select(true);
                        }
                    }
                }

            }
            Parent.UpdateSelection();
            Parent.UIAction.Execute(ID_ActionItem.ID_ADD_TO_OUTLINE_FROM_SELECTION);
        }
        private static void MakeSolderMaskPositive(IMatrix matrix)
        {
            foreach (string layerName in matrix.GetAllLayerNames())
            {
                //if (matrix.GetMatrixLayerType(layerName) == MatrixLayerType.Solder_mask && matrix.GetMatrixLayerContext(layerName) == MatrixLayerContext.Board && matrix.GetMatrixLayerPolarity(layerName) == MatrixLayerPolarity.Negative)
                if (matrix.GetMatrixLayerType(layerName) == MatrixLayerType.Solder_mask && matrix.GetMatrixLayerContext(layerName) == MatrixLayerContext.Board)
                {
                    matrix.MakeLayerPositive(layerName);
                }
            }
        }
    }
}