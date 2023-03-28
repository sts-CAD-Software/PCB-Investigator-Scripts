//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 23.02.2016
// Autor support@easylogix.de
// www.pcb-investigator.com
// SDK online reference http://www.pcb-investigator.com/sites/default/files/documents/InterfaceDocumentation/Index.html
// SDK http://www.pcb-investigator.com/en/sdk-participate
// 
// Script to import attribute for components from clipboart (e.g. MS excel).
//-----------------------------------------------------------------------------------
// The script allows the user to import attributes for parts from the clipboard (e.g. from MS Excel).
// It consists of a class that opens a dialog window with a button for inserting data from the clipboard and a button for adding attributes to the parts.
// The script should be used when the user wants to import attributes for multiple parts quickly and easily from another source, such as an Excel spreadsheet. 
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PCBIScript
{
    public class PScript  : IPCBIScript
    {
        public PScript()
        {
        }

        public void Execute(IPCBIWindow parent)
        {
            //your code here
            ExcelToPCBI excel = new ExcelToPCBI(parent);
            excel.ShowDialog();
            parent.UpdateView();
        }

    }
    ///// subclasses
    public class ExcelToPCBI : Form
    {
        public ExcelToPCBI(IPCBIWindow parent)
        {
            InitializeComponent();
            PCBI_Parent = parent;
        }
        public IPCBIWindow PCBI_Parent;

        private ICMPObject GetComponent(string RefName)
        {
            foreach (ICMPObject cmp in PCBI_Parent.GetCurrentStep().GetAllCMPObjects())
            {
                if (cmp.Ref == RefName)
                    return cmp;
            }
            return null;
        }
        private void buttonAddToReferences_Click(object sender, EventArgs e)
        {
            if (PCBI_Parent == null) return;

            for (int rows = 0; rows < myDataGridView.Rows.Count - 1; rows++)
            {
                ICMPObject cmp = GetComponent(myDataGridView.Rows[rows].Cells[0].Value.ToString());
                if (cmp == null) continue;

                for (int col = 1; col < myDataGridView.Rows[rows].Cells.Count; col++)
                {
                    //myDataGridView.Rows[0].Cells[col].Value.ToString()
                    cmp.AddComponentAttribute(myDataGridView.Columns[col].HeaderText.ToString(), myDataGridView.Rows[rows].Cells[col].Value.ToString(), false);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DataObject o = (DataObject)Clipboard.GetDataObject();

            if (o.GetDataPresent(DataFormats.Text))
            {
                if (myDataGridView.Rows.Count > 0)
                    myDataGridView.Rows.Clear();
                if (myDataGridView.Columns.Count > 0)
                    myDataGridView.Columns.Clear();

                bool columnsAdded = false;
                string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
                foreach (string pastedRow in pastedRows)
                {
                    string[] pastedRowCells = pastedRow.Split(new char[] { '\t' });

                    if (!columnsAdded)
                    {
                        for (int i = 0; i < pastedRowCells.Length; i++)
                            myDataGridView.Columns.Add("col" + i, pastedRowCells[i]);

                        columnsAdded = true;
                        continue;
                    }

                    myDataGridView.Rows.Add(pastedRowCells);
                }
            }

        }

        ////////////////////////////////////////////////////////////////
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gel?scht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode f?r die Designerunterst?tzung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor ge?ndert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.myDataGridView = new System.Windows.Forms.DataGridView();
            this.buttonPaste = new System.Windows.Forms.Button();
            this.buttonAddToReferences = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.myDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // myDataGridView
            // 
            this.myDataGridView.AllowDrop = true;
            this.myDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.myDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.myDataGridView.Location = new System.Drawing.Point(3, 39);
            this.myDataGridView.Name = "myDataGridView";
            this.myDataGridView.Size = new System.Drawing.Size(615, 432);
            this.myDataGridView.TabIndex = 0;
            // 
            // buttonPaste
            // 
            this.buttonPaste.Location = new System.Drawing.Point(21, 12);
            this.buttonPaste.Name = "buttonPaste";
            this.buttonPaste.Size = new System.Drawing.Size(126, 21);
            this.buttonPaste.TabIndex = 1;
            this.buttonPaste.Text = "paste Excel selection";
            this.buttonPaste.UseVisualStyleBackColor = true;
            this.buttonPaste.Click += new System.EventHandler(this.button1_Click);

            // 
            // buttonAddToReferences
            // 
            this.buttonAddToReferences.Location = new System.Drawing.Point(474, 12);
            this.buttonAddToReferences.Name = "buttonAddToReferences";
            this.buttonAddToReferences.Size = new System.Drawing.Size(126, 21);
            this.buttonAddToReferences.TabIndex = 2;
            this.buttonAddToReferences.Text = "Set Attributes";
            this.buttonAddToReferences.UseVisualStyleBackColor = true;
            this.buttonAddToReferences.Click += new System.EventHandler(this.buttonAddToReferences_Click);
            // 
            // ExcelToPCBI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 471);
            this.Controls.Add(this.buttonAddToReferences);
            this.Controls.Add(this.buttonPaste);
            this.Controls.Add(this.myDataGridView);
            this.Name = "ExcelToPCBI";
            this.Text = "Set Attributes";
            ((System.ComponentModel.ISupportInitialize)(this.myDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView myDataGridView;
        private System.Windows.Forms.Button buttonPaste;
        private System.Windows.Forms.Button buttonAddToReferences;
    }
}