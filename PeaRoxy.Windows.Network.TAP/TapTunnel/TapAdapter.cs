﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using PeaRoxy.Windows.Network.Win32_WMI;

namespace PeaRoxy.Windows.Network.TAP
{
    public partial class TapTunnel
    {
        public class TapAdapter
        {
            public const string DriverServiceName = "tap0901";
            public static NetworkAdapter InstallAnAdapter(string name)
            {
                NetworkAdapter net = NetworkAdapter.GetByServiceName(DriverServiceName);
                if (net == null)
                {
                    if (NetworkAdapter.GetByName(name) != null)
                        return null;
                    string osBit = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                    Common.CreateProcess(
                        "TAPDriver\\" + osBit + "\\tapinstall.exe",
                        "install \"TAPDriver\\" + osBit + "\\OemWin2k.inf\" " + DriverServiceName).WaitForExit();
                }
                net = NetworkAdapter.GetByServiceName(DriverServiceName);
                if (net == null)
                    return null;
                if (!net.RenameAdapter(name))
                    return null;
                return net;
            }

            public static void RemoveAllAdapters()
            {
                string osBit = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                Common.CreateProcess(
                    "TAPDriver\\" + osBit + "\\tapinstall.exe",
                    "remove " + DriverServiceName).WaitForExit();
            }
        }
    }
}
