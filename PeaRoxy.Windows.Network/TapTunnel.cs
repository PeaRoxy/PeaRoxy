using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using PeaRoxy.Windows.Network.Win32_WMI;

namespace PeaRoxy.Windows.Network
{
    public partial class TapTunnel
    {
        public IPAddress AdapterAddressRange { get; set; }
        public IPAddress DNSResolvingAddress { get; set; }
        public IPEndPoint SocksProxyEndPoint { get; set; }
        public IPAddress[] ExceptionIPs { get; set; }
        public string TunnelName { get; set; }
        private IPAddress ipSubnet = IPAddress.Parse("255.255.255.0");

        public TapTunnel()
        {
            this.AdapterAddressRange = IPAddress.Parse("10.0.0.0");
            this.DNSResolvingAddress = IPAddress.Parse("8.8.8.8");
            this.SocksProxyEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1080);
            this.ExceptionIPs = new IPAddress[] { IPAddress.Parse("8.8.8.8") };
            this.TunnelName = "PeaRoxy";
        }

        public bool IsRunning()
        {
            return Tun2Socks.IsTun2SocksRunning();
        }

        public void StopTunnel()
        {
            Tun2Socks.StopTun2Socks();
            Tun2Socks.CleanAllTun2Socks();
            foreach (IP4RouteTable ip4 in IP4RouteTable.GetCurrentTable())
                foreach (IPAddress ip in ExceptionIPs)
                    if (ip4.Destination != null && ip4.NextHop != null && ip4.Mask != null && ip4.Destination.ToString() == ip.ToString() && ip4.Mask == "255.255.255.255" && ip4.NextHop != "0.0.0.0")
                    {
                        ip4.RemoveRouteRule();
                        break;
                    }
        }

        public bool StartTunnel()
        {
            if (!CommonLibrary.Common.IsValidIPSubnet(ipSubnet))
                return false;
            AdapterAddressRange = CommonLibrary.Common.MergeIPIntoIPSubnet(AdapterAddressRange, ipSubnet, IPAddress.Parse("10.0.0.1"));
            IPAddress ipAddressGateWay = CommonLibrary.Common.MergeIPIntoIPSubnet(AdapterAddressRange, ipSubnet, IPAddress.Parse("10.0.0.2"));
            NetworkAdapter net = TapAdapter.InstallAnAdapter(TunnelName);
            if (net == null)
                return false;
            if (!Tun2Socks.StartTun2Socks(net, AdapterAddressRange, ipSubnet, ipAddressGateWay, DNSResolvingAddress, SocksProxyEndPoint))
                return false;
            NetworkAdapterConfiguration netconfig = net.GetNetworkConfiguration();
            if (netconfig == null)
                return false;

            int timeout = 30;
            timeout = timeout / 3;
            while (true)
            {
                if (netconfig.SetIPAddresses(AdapterAddressRange.ToString(), ipSubnet.ToString()) && netconfig.SetDnsSearchOrder(DNSResolvingAddress.ToString()))
                    break;
                if (timeout == 0)
                    return false;
                timeout--;
                System.Threading.Thread.Sleep(3000);
            }
            IP4RouteTable.AddChangeRouteRule(IPAddress.Parse("0.0.0.0"), ipAddressGateWay, 2, IPAddress.Parse("0.0.0.0"),(int)net.InterfaceIndex);
            int lowestMetric = 1000;
            IP4RouteTable me = null;
            List<IP4RouteTable> internetRules = new List<IP4RouteTable>();
            foreach (IP4RouteTable r in IP4RouteTable.GetCurrentTable().ToArray())
            {
                if (r.Destination == "0.0.0.0")
                {

                    if (r.InterfaceIndex == net.InterfaceIndex)
                        me = r;
                    else
                    {
                        internetRules.Add(r);
                        if (lowestMetric > r.Metric1)
                            lowestMetric = r.Metric1;
                    }
                }
            }
            if (me == null)
                return false;

            foreach (IP4RouteTable ip4 in IP4RouteTable.GetCurrentTable())
                foreach (IPAddress ip in ExceptionIPs)
                    if (ip4.Destination != null && ip4.NextHop != null && ip4.Mask != null && ip4.Destination.ToString() == ip.ToString() && ip4.Mask == "255.255.255.255" && ip4.NextHop != "0.0.0.0")
                    {
                        ip4.RemoveRouteRule();
                        break;
                    }

            //List<IPAddress> ForwardIPs = new List<IPAddress>(ExceptionIPs);
            //for (int i = 0; i < ForwardIPs.Count; i++)
            //{
            //    int fip = IP4RouteTable.GetBestInterfaceIndexForIP(ForwardIPs[i]);
            //    if (fip != -1)
            //    {
            //        bool isIn = false;
            //        foreach (IP4RouteTable ir in internetRules)
            //            if (ir.InterfaceIndex == fip)
            //            {
            //                isIn = true;
            //                break;
            //            }
            //        if (isIn)
            //            continue;
            //    }
            //    ForwardIPs.Remove(ForwardIPs[i]);
            //    i--;
            //}
            //ExceptionIPs = ForwardIPs.ToArray();

            int des = 0;
            if (me.Metric1 >= lowestMetric)
                des = (me.Metric1 - lowestMetric) + 1;
            foreach (IP4RouteTable r in internetRules)
            {
                if (des > 0)
                    r.SetMetric1(r.Metric1 + des);
                if (r.NextHop != null && r.NextHop != string.Empty)
                    foreach (IPAddress ip in ExceptionIPs)
                        //if (IP4RouteTable.GetBestInterfaceIndexForIP(ip) == net.InterfaceIndex)
                            IP4RouteTable.AddChangeRouteRule(ip, IPAddress.Parse(r.NextHop), 1, null, (int)r.InterfaceIndex);
            }
            return true;
        }
    }
}
