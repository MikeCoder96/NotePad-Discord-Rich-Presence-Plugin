// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Kbg.NppPluginNET.PluginInfrastructure;
using NppPlugin.DllExport;

namespace Kbg.NppPluginNET
{
    class UnmanagedExports
    {
        /// <summary>
        /// This is necessary to load required library
        /// </summary>
        private static string[] LOAD_ASSEMBLIES = { "DiscordRPC.dll", "Newtonsoft.Json.dll" };

        public static void initializeAssembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
            {
                string assemblyFile = (args.Name.Contains(','))
                    ? args.Name.Substring(0, args.Name.IndexOf(','))
                    : args.Name;

                assemblyFile += ".dll";

                // Forbid non handled dll's
                if (!LOAD_ASSEMBLIES.Contains(assemblyFile))
                {
                    return null;
                }

                string absoluteFolder = new FileInfo((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).Directory.FullName + "\\Lib";
                string targetPath = Path.Combine(absoluteFolder, assemblyFile);

                try
                {
                    return Assembly.LoadFile(targetPath);
                }
                catch (Exception)
                {
                    return null;
                }
            };
        }

        [DllExport(CallingConvention=CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            initializeAssembly();
            PluginBase.nppData = notepadPlusData;
            Main.CommandMenuInit();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            nbF = PluginBase._funcItems.Items.Count;
            return PluginBase._funcItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        static IntPtr _ptrPluginName = IntPtr.Zero;
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Main.PluginName);
            return _ptrPluginName;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            ScNotification notification = (ScNotification)Marshal.PtrToStructure(notifyCode, typeof(ScNotification));
            if (notification.Header.Code == (uint)NppMsg.NPPN_TBMODIFICATION)
            {
                PluginBase._funcItems.RefreshItems();
                Main.SetToolBarIcon();
            }
            else if (notification.Header.Code == (uint)NppMsg.NPPN_SHUTDOWN)
            {
                Main.PluginCleanUp();
                Marshal.FreeHGlobal(_ptrPluginName);
            }
            else
            {
	            Main.OnNotification(notification);
            }
        }
    }
}
