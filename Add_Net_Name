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
			Dictionary<string, WorkingClass.netItem> NetList; //list of all created nets
            NetList = new Dictionary<string, WorkingClass.netItem>();
			WorkingClass.lastNetNr = 0; //next job start new

		    WorkingClass.SetNetNameToSelection(parent, NetList, "Net1");
						           
		//	WorkingClass.AutoCreateNetNames(parent, NetList, "Net");
			
		}
	}
    public class WorkingClass
    {
        public static int lastNetNr = 0; //counter for all nets

        /// <summary>
        /// Set Net Name to the selected objects.
        /// </summary>
        /// <param name="parent">The parent IPCBIWindow</param>
        /// <param name="NetList">A list of all created nets</param>
        /// <param name="netName">The selected net name</param>
        public static void SetNetNameToSelection(IPCBIWindow parent, Dictionary<string, netItem> NetList, string netName)
        {
            IStep currentStep = parent.GetCurrentStep();

            if (currentStep == null) return; //maybe there are no Job loaded?

            List<IODBObject> ListOfSelection = currentStep.GetSelectedElements();

            foreach (IODBObject objSelected in ListOfSelection)
            {
                ILayer parentLayer = currentStep.GetLayer(objSelected.GetParentLayerName());
                if (parentLayer == null) continue;

                if (parentLayer is IODBLayer)
                    AddNetInfos(NetList, currentStep, (IODBLayer)parentLayer, netName, objSelected);
            }
        }

        /// <summary>
        /// Set for all layers netnames, if no connection it will be named with $NONE$
        /// </summary>
        /// <param name="parent">The parent IPCBIWindow</param>
        /// <param name="NetList">A list of all created nets</param>
        /// <param name="netName">The selected net name</param>
        public static void AutoCreateNetNames(IPCBIWindow parent, Dictionary<string, netItem> NetList, string netNameStart)
        {
            IStep currentStep = parent.GetCurrentStep();

            if (currentStep == null) return; //maybe there are no Job loaded?

            IMatrix matrix = parent.GetMatrix();
            foreach (string layername in currentStep.GetAllLayerNames()) //fo all layers
            {
                //check type
                if (matrix.IsImageLayer(layername)) continue;
                if (matrix.GetMatrixLayerType(layername) == MatrixLayerType.Component) continue;

                IODBLayer parentLayer = (IODBLayer)currentStep.GetLayer(layername); //this works only because we have no image and no cmp layers

                parentLayer.CreateLayerNetList(true); //create internal net list

                foreach (IODBObject objODB in parentLayer.GetAllLayerObjects()) //all items
                {
                    if (objODB.LayerNetNumber == -1)
                    {
                        AddNetInfos(NetList, currentStep, parentLayer, "$NONE$", objODB);       //no connection -> $NONE$-Net                 
                    }
                    else
                    {
                        AddNetInfos(NetList, currentStep, parentLayer, netNameStart + layername + "_" + objODB.LayerNetNumber, objODB);
                    }
                }
            }
        }

        /// <summary>
        /// You can use this method to add netinformation to an object.
        /// </summary>
        /// <param name="NetList">Hold all information in your own list.</param>
        /// <param name="step">The relevant step in PCB-I.</param>
        /// <param name="parentLayer">The layer from the relevant object.</param>
        /// <param name="NetName">The net name for this object.</param>
        /// <param name="netItem">This can be e.g. a pad or line or arc.</param>
        private static void AddNetInfos(Dictionary<string, netItem> NetList, IStep step, IODBLayer parentLayer, String NetName, IODBObject netItem)
        {
            if (NetName.Length == 0) //the default net
                NetName = "$NONE$";
            if (!NetList.ContainsKey(NetName)) //create a new net in the step.
            {
                INet outNet;
                int netNr = step.AddNet(NetName, "", out outNet);
                netItem netI = new netItem(netNr, outNet);
                NetList.Add(NetName, netI);
            }

            netItem.PcbNetNumber = NetList[NetName].netNr; //set the netinformation to the object
            int newNr = NetList[NetName].Net.AddLayerRef(parentLayer.GetLayerName(), lastNetNr); //each net saves a list of all layers are used.
            if (newNr == lastNetNr)
                lastNetNr++;
            parentLayer.SetNetNumber(NetName, netItem.PcbNetNumber);//for each object the layer needs information that this net is used.
            NetList[NetName].Net.AddFID(PCBI.FidType.Copper, parentLayer.GetLayerName(), netItem.GetIndexOnLayer(), ""); //this is specific to ODB++, if you don't use it and save the data as ODB++ some information get lost.
        }

        //helper class for nets
        public class netItem
        {
            internal int netNr = -1;
            internal INet Net;
            internal netItem(int netNr, INet Net)
            {
                this.netNr = netNr;
                this.Net = Net;
            }
        }


    }	
}
