using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DiscordRPC;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace Kbg.NppPluginNET
{
    class Main
    {
        internal const string PluginName = "Discord Rich Presence";
        static string iniFilePath = null;
        static Thread thread;
        static preferencesDlg prfsDlg = null;
        static DiscordRpcClient client = null;
        static int idMyDlg = -1;
        static bool stopRefresh = false, isStarted = false, autoStart = false;
        static Icon tbIcon = null;
        static IScintillaGateway editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
        static INotepadPPGateway notepad = new NotepadPPGateway();

        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++
            // use eg. as
            // if (notification.Header.Code == (uint)NppMsg.NPPN_xxx)
            // { ... }
            // or
            //
            // if (notification.Header.Code == (uint)SciMsg.SCNxxx)
            // { ... }
        }

        public static void refreshCheckbox()
        {
            if (autoStart)
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idMyDlg]._cmdID, 1);
            else
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idMyDlg]._cmdID, 0);
        }

        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath))
                Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");
            autoStart = (Win32.GetPrivateProfileInt("settings", "autostart", 0, iniFilePath) != 0);

            PluginBase.SetCommand(0, "Start Rich Presence Discord", startFunction, new ShortcutKey(false, false, false, Keys.None)); idMyDlg = 0;
            PluginBase.SetCommand(1, "Preferences", myDockableDialog);
            if (autoStart)
                executePresence();
        }

        internal static void PluginCleanUp()
        {
            if (client != null && !client.IsDisposed)
            {
                thread.Abort();
                client.ClearPresence();
                client.Dispose();
            }
        }

        static void callbackStartDiscordRichPresence(object data)
        {
            client = new DiscordRpcClient("529306098646122516");           
            client.Initialize();

            Thread.Sleep(2000);
            StringBuilder sbFieldValue = new StringBuilder(32767);
            Win32.GetPrivateProfileString("settings", "excludelist", "", sbFieldValue, 32767, iniFilePath);
            var excludeList_tmp = sbFieldValue.ToString().Split('|');
            var excludeList = excludeList_tmp.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            
            var fileName = getCurrentPath(NppMsg.FILE_NAME);
            var actualFile = "";
            if (excludeList.Contains(fileName))
                fileName = "Secret File";

            client.SetPresence(new RichPresence()
            {
                Details = "Working on file:",
                State = fileName,
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "Notepad++",
                    SmallImageKey = "image_small"
                }
            });

            while (true)
            {
                Thread.Sleep(800);
                if (stopRefresh)
                {
                    client.Dispose();
                    stopRefresh = false;
                    break;
                }

                fileName = getCurrentPath(NppMsg.FILE_NAME);
  
                Win32.GetPrivateProfileString("settings", "excludelist", "", sbFieldValue, 32767, iniFilePath);
                excludeList_tmp = sbFieldValue.ToString().Split('|');
                excludeList = excludeList_tmp.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (excludeList.Contains(fileName))
                {
                    if (actualFile != fileName)
                        client.UpdateState("Secret File");
                    actualFile = fileName;
                    continue;
                }


                if (actualFile != fileName)
                {
                    actualFile = fileName;
                    client.UpdateState(fileName);
                }
            }
        }

        static string getCurrentPath(NppMsg which)
        {
            NppMsg msg = NppMsg.NPPM_GETFULLCURRENTPATH;
            if (which == NppMsg.FILE_NAME)
                msg = NppMsg.NPPM_GETFILENAME;
            else if (which == NppMsg.CURRENT_DIRECTORY)
                msg = NppMsg.NPPM_GETCURRENTDIRECTORY;

            StringBuilder path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)msg, 0, path);

            return path.ToString();
        }

        internal static void executePresence()
        {
            if (!isStarted)
            {
                thread = new Thread(new ParameterizedThreadStart(callbackStartDiscordRichPresence));
                thread.Start(null);
                isStarted = true;
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idMyDlg]._cmdID, 1);
            }
            else
            {
                stopRefresh = true;
                isStarted = false;
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idMyDlg]._cmdID, 0);
            }
            
        }

        internal static void startFunction()
        {
            executePresence();
        }

        internal static void myDockableDialog()
        {
            if (prfsDlg == null)
            {
                prfsDlg = new preferencesDlg();

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = prfsDlg.Handle;
                _nppTbData.pszName = "Discord Settings";
                _nppTbData.dlgID = idMyDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                //_nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMSHOW, 0, prfsDlg.Handle);
            }
        }
    }
}