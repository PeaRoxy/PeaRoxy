using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;
using System.Net;

namespace PeaRoxy.Windows.Network.Win32_WMI
{
    public class IP4RouteTable
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern int GetBestInterface(UInt32 DestAddr, out UInt32 BestIfIndex);

        public uint Age { get; private set; }
        public string Caption { get; private set; }
        public string Description { get; private set; }
        public string Destination { get; private set; }
        public string Information { get; private set; }
        public int InterfaceIndex { get; private set; }
        public string Mask { get; private set; }
        public int Metric1 { get; private set; }
        public int Metric2 { get; private set; }
        public int Metric3 { get; private set; }
        public int Metric4 { get; private set; }
        public int Metric5 { get; private set; }
        public string Name { get; private set; }
        public string NextHop { get; private set; }
        public uint Protocol { get; private set; }
        public string Status { get; private set; }
        public uint Type { get; private set; }
        private ManagementObject ManagementSource;

        public static List<IP4RouteTable> GetCurrentTable()
        {
            List<IP4RouteTable> nas = new List<IP4RouteTable>();
            ManagementClass mc = new ManagementClass("Win32_IP4RouteTable");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
                nas.Add(new IP4RouteTable(mo));
            return nas;
        }

        public IP4RouteTable(ManagementObject mo)
        {
            ManagementSource = mo;
            foreach (System.Reflection.PropertyInfo p in this.GetType().GetProperties())
                p.SetValue(this, ManagementSource[p.Name], null);
        }

        public void RefreshProperties()
        {
            ManagementSource.Get();
            foreach (System.Reflection.PropertyInfo p in this.GetType().GetProperties())
                p.SetValue(this, ManagementSource[p.Name], null);
        }

        public void SetMetric1(int value)
        {
            ManagementSource.SetPropertyValue("Metric1", value);
            ManagementSource.Put();
            RefreshProperties();
        }

        public static NetworkAdapter GetBestInterfaceForIP(IPAddress ip)
        {
            int ad = GetBestInterfaceIndexForIP(ip);
            if (ad == -1)
                return null;
            foreach (NetworkAdapter n in NetworkAdapter.GetNetworks())
                if (n.InterfaceIndex == ad)
                    return n;
            return null;
        }

        public static int GetBestInterfaceIndexForIP(IPAddress ip)
        {
            uint ad;
            #pragma warning disable 618
            int res = GetBestInterface((uint)ip.Address, out ad);
            #pragma warning restore 618
            if (res != 0)
                return -1;
            return (int)ad;
        }

        public static void AddChangeRouteRule(IPAddress destination, IPAddress gateWay, int metric, IPAddress netMask = null, int interfaceId = -1, bool change = false)
        {

            System.Diagnostics.Process p = Windows.Common.CreateProcess(
                "route",
                ((change) ? "CHANGE" : "ADD") + " " + destination.ToString() + " " + ((netMask != null) ? "mask " + netMask.ToString() + " " : "") + gateWay.ToString() + " " + "metric " + metric.ToString() + ((interfaceId > -1) ? " IF " + interfaceId : ""),
                true, false);
            p.WaitForExit();
        }

        public void RemoveRouteRule()
        {
            if (Destination == null || Destination == string.Empty)
                return;
            string str = Destination;
            if (this.NextHop != null && this.NextHop != string.Empty)
                str += " " + this.NextHop;
            if (this.Mask != null && this.Mask != string.Empty)
                str = str.Replace(" ", " mask " + this.Mask + " ");
            if (this.InterfaceIndex > 0)
                str += " IF " + this.InterfaceIndex.ToString();
            System.Diagnostics.Process p = Windows.Common.CreateProcess(
                "route",
                "DELETE " + str,
                true, false);
            p.WaitForExit();
        }
    }
}
