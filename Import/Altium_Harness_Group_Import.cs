//Synchronous template
//-----------------------------------------------------------------------------------
// PCB-Investigator Automation Script
// Created on 04.11.2016
// Autor Guenther
// 
// Empty template to fill for synchronous script.
//-----------------------------------------------------------------------------------
// GUID newScript_636138465480261016

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
// example line to import dll DLLImport C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\searchedDLL.dll;
//this line includes a custom dll, e.g. your own dll or other microsoft dlls

namespace PCBIScript
{
    public class PScript : IPCBIScript
    {
        public PScript()
        {
        }
        IPCBIWindow parent;
        public void Execute(IPCBIWindow parent)
        {
            //your code here
            this.parent = parent;
            AddGroup();
            parent.UpdateView();
        }
        private void AddGroup()
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.SelectedPath = Environment.CurrentDirectory;
            if (fb.ShowDialog() == DialogResult.OK)
            {
                string FilePath = fb.SelectedPath;
                DirectoryInfo di = new DirectoryInfo(FilePath);
                groupsPerNet.Clear();
                foreach (FileInfo fi in di.GetFiles())
                {
                    if (fi.Extension.ToLowerInvariant() == ".harness")
                    {
                        AddNetGroup(fi.FullName);
                    }
                }
                IStep step = parent.GetCurrentStep();
                if (step != null)
                {
                    List<INet> nets = step.GetNets();
                    if (nets != null)
                    {
                        foreach (INet net in nets)
                        {
                            string group = null;
                            if (groupsPerNet.ContainsKey(net.NetName)) group = groupsPerNet[net.NetName].Key;

                            if (string.IsNullOrWhiteSpace(group))
                            {
                                IAttribute.RemoveAttribute(PCBI.FeatureAttributeEnum.net_type, net);
                                IAttribute.RemoveAttribute("NET_DESCRIPTION", net, parent);
                            }
                            else
                            {
                                IAttributeElement attr = new IAttributeElement(PCBI.FeatureAttributeEnum.net_type);
                                attr.Value = group;
                                IAttribute.SetAttribute(attr, net);
                                IAttributeElement netDesAttr = IAttributeElement.CreateUserAttribute("NET_DESCRIPTION", groupsPerNet[net.NetName].Value, PCBI.ODBEntityEnum.Net, parent);
                                IAttribute.SetAttribute(netDesAttr, net);
                            }
                        }
                    }

                }
            }
        }
        private Dictionary<string, KeyValuePair<string, string>> groupsPerNet = new Dictionary<string, KeyValuePair<string, string>>();
        private void AddNetGroup(string FileName)
        {
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                using (fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                {
                    using (sr = new StreamReader(fs, System.Text.Encoding.Default))
                    {
                        string line = "";
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] lineParts = line.Split('=');
                            if (lineParts.Length >= 2)
                            {
                                string[] NetNames = lineParts[1].Split(',');
                                if (NetNames.Length >= 1)
                                {
                                    foreach (string NetString in NetNames)
                                    {
                                        //INet net = step.GetNet(NetString);
                                        if (!groupsPerNet.ContainsKey(NetString))
                                        {
                                            groupsPerNet.Add(NetString, new KeyValuePair<string, string>(lineParts[0], Path.GetFileNameWithoutExtension(FileName)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (sr != null) sr.Dispose();
                if (fs != null) fs.Dispose();
            }
        }
    }
}