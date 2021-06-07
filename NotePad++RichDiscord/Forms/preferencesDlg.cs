using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Kbg.NppPluginNET
{
    public partial class preferencesDlg : Form
    {
        public preferencesDlg()
        {
            InitializeComponent();
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            var iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath))
                Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, "Discord Rich Presence" + ".ini");
            if ((Win32.GetPrivateProfileInt("settings", "autostart", 0, iniFilePath) != 0))
                checkBox1.Checked = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            var iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath))
                Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, "Discord Rich Presence" + ".ini");
            if (checkBox1.Checked)
                Win32.WritePrivateProfileString("settings", "autostart", "1", iniFilePath);
            else
                Win32.WritePrivateProfileString("settings", "autostart", "0", iniFilePath);
        }
    }
}
