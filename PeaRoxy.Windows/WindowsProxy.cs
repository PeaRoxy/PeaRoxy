using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace PeaRoxy.Windows.WPFClient
{
    public class WindowsProxy
    {
        private struct Struct_INTERNET_PROXY_INFO
        {
            public int dwAccessType;
            public IntPtr proxy;
            public IntPtr proxyBypass;
        };
        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;
        private const int INTERNET_OPTION_PROXY_SETTINGS_CHANGED = 95;
        private const int INTERNET_OPTION_PROXY = 38;
        private const int INTERNET_OPEN_TYPE_PROXY = 3;
        private const int INTERNET_OPEN_TYPE_DIRECT = 1;
        public static bool Windows_SetActiveProxy(string ip, int port, bool https = false,bool http = false, bool socks = false)
        {
            try
            {
                if (!http && !https && !socks)
                    return Windows_DisableProxy();

                string proxyString = ip + ":" + port.ToString();
                proxyString = ((http) ? "http=" + proxyString + ";" : "") + ((https) ? "https=" + proxyString + ";" : "") + ((socks) ? "socks=" + proxyString + ";" : "");
                if (Windows_IsProxyEnable(proxyString))
                    return RefreshProxySettings(true);

                Struct_INTERNET_PROXY_INFO proxyInfo;
                proxyInfo.dwAccessType = INTERNET_OPEN_TYPE_PROXY;
                proxyInfo.proxy = Marshal.StringToHGlobalAnsi(proxyString);
                proxyInfo.proxyBypass = Marshal.StringToHGlobalAnsi("<local>");
                IntPtr intptrStruct = Marshal.AllocCoTaskMem(Marshal.SizeOf(proxyInfo));
                Marshal.StructureToPtr(proxyInfo, intptrStruct, true);

                if (InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, intptrStruct, Marshal.SizeOf(proxyInfo)))
                {
                    InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY_SETTINGS_CHANGED, IntPtr.Zero, 0);
                    InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                    InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
                    System.Threading.Thread.Sleep(1000);
                    if (Windows_IsProxyEnable(proxyString))
                        return true;
                }

                RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                registry.SetValue("ProxyEnable", 1);
                registry.SetValue("ProxyServer", proxyString);
                registry.Flush();
                registry.Close();
                RefreshProxySettings();
                if (Windows_IsProxyEnable(proxyString))
                    return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool RefreshProxySettings(bool rV)
        {
            RefreshProxySettings();
            return rV;
        }

        public static void RefreshProxySettings()
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            System.Threading.Thread.Sleep(1000);
        }

        public static bool Windows_DisableProxy()
        {
            try
            {
                if (!Windows_IsProxyEnable())
                    return RefreshProxySettings(true);

                Struct_INTERNET_PROXY_INFO proxyInfo;
                proxyInfo.dwAccessType = INTERNET_OPEN_TYPE_DIRECT;
                proxyInfo.proxy = IntPtr.Zero;
                proxyInfo.proxyBypass = IntPtr.Zero;
                IntPtr intptrStruct = Marshal.AllocCoTaskMem(Marshal.SizeOf(proxyInfo));
                Marshal.StructureToPtr(proxyInfo, intptrStruct, true);

                if (InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, intptrStruct, Marshal.SizeOf(proxyInfo)))
                {
                    RefreshProxySettings();
                    if (!Windows_IsProxyEnable())
                        return true;
                }

                RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                registry.SetValue("ProxyEnable", 0);
                registry.SetValue("ProxyServer", "");
                registry.Flush();
                registry.Close();
                RefreshProxySettings();
                if (!Windows_IsProxyEnable())
                    return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool Windows_IsProxyEnable(string proxyString = "")
        {
            try
            {
                RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", false);
                using (registry)
                {
                    if ((int)registry.GetValue("ProxyEnable", 0) != 1)
                        return false;
                    //if (proxyString == string.Empty)
                    //    return false;
                    if ((string)registry.GetValue("ProxyServer", string.Empty) == proxyString)
                        return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool Windows_SetActiveProxyAutoConfig(string address)
        {
            try
            {
                RegistryKey registry;
                if (address == string.Empty)
                    return Windows_DisableProxyAutoConfig();

                if (Windows_IsProxyAutoConfigEnable(address))
                    return RefreshProxySettings(true);

          
                registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                registry.SetValue("AutoConfigURL", address);
                registry.Flush();
                registry.Close();
                RefreshProxySettings();
                if (Windows_IsProxyAutoConfigEnable(address))
                    return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool Windows_DisableProxyAutoConfig()
        {
            try
            {
                RegistryKey registry;

                if (!Windows_IsProxyAutoConfigEnable())
                    return RefreshProxySettings(true);

                registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                registry.DeleteValue("AutoConfigURL", false);
                registry.Flush();
                registry.Close();
                RefreshProxySettings();
                if (!Windows_IsProxyAutoConfigEnable())
                    return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool Windows_IsProxyAutoConfigEnable(string address = "")
        {
            try
            {
                RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", false);
                using (registry)
                {
                    if (address == string.Empty)
                        return false;
                    if (registry.GetValue("AutoConfigURL", string.Empty).Equals(address))
                        return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool IsFirefoxNeedReconfig()
        {
            try
            {
                string firefoxProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
                if (!Directory.Exists(firefoxProfilesPath))
                    return true;
                foreach (string profileAddress in Directory.GetDirectories(firefoxProfilesPath))
                    if (File.Exists(Path.Combine(profileAddress, "prefs.js")))
                    {
                        FileStream fs = File.Open(Path.Combine(profileAddress, "prefs.js"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        StreamReader sr = new StreamReader(fs);
                        while (!sr.EndOfStream)
                            if (sr.ReadLine().ToLower().Contains("\"network.proxy.type\""))
                            {
                                sr.Close();
                                fs.Close();
                                return true;
                            }
                        sr.Close();
                        fs.Close();
                    }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool ForceFirefoxToUseSystemSettings()
        {
            bool isError = false;
            try
            {
                if (System.Diagnostics.Process.GetProcessesByName("firefox").Count() > 0)
                    return false;
                string firefoxProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
                if (!Directory.Exists(firefoxProfilesPath))
                    return true;
                foreach (string profileAddress in Directory.GetDirectories(firefoxProfilesPath))
                {
                    try
                    {
                        if (File.Exists(Path.Combine(profileAddress, "prefs.js")))
                        {
                            string fileKeeper = "";
                            FileStream fs = File.Open(Path.Combine(profileAddress, "prefs.js"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                            StreamReader sr = new StreamReader(fs);
                            while (!sr.EndOfStream)
                            {
                                string data = sr.ReadLine();
                                if (!data.ToLower().Contains("\"network.proxy.type\""))
                                    fileKeeper += data + Environment.NewLine;
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
    }
}
