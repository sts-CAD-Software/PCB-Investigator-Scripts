//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 27.05.2015
// Autor Gruber Fabio
// 
// Clear Attributes of components. Input Box for Attribute name.
//-----------------------------------------------------------------------------------
// The script is used to browse all component objects in a given step and remove a specific attribute from them.
// The attribute is entered in a dialog box that is displayed to the user when the script is executed.
// It can be useful when the user needs to remove an attribute from several component objects at once. 
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
		string Attrib = "";
		ShowInputDialog(ref Attrib);
            IMatrix m = parent.GetMatrix();
            IStep step = parent.GetCurrentStep();


            foreach (ICMPObject comp in step.GetAllCMPObjects())
            {
                comp.RemoveAttribute( Attrib);
            }
            parent.UpdateView(); 
         }


private static DialogResult ShowInputDialog(ref string input)
    {
        System.Drawing.Size size = new System.Drawing.Size(200, 70);
        Form inputBox = new Form();

        inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        inputBox.ClientSize = size;
        inputBox.Text = "Attribute Value";

        System.Windows.Forms.TextBox textBox = new TextBox();
        textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
        textBox.Location = new System.Drawing.Point(5, 5);
        textBox.Text = input;
        inputBox.Controls.Add(textBox);

        Button okButton = new Button();
        okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        okButton.Name = "okButton";
        okButton.Size = new System.Drawing.Size(75, 23);
        okButton.Text = "&OK";
        okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
        inputBox.Controls.Add(okButton);

        Button cancelButton = new Button();
        cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new System.Drawing.Size(75, 23);
        cancelButton.Text = "&Cancel";
        cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
        inputBox.Controls.Add(cancelButton);

        inputBox.AcceptButton = okButton;
        inputBox.CancelButton = cancelButton; 

        DialogResult result = inputBox.ShowDialog();
        input = textBox.Text;
        return result;
    }		
    }
	
}