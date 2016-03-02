//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 23.02.2016
// Autor Guenther
// 
// Empty template to fill for synchronous script.
//-----------------------------------------------------------------------------------


// Comment	Description	Footprint	LibRef	Quantity	Designator
// 1uF	0603 1 uF 16 V ±10% Tolerance X5R SMT Multilayer Ceramic Capacitor	PCB-9voc856bm8rdirtpr58d-1	CMP-19299708-5	1	C1

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
using System.Diagnostics;

namespace PCBIScript
{
    public class PScript : IPCBIScript
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
        private int attribCount = 0;
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.pcb-investigator.com/");
        }
        private ICMPObject GetComponent(string RefName)
        {
            foreach (ICMPObject cmp in PCBI_Parent.GetCurrentStep().GetAllCMPObjects())
            {
                if (cmp.Ref == RefName)
                {
                    attribCount++;
                    return cmp;
                }
            }
            return null;
        }
        private string CreateNewRefList(string oldList)
        {
            string[] desList = oldList.Split(',');
            string newList = "";
            foreach (string part in desList)
            {
                if (part.Contains("-"))
                {
                    string startDigit = "";
                    foreach (char c in part)
                    {
                        if (Char.IsDigit(c))
                        {
                            startDigit = c.ToString();
                            break;
                        }
                    }
                    string RefName = part.Substring(0, part.IndexOf(startDigit));
                    string startNum = part.Substring(part.IndexOf(startDigit), part.IndexOf("-") - part.IndexOf(startDigit));
                    string endNum = part.Substring(part.IndexOf("-") + 1);
                    int startIndex = int.Parse(startNum);
                    int endIndex = int.Parse(endNum) + 1;
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        newList += RefName + i.ToString() + ",";
                    }
                }
                else
                {
                    newList += part + ",";
                }
            }
            return newList;
        }
        private void buttonAddToReferences_Click(object sender, EventArgs e)
        {
            System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = ",";
            // find designator colum
            int columDesignator = 0;
            attribCount = 0;
            //         for (int colDes = 0; colDes < myDataGridView.Rows[0].Cells.Count; colDes++)
            //         {
            //             if (myDataGridView.Columns[colDes].HeaderText.ToLower().Trim() == "designator")
            //             {
            //                 columDesignator = colDes;
            //                 break;
            //             }
            //         }
            columDesignator = comboBoxREfColum.SelectedIndex;
            for (int rows = 0; rows < myDataGridView.Rows.Count - 1; rows++)
            {
                myDataGridView.Rows[rows].Cells[columDesignator].Style.BackColor = Color.Red;
                string Designators = myDataGridView.Rows[rows].Cells[columDesignator].Value.ToString();
                Designators = CreateNewRefList(Designators);
                string[] desList = Designators.Split(',');
                foreach (string CompDesignator in desList)
                {
                    ICMPObject cmp = GetComponent(CompDesignator.Trim());
                    if (cmp == null) continue;
                    for (int col = 0; col < myDataGridView.Rows[rows].Cells.Count; col++)
                    {
                        if (col == columDesignator) continue;
                        if (myDataGridView.Columns[col].HeaderText.ToLower() == "height")
                        {
                            IComponentSpecifics cs = (IComponentSpecifics)cmp.GetSpecifics();
                            cs.Height = float.Parse(myDataGridView.Rows[rows].Cells[col].Value.ToString().Replace(",", "."), nfi);
                            cmp.SetSpecifics(cs);
                        } else
                        {
                            cmp.AddComponentAttribute(myDataGridView.Columns[col].HeaderText.ToLower(), myDataGridView.Rows[rows].Cells[col].Value.ToString(), false);
                            myDataGridView.Rows[rows].Cells[columDesignator].Style.BackColor = Color.GreenYellow;
                        }
                    }

                }
            }
            if (attribCount > 0)
            {
                MessageBox.Show(attribCount.ToString() + " : Components changed, " + PCBI_Parent.GetCurrentStep().GetAllCMPObjects().Count.ToString() + " Components in design", "Finished");
            }
            else
            {
                MessageBox.Show("No attributes added", "Finished without success");
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
            comboBoxREfColum.Items.Clear();
            for (int c = 0; c < myDataGridView.Columns.Count; c++)
            {
                comboBoxREfColum.Items.Add(myDataGridView.Columns[c].HeaderText.ToLower());
            }
            for (int i = 0; i < comboBoxREfColum.Items.Count; i++)
            {
                if (comboBoxREfColum.Items[i].ToString().ToLower().Trim() == "designator" || comboBoxREfColum.Items[i].ToString().ToLower().Trim() == "refdes")
                {
                    comboBoxREfColum.SelectedIndex = i;
                    break;
                }
            }
            buttonAddToReferences.Enabled = true;
        }
        ////////////////////////////////////////////////////////////////
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
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
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.myDataGridView = new System.Windows.Forms.DataGridView();
            this.buttonPaste = new System.Windows.Forms.Button();
            this.buttonAddToReferences = new System.Windows.Forms.Button();
            this.comboBoxREfColum = new System.Windows.Forms.ComboBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.myDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
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
            this.buttonAddToReferences.Text = "paste Excel selection";
            this.buttonAddToReferences.UseVisualStyleBackColor = true;
            // 
            // buttonAddToReferences
            // 
            this.buttonAddToReferences.Enabled = false;
            this.buttonAddToReferences.Location = new System.Drawing.Point(474, 12);
            this.buttonAddToReferences.Name = "buttonAddToReferences";
            this.buttonAddToReferences.Size = new System.Drawing.Size(126, 21);
            this.buttonAddToReferences.TabIndex = 2;
            this.buttonAddToReferences.Text = "Add Attributes to PCB";
            this.buttonAddToReferences.UseVisualStyleBackColor = true;
            this.buttonAddToReferences.Click += new System.EventHandler(this.buttonAddToReferences_Click);
            // 
            // comboBoxREfColum
            // 
            this.comboBoxREfColum.FormattingEnabled = true;
            this.comboBoxREfColum.Location = new System.Drawing.Point(270, 12);
            this.comboBoxREfColum.Name = "comboBoxREfColum";
            this.comboBoxREfColum.Size = new System.Drawing.Size(198, 21);
            this.comboBoxREfColum.TabIndex = 3;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(175, 16);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(56, 13);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Select Ref Colum";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // Column1
            // 
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.Column1.DefaultCellStyle = dataGridViewCellStyle1;
            this.Column1.HeaderText = "Please use \"Paste Excel selection\" to fill in data";
            this.Column1.Name = "Column1";
            this.Column1.Width = 1000;
            // 
            // ExcelToPCBI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 471);
            this.Controls.Add(this.buttonAddToReferences);
            this.Controls.Add(this.buttonPaste);
            this.Controls.Add(this.myDataGridView);
            this.Controls.Add(this.comboBoxREfColum);
            this.Controls.Add(this.linkLabel1);
            this.Name = "ExcelToPCBI";
            this.Text = "Add Attributes to Components via Reference Designator";
            ((System.ComponentModel.ISupportInitialize)(this.myDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView myDataGridView;
        private System.Windows.Forms.Button buttonPaste;
        private System.Windows.Forms.Button buttonAddToReferences;
        private System.Windows.Forms.ComboBox comboBoxREfColum;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
    }
}