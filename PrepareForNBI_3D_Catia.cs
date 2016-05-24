//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 24.05.2016
// Autor Guenther Schindler and Fabio Gruber

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
        //change if your NBI location is different:
        string pathNBICatia = @"C:\Program Files (x86)\NativeBoardImport\NBI Catia V5\NBI_Catia_V5.exe";
            
		private IPCBIWindow Parent;
		public void Execute(IPCBIWindow parent)
		{
			//your code here
            this.Parent = parent;

			if (MakeWorkingCopy() == DialogResult.OK)
			{
				RemoveObjectsOutsideProfile(parent);
				parent.UIAction.Execute(ID_ActionItem.ID_CHANGE_UNIT_To_MILS); //important for odb string
                PrepareFor3D_inNBI(parent);
			}

			parent.UpdateView();
		}

		private DialogResult MakeWorkingCopy()
		{
			DialogResult res = DialogResult.Abort;
			string stepName = this.Parent.GetCurrentStep().Name;
			string pathForWorkingODB = Path.GetTempPath() + "NBI\\" +Parent.GetJobName()+"_"+ stepName + DateTime.Now.Ticks+ "\\";
            
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
                //res = MessageBox.Show("Continue", "Workingcopy is loaded", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                return DialogResult.OK;
            }
            return res;
        }
        private void PrepareFor3D_inNBI(IPCBIWindow Parent)
        {
            IMatrix matrix = Parent.GetMatrix();
            IStep step = Parent.GetCurrentStep();

            MakeSolderMaskPositive(matrix);
            MakeDrillCutOutOnCopperLayers(Parent, matrix, step);

            Parent.UIAction.Execute(ID_ActionItem.ID_CLEAR_SELECTION);
            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_LAYER_OFF);
            Parent.UIAction.Execute(ID_ActionItem.ID_ALL_SIGNAL_LAYER_ON);
            Parent.UpdateView();
            
            Parent.UIAction.Execute(ID_ActionItem.ID_SAVE);

            String JobLocation = Parent.GetODBJobDirectory();
                                    
            if (File.Exists(pathNBICatia))
            {
                try
                {
                    ProcessStartInfo ps = new ProcessStartInfo(pathNBICatia, "\"" + JobLocation + "\"");
                    Process.Start(ps);
                  //  Process.Start(pathNBICatia + " " + "\"" +  JobLocation + "\"");
		}

                catch (Exception ex)
		{

                    PCB_Investigator.Localization.PCBILocalization.ShowMsgBox("Can not open NBI, because "+ex.Message.ToString(), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
	}
}

        private static void MakeDrillCutOutOnCopperLayers(IPCBIWindow Parent, IMatrix matrix, IStep step)
{

            IFilter filter = new IFilter(Parent);
            int counterDielectirc = 1;
            foreach (string layerName in matrix.GetAllDrillLayerNames())
	{

                ILayer drill_layer = step.GetLayer(layerName);

                bool lastWasSignal = false;
                foreach (string sigLayer in matrix.GetAllLayerWithThisDrills(layerName))
		{

                    var type = matrix.GetMatrixLayerType(sigLayer);
                    if (type != MatrixLayerType.Dielectric && !matrix.IsSignalLayer(sigLayer)) //only dielectric und signals
                        continue;
                    if ((type == MatrixLayerType.Power_ground || type == MatrixLayerType.Signal))
			{

                        if (lastWasSignal)
				{
					//if last and current one are signals -> we need a dielectric between both
                            IODBLayer dielectric = filter.CreateEmptyODBLayer("dielectric_" + counterDielectirc, step.Name);
                            counterDielectirc++;
                            if (dielectric != null)
					{

                                AddDrillObjects(filter, drill_layer, MatrixLayerType.Dielectric, dielectric);
                                matrix.SetMatrixLayerIndex(dielectric.GetLayerName(), matrix.GetRawIndexByName(sigLayer));
                                matrix.SetMatrixLayerParameter(dielectric.GetLayerName(), MatrixLayerContext.Board, MatrixLayerPolarity.Negative, MatrixLayerType.Dielectric, -1, -1);
                                matrix.SetLayerHeight(dielectric.GetLayerName(), 5); //value in mils
                                matrix.UpdateDataAndList(); //update list for correct order
					}
                            else
                                Debug.WriteLine("error, can't create dielectric...");
				}

                        lastWasSignal = true;
			}
                    else
                        lastWasSignal = false;

                    IODBLayer odb_sig_layer = (IODBLayer)step.GetLayer(sigLayer);

                    if (drill_layer.GetType() == typeof(IODBLayer))
			{

                        AddDrillObjects(filter, drill_layer, type, odb_sig_layer);

                        drill_layer.EnableLayer(false);
                        odb_sig_layer.EnableLayer(true);
			}
		}
	}

            matrix.UpdateDataAndList();
            Parent.UpdateControlsAndResetView();
}

        private static void AddDrillObjects(IFilter filter, ILayer drill_layer, MatrixLayerType type, IODBLayer odb_sig_layer)
{

            Dictionary<int, int> symbolUsed = new Dictionary<int, int>();
            foreach (IODBObject obj in drill_layer.GetAllLayerObjects())
	{

                #region one drill layer
                obj.Select(true);
                if (obj.Type == IObjectType.Pad)
		{

                    IPadSpecifics ops = (IPadSpecifics)obj.GetSpecifics();
                    if (type != MatrixLayerType.Dielectric)
                        ops.Positive = false;
                    else
                        ops.Positive = true;

                    if (!symbolUsed.ContainsKey(ops.ShapeIndex))
			{

                        int index = IFilter.AddToolFromODBString(odb_sig_layer, ops.ODBSymbol_String);
                        symbolUsed.Add(ops.ShapeIndex, index);
			}

                    ops.ShapeIndex = symbolUsed[ops.ShapeIndex];

                    IODBObject pad = filter.CreatePad(odb_sig_layer);
                    pad.SetSpecifics(ops);
		}
                else if (obj.Type == IObjectType.Line)
		{

                    ILineSpecifics ops = (ILineSpecifics)obj.GetSpecifics();
                    if (type != MatrixLayerType.Dielectric)
                        ops.Positive = false;
                    else
                        ops.Positive = true;

                    if (!symbolUsed.ContainsKey(ops.ShapeIndex))
			{

                        int index = IFilter.AddToolFromODBString(odb_sig_layer, ops.ODBSymbol_String);
                        symbolUsed.Add(ops.ShapeIndex, index);
			}

                    ops.ShapeIndex = symbolUsed[ops.ShapeIndex];

                    IODBObject line = filter.CreateLine(odb_sig_layer);
                    line.SetSpecifics(ops);
		}

                #endregion
	}
}

        private static void MakeSolderMaskPositive(IMatrix matrix)
{

            foreach (string layerName in matrix.GetAllLayerNames())
	{

                if (matrix.GetMatrixLayerType(layerName) == MatrixLayerType.Solder_mask && matrix.GetMatrixLayerContext(layerName) == MatrixLayerContext.Board)
		{

                    matrix.MakeLayerPositive(layerName);
		}
	}
}
		public void RemoveObjectsOutsideProfile(IPCBIWindow Parent)
        {
            IStep step = Parent.GetCurrentStep();
            IMatrix matrix = Parent.GetMatrix();
            if (step == null) return;
            IODBObject profile = step.GetPCBOutlineAsODBObject();

            foreach (string layerName in step.GetAllLayerNames())
            {
                if (matrix.GetMatrixLayerType(layerName) == MatrixLayerType.Component)
                    continue; //no component layers

                ILayer Layer = step.GetLayer(layerName);
                if (Layer is IODBLayer)
                {
                    List<IODBObject> objectsToDelete = new List<IODBObject>();
                    IODBLayer layer = (IODBLayer)Layer;
                    foreach (IODBObject obj in layer.GetAllLayerObjects())
                    {
                        if (profile.IsPointOfSecondObjectIncluded(obj))
                        {
                            //inside not delete
                        }
                        else
                        {
                            objectsToDelete.Add(obj);
                        }
                    }
                    layer.RemoveObjects(objectsToDelete);
                }
            }
            Parent.UpdateView();
        }    
    }
}