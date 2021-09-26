using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Kbg.NppPluginNET
{
    public partial class preferencesDlg : Form
    {
        private string path;
        private void getINIPath()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            var iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath))
                Directory.CreateDirectory(iniFilePath);
            path = Path.Combine(iniFilePath, "Discord Rich Presence" + ".ini");

        }

        public preferencesDlg()
        {
            InitializeComponent();
            getINIPath();

            StringBuilder sbFieldValue = new StringBuilder(32767);
            Win32.GetPrivateProfileString("settings", "autostart", "", sbFieldValue, 32767, path);
            if (int.Parse(sbFieldValue.ToString()) == 1)
                checkBox1.Checked = true;

            Win32.GetPrivateProfileString("settings", "excludelist", "", sbFieldValue, 32767, path);
            if (sbFieldValue.Length > 0)
            {           
                var excludeList_tmp = sbFieldValue.ToString().Split('|');
                var excludeList = excludeList_tmp.Where(x => !string.IsNullOrEmpty(x)).ToArray(); ;
                listBox1.DataSource = excludeList;
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                Win32.WritePrivateProfileString("settings", "autostart", "1", path);
            else
                Win32.WritePrivateProfileString("settings", "autostart", "0", path);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder sbFieldValue = new StringBuilder(32767);
            Win32.GetPrivateProfileString("settings", "excludelist", "", sbFieldValue, 32767, path);   
            sbFieldValue.Append("|" + textBox1.Text);
            listBox1.Items.Add(textBox1.Text);
            textBox1.Text = "";
            Win32.WritePrivateProfileString("settings", "excludelist", sbFieldValue.ToString(), path);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder sbFieldValue = new StringBuilder(32767);
            var s = listBox1.GetItemText(listBox1.SelectedItem);
            foreach (var x in listBox1.Items)
            {            
                if (x.ToString() != s)
                    sbFieldValue.Append("|" + x.ToString());
            }
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            Win32.WritePrivateProfileString("settings", "excludelist", sbFieldValue.ToString(), path);
        }
    }
}
