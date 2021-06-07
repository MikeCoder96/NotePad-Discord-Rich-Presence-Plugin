using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        static Bitmap tbBmp = NotePad__RichDiscord.Properties.Resources.star;
        static Bitmap tbBmp_tbTab = NotePad__RichDiscord.Properties.Resources.star_bmp;
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

        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
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

        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idMyDlg]._cmdID, isStarted ? 1 : 0);

        }

        internal static void PluginCleanUp()
        {
            if (client != null && !client.IsDisposed)
            {
                stopRefresh = true;
                thread.Abort();
                client.Dispose();
            }
        }

        static void callbackWhatIsNpp(object data)
        {
            client = new DiscordRpcClient("529306098646122516");

            client.Initialize();
            while (true)
            {
                Thread.Sleep(2000);
                if (stopRefresh)
                {
                    client.Dispose();
                    stopRefresh = false;
                    break;
                }    
                var fileName = getCurrentPath(NppMsg.FILE_NAME);
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
                thread = new Thread(new ParameterizedThreadStart(callbackWhatIsNpp));
                thread.Start(null);
                isStarted = true;
            }
            else
            {
                stopRefresh = true;
            }
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idMyDlg]._cmdID, isStarted ? 1 : 0);
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

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = prfsDlg.Handle;
                _nppTbData.pszName = "Discord Settings";
                _nppTbData.dlgID = idMyDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMSHOW, 0, prfsDlg.Handle);
            }
        }
    }
}