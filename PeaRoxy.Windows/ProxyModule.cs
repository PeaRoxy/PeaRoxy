// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyModule.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The windows proxy module.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows
{
    #region

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;

    using Microsoft.Win32;

    #endregion

    /// <summary>
    /// The windows proxy module.
    /// </summary>
    public static class ProxyModule
    {
        #region Constants

        /// <summary>
        /// The interne t_ ope n_ typ e_ direct.
        /// </summary>
        private const int InternetOpenTypeDirect = 1;

        /// <summary>
        /// The interne t_ ope n_ typ e_ proxy.
        /// </summary>
        private const int InternetOpenTypeProxy = 3;

        /// <summary>
        /// The interne t_ optio n_ proxy.
        /// </summary>
        private const int InternetOptionProxy = 38;

        /// <summary>
        /// The interne t_ optio n_ prox y_ setting s_ changed.
        /// </summary>
        private const int InternetOptionProxySettingsChanged = 95;

        /// <summary>
        /// The interne t_ optio n_ refresh.
        /// </summary>
        private const int InternetOptionRefresh = 37;

        /// <summary>
        /// The interne t_ optio n_ setting s_ changed.
        /// </summary>
        private const int InternetOptionSettingsChanged = 39;

        #endregion

        #region Public Methods and Operators

        public static bool ForceFirefoxToUseSystemSettings()
        {
            if (!IsFirefoxNeedReconfig())
            {
                return true;
            }

            bool isError = false;
            try
            {
                if (Process.GetProcessesByName("firefox").Any())
                {
                    return false;
                }

                string firefoxProfilesPath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                        "Mozilla\\Firefox\\Profiles");
                if (!Directory.Exists(firefoxProfilesPath))
                {
                    return true;
                }

                foreach (string profileAddress in Directory.GetDirectories(firefoxProfilesPath))
                {
                    try
                    {
                        if (File.Exists(Path.Combine(profileAddress, "prefs.js")))
                        {
                            string fileKeeper = string.Empty;
                            FileStream fs = File.Open(
                                Path.Combine(profileAddress, "prefs.js"), 
                                FileMode.Open, 
                                FileAccess.ReadWrite, 
                                FileShare.None);
                            StreamReader sr = new StreamReader(fs);
                            while (!sr.EndOfStream)
                            {
                                string data = sr.ReadLine();
                                if (data != null && !data.ToLower().Contains("\"network.proxy.type\""))
                                {
                                    fileKeeper += data + Environment.NewLine;
                                }
                            }

                            fs.Position = 0;
                            fs.SetLength(0);
                            StreamWriter sw = new StreamWriter(fs);
                            sw.Write(fileKeeper);
                            sw.Flush();
                            sw.Close();
                            sr.Close();
                            fs.Close();
                        }
                    }
                    catch (Exception)
                    {
                        isError = true;
                    }
                }

                return !isError;
            }
            catch (Exception)
            {
            }

            return false;
        }

        private static bool IsFirefoxNeedReconfig()
        {
            try
            {
                string firefoxProfilesPath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                        "Mozilla\\Firefox\\Profiles");
                if (!Directory.Exists(firefoxProfilesPath))
                {
                    return true;
                }

                foreach (string profileAddress in Directory.GetDirectories(firefoxProfilesPath))
                {
                    if (File.Exists(Path.Combine(profileAddress, "prefs.js")))
                    {
                        FileStream fs = File.Open(
                            Path.Combine(profileAddress, "prefs.js"), 
                            FileMode.Open, 
                            FileAccess.Read, 
                            FileShare.ReadWrite);
                        StreamReader sr = new StreamReader(fs);
                        while (!sr.EndOfStream)
                        {
                            string readLine = sr.ReadLine();
                            if (readLine != null && readLine.ToLower().Contains("\"network.proxy.type\""))
                            {
                                sr.Close();
                                fs.Close();
                                return true;
                            }
                        }

                        sr.Close();
                        fs.Close();
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        private static void RefreshProxySettings()
        {
            InternetSetOption(IntPtr.Zero, InternetOptionProxySettingsChanged, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);
            Thread.Sleep(1000);
        }

        public static bool DisableProxy()
        {
            try
            {
                if (!IsProxyEnable())
                {
                    //RefreshProxySettings();
                    return true;
                }

                StructInternetProxyInfo proxyInfo;
                proxyInfo.DwAccessType = InternetOpenTypeDirect;
                proxyInfo.Proxy = IntPtr.Zero;
                proxyInfo.ProxyBypass = IntPtr.Zero;
                IntPtr intptrStruct = Marshal.AllocCoTaskMem(Marshal.SizeOf(proxyInfo));
                Marshal.StructureToPtr(proxyInfo, intptrStruct, true);

                if (InternetSetOption(IntPtr.Zero, InternetOptionProxy, intptrStruct, Marshal.SizeOf(proxyInfo)))
                {
                    RefreshProxySettings();
                    if (!IsProxyEnable())
                    {
                        return true;
                    }
                }

                RegistryKey registry =
                    Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", 
                        true);
                if (registry != null)
                {
                    registry.SetValue("ProxyEnable", 0);
                    registry.SetValue("ProxyServer", string.Empty);
                    registry.Flush();
                    registry.Close();
                }
                RefreshProxySettings();
                if (!IsProxyEnable())
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static bool DisableProxyAutoConfig()
        {
            try
            {
                if (!IsProxyAutoConfigEnable())
                {
                    //RefreshProxySettings();
                    return true;
                }

                RegistryKey registry = Registry.CurrentUser.OpenSubKey(
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", 
                    true);
                if (registry != null)
                {
                    registry.DeleteValue("AutoConfigURL", false);
                    registry.Flush();
                    registry.Close();
                }
                RefreshProxySettings();
                if (!IsProxyAutoConfigEnable())
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static bool IsProxyAutoConfigEnable(string address = "")
        {
            try
            {
                RegistryKey registry =
                    Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", 
                        false);
                using (registry)
                {
                    if (registry != null
                        && ((string.IsNullOrWhiteSpace(address)
                             && !string.IsNullOrWhiteSpace((string)registry.GetValue("AutoConfigURL", string.Empty)))
                            || (!string.IsNullOrWhiteSpace(address) && registry.GetValue("AutoConfigURL", string.Empty).Equals(address))))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        private static bool IsProxyEnable(string proxyString = "")
        {
            try
            {
                RegistryKey registry =
                    Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", 
                        false);
                using (registry)
                {
                    if (registry != null && (int)registry.GetValue("ProxyEnable", 0) != 1)
                    {
                        return false;
                    }

                    if (registry != null && (string)registry.GetValue("ProxyServer", string.Empty) == proxyString)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static bool SetActiveProxy(IPEndPoint gateway)
        {
            try
            {
                string proxyString = string.Format("{0}:{1}", gateway.Address, gateway.Port);
                proxyString = string.Format("http={0};" + "https={0};" + "socks={0};", proxyString);
                if (IsProxyEnable(proxyString))
                {
                    RefreshProxySettings();
                    return true;
                }

                StructInternetProxyInfo proxyInfo;
                proxyInfo.DwAccessType = InternetOpenTypeProxy;
                proxyInfo.Proxy = Marshal.StringToHGlobalAnsi(proxyString);
                proxyInfo.ProxyBypass = Marshal.StringToHGlobalAnsi("<local>");
                IntPtr intptrStruct = Marshal.AllocCoTaskMem(Marshal.SizeOf(proxyInfo));
                Marshal.StructureToPtr(proxyInfo, intptrStruct, true);

                if (InternetSetOption(IntPtr.Zero, InternetOptionProxy, intptrStruct, Marshal.SizeOf(proxyInfo)))
                {
                    InternetSetOption(IntPtr.Zero, InternetOptionProxySettingsChanged, IntPtr.Zero, 0);
                    InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
                    InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);
                    Thread.Sleep(1000);
                    if (IsProxyEnable(proxyString))
                    {
                        return true;
                    }
                }

                RegistryKey registry =
                    Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", 
                        true);
                if (registry != null)
                {
                    registry.SetValue("ProxyEnable", 1);
                    registry.SetValue("ProxyServer", proxyString);
                    registry.Flush();
                    registry.Close();
                }
                RefreshProxySettings();
                if (IsProxyEnable(proxyString))
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static bool SetActiveAutoConfig(string address)
        {
            try
            {
                if (address == string.Empty)
                {
                    return DisableProxyAutoConfig();
                }

                if (IsProxyAutoConfigEnable(address))
                {
                    RefreshProxySettings();
                    return true;
                }

                RegistryKey registry = Registry.CurrentUser.OpenSubKey(
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", 
                    true);
                if (registry != null)
                {
                    registry.SetValue("AutoConfigURL", address);
                    registry.Flush();
                    registry.Close();
                }
                RefreshProxySettings();
                if (IsProxyAutoConfigEnable(address))
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        #endregion

        #region Methods

        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(
            IntPtr hInternet, 
            int dwOption, 
            IntPtr lpBuffer, 
            int dwBufferLength);

        #endregion

        private struct StructInternetProxyInfo
        {
            #region Fields

            /// <summary>
            /// The dw access type.
            /// </summary>
            // ReSharper disable once NotAccessedField.Local
            public int DwAccessType;

            /// <summary>
            /// The proxy.
            /// </summary>
            // ReSharper disable once NotAccessedField.Local
            public IntPtr Proxy;

            /// <summary>
            /// The proxy bypass.
            /// </summary>
            // ReSharper disable once NotAccessedField.Local
            public IntPtr ProxyBypass;

            #endregion
        };
    }
}