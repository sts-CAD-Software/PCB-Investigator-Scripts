//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 2014-04-24
// Autor 
// 
// Make copy of job and open it in temporary path.
//-----------------------------------------------------------------------------------
// The script is  used to create and open a temporary working copy of a job.
// Its code retrieves the temporary storage path and stores the current job in the temporary path. 
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
            
			string pathForWorkingODB = Path.GetTempPath() + "example\\WorkingJob";
            		parent.SaveJob(pathForWorkingODB, false);

            		if (!parent.LoadODBJob(pathForWorkingODB))
                		MessageBox.Show("Can't create a temporary working copy of ODB-Data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			else
				MessageBox.Show("Workingcopy is loaded.", "Finish", MessageBoxButtons.OK, MessageBoxIcon.None);
        	}
		
    	}
}
