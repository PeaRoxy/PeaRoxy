// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsProxy.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The windows proxy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows
{
    #region

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    using Microsoft.Win32;

    #endregion

    /// <summary>
    /// The windows proxy.
    /// </summary>
    public static class WindowsProxy
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

        /// <summary>
        /// The force firefox to use system settings.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ForceFirefoxToUseSystemSettings()
        {
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

        /// <summary>
        /// The is firefox need reconfig.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsFirefoxNeedReconfig()
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
                            if (readLine == null || !readLine.ToLower().Contains("\"network.proxy.type\""))
                            {
                                continue;
                            }
                            sr.Close();
                            fs.Close();
                            return true;
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

        /// <summary>
        /// The refresh proxy settings.
        /// </summary>
        /// <param name="rV">
        /// The r v.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool RefreshProxySettings(bool rV)
        {
            RefreshProxySettings();
            return rV;
        }

        /// <summary>
        /// The refresh proxy settings.
        /// </summary>
        public static void RefreshProxySettings()
        {
            InternetSetOption(IntPtr.Zero, InternetOptionProxySettingsChanged, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);
            Thread.Sleep(1000);
        }

        /// <summary>
        /// The windows_ disable proxy.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool WindowsDisableProxy()
        {
            try
            {
                if (!WindowsIsProxyEnable())
                {
                    return RefreshProxySettings(true);
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
                    if (!WindowsIsProxyEnable())
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
                if (!WindowsIsProxyEnable())
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// The windows_ disable proxy auto config.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool WindowsDisableProxyAutoConfig()
        {
            try
            {
                if (!WindowsIsProxyAutoConfigEnable())
                {
                    return RefreshProxySettings(true);
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
                if (!WindowsIsProxyAutoConfigEnable())
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// The windows_ is proxy auto config enable.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool WindowsIsProxyAutoConfigEnable(string address = "")
        {
            try
            {
                RegistryKey registry =
                    Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", 
                        false);
                using (registry)
                {
                    if (address == string.Empty)
                    {
                        return false;
                    }

                    if (registry != null && registry.GetValue("AutoConfigURL", string.Empty).Equals(address))
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

        /// <summary>
        /// The windows_ is proxy enable.
        /// </summary>
        /// <param name="proxyString">
        /// The proxy string.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool WindowsIsProxyEnable(string proxyString = "")
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

                    // if (proxyString == string.Empty)
                    // return false;
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

        /// <summary>
        /// The windows_ set active proxy.
        /// </summary>
        /// <param name="ip">
        /// The ip.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="https">
        /// The https.
        /// </param>
        /// <param name="http">
        /// The http.
        /// </param>
        /// <param name="socks">
        /// The socks.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool WindowsSetActiveProxy(
            string ip, 
            int port, 
            bool https = false, 
            bool http = false, 
            bool socks = false)
        {
            try
            {
                if (!http && !https && !socks)
                {
                    return WindowsDisableProxy();
                }

                string proxyString = ip + ":" + port;
                proxyString = (http ? "http=" + proxyString + ";" : string.Empty)
                              + (https ? "https=" + proxyString + ";" : string.Empty)
                              + (socks ? "socks=" + proxyString + ";" : string.Empty);
                if (WindowsIsProxyEnable(proxyString))
                {
                    return RefreshProxySettings(true);
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
                    if (WindowsIsProxyEnable(proxyString))
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
                if (WindowsIsProxyEnable(proxyString))
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// The windows_ set active proxy auto config.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool WindowsSetActiveProxyAutoConfig(string address)
        {
            try
            {
                if (address == string.Empty)
                {
                    return WindowsDisableProxyAutoConfig();
                }

                if (WindowsIsProxyAutoConfigEnable(address))
                {
                    return RefreshProxySettings(true);
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
                if (WindowsIsProxyAutoConfigEnable(address))
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

        /// <summary>
        /// The internet set option.
        /// </summary>
        /// <param name="hInternet">
        /// The h internet.
        /// </param>
        /// <param name="dwOption">
        /// The dw option.
        /// </param>
        /// <param name="lpBuffer">
        /// The lp buffer.
        /// </param>
        /// <param name="dwBufferLength">
        /// The dw buffer length.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(
            IntPtr hInternet, 
            int dwOption, 
            IntPtr lpBuffer, 
            int dwBufferLength);

        #endregion

        /// <summary>
        /// The internet proxy info structure
        /// </summary>
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