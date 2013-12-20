using System;
using System.Net;

namespace PeaRoxy.Windows.Network.TAP
{
    public partial class TapTunnel
    {
        public class Tun2Socks
        {
            static System.Diagnostics.Process Tun2SocksProcess = null;

            public static bool IsTun2SocksRunning()
            {
                if (Tun2SocksProcess != null && Tun2SocksProcess.HasExited == false)
                    return true;
                return false;
            }

            public static bool StopTun2Socks()
            {
                if (IsTun2SocksRunning())
                {
                    Tun2SocksProcess.Kill();
                    Tun2SocksProcess.WaitForExit();
                    return true;
                }
                return false;
            }

            public static void CleanAllTun2Socks()
            {
                bool wasA = false;
                foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
                    if (p.ProcessName == "tun2socks")
                        try
                        {
                            wasA = true;
                            p.Kill();
                            p.WaitForExit();
                        }
                        catch (Exception) { }
                if (wasA)
                    System.Threading.Thread.Sleep(5000);
            }

            public static bool StartTun2Socks(Win32_WMI.NetworkAdapter network, IPAddress ipAddress, IPAddress ipSubnet, IPAddress ipServer, IPEndPoint SocksProxy)
            {
                CleanAllTun2Socks();
                Tun2SocksProcess = Common.CreateProcess(
                    "TAPDriver\\tun2socks.exe",
                    "--tundev \"" + network.ServiceName + ":" + network.NetConnectionID + ":" + ipAddress.ToString() + ":" + CommonLibrary.Common.MergeIpIntoIpSubnet(ipAddress, ipSubnet, IPAddress.Parse("10.0.0.0")).ToString() + ":" + ipSubnet.ToString() + "\" --netif-ipaddr " + ipServer.ToString() + " --netif-netmask " + ipSubnet.ToString() + " --socks-server-addr " + SocksProxy.Address.ToString() + ":" + SocksProxy.Port.ToString());
                int timeout = 120;
                timeout = timeout * 10;
                while (!network.NetEnabled && // Vista+
                    network.NetConnectionStatus != 1 && // XP SP3+
                    network.NetConnectionStatus != 2 &&  // XP SP3+
                    network.NetConnectionStatus != 9) // XP SP2-
                {
                    if (timeout == 0)
                        return StopTun2Socks() && false;
                    if (Tun2SocksProcess.HasExited)
                        return StopTun2Socks() && false;
                    network.RefreshProperties();
                    System.Threading.Thread.Sleep(100);
                    timeout--;
                }
                return true;
            }
        }
    }
}
