// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    #endregion

    internal static class Program
    {
        #region Public Methods and Operators

        public static IPEndPoint ParseEndPoint(string endpointstring)
        {
            if (string.IsNullOrEmpty(endpointstring) || endpointstring.Trim().Length == 0)
            {
                throw new ArgumentException("Endpoint descriptor may not be empty.");
            }

            string[] values = endpointstring.Split(new[] { ':' });
            IPAddress ipaddy;
            ushort port;

            //check if we have an IPv6 or ports
            if (values.Length <= 2) // ipv4 or hostname
            {
                if (values.Length == 1)
                {
                    throw new ArgumentException("Invalid or missing port number.");
                }
                port = ParsePort(values[1]);
                ipaddy = ParseIpAddress(values[0]);
            }
            else // IPv6 or anything else
            {
                throw new FormatException(string.Format("Invalid endpoint ipaddress {0}.", endpointstring));
            }

            return new IPEndPoint(ipaddy, port);
        }

        public static IPAddress ParseIpAddress(string p)
        {
            IPAddress ipaddy;
            if (IPAddress.TryParse(p, out ipaddy))
            {
                return ipaddy;
            }

            IPAddress[] hosts = Dns.GetHostAddresses(p);
            if (hosts != null)
            {
                foreach (IPAddress host in hosts)
                {
                    if (host.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return host;
                    }
                }
            }
            throw new ArgumentException(string.Format("Host not found: {0}", p));
        }

        public static ushort ParsePort(string p)
        {
            ushort port;
            if (!ushort.TryParse(p, out port) || port > IPEndPoint.MaxPort)
            {
                throw new FormatException(string.Format("Invalid end point port {0}.", p));
            }

            return port;
        }

        #endregion

        #region Methods

        private static void Main()
        {
            try
            {
                Controller.ProxyEndPoint = ParseEndPoint(CommandLineOptions.Default.Proxy);
                Controller.TargetProcesses.AddRange(
                    CommandLineOptions.Default.Apps.Split(new[] { '|' }).Select(x => x.Trim()).ToArray());
                Controller.IsDebug = CommandLineOptions.Default.IsDebug;
                Controller.DnsGrabberPattern = CommandLineOptions.Default.InvalidResolverPattern;
                Controller.FakeIpSupLevel = CommandLineOptions.Default.DummyIpResolverSupLevel;
                Controller.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }

        #endregion
    }
}