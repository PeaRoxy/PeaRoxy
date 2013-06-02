using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PeaRoxy.CoreProtocol
{
    public class HTTPForger
    {
        private static Random rnd = new Random();
        private int eofPointer = 0;
        private int domainPointer = 0;
        private int hPointer = 0;
        private byte[] hostString = System.Text.Encoding.ASCII.GetBytes("Host: ");
        private byte[] domainName;
        private bool async;
        private Socket client;
        public byte[] HeaderBytes = new byte[] {};
        private int maxBuffering;
        private int timeoutCounter = 0;
        private int timeout = 0;
        public HTTPForger(Socket Client, string DomainName = "", bool Async = false,int MaxBuffering = 8192, int AsyncTimeout = 60)
        {
            this.domainName = System.Text.Encoding.ASCII.GetBytes(DomainName.ToLower().Trim());
            this.async = Async;
            this.timeout = this.timeoutCounter = AsyncTimeout * 100;
            this.maxBuffering = MaxBuffering;
            this.client = Client;
        }

        public bool ReceiveRequest()
        {
            bool p = false;
            return ReceiveRequest(out p);
        }

        public bool ReceiveResponse()
        {
            byte[] bytes = new byte[1];
            while (((async && client.Available > 0) || (!async)) && maxBuffering > 0 && timeoutCounter > 0)
            {
                if (!CommonLibrary.Common.IsSocketConnected(client))
                    return false;
                if (client.Available > 0)
                {
                    timeoutCounter = timeout;
                    maxBuffering--;
                    int i = client.Receive(bytes);
                    if (i > 0)
                    {
                        if (bytes[0] == 13 || bytes[0] == 10)
                            eofPointer += 1;
                        else
                            eofPointer = 0;
                    }
                    else
                        return false;
                    if (eofPointer == 4)
                        return true;
                }
                else
                {
                    timeoutCounter--;
                    System.Threading.Thread.Sleep(10);
                }
            }
            return false;
        }

        public bool ReceiveRequest(out bool NoRelated)
        {
            byte[] bytes = new byte[1];
            NoRelated = false;
            while (((async && client.Available > 0) || (!async)) && maxBuffering > 0 && timeoutCounter > 0)
            {
                if (client.Available > 0)
                {
                    timeoutCounter = timeout;
                    maxBuffering--;
                    int i = client.Receive(bytes);
                    if (i > 0)
                    {
                        Array.Resize(ref HeaderBytes, HeaderBytes.Length + 1);
                        HeaderBytes[HeaderBytes.Length - 1] = bytes[0];
                        if (bytes[0] == 13 || bytes[0] == 10)
                            eofPointer += 1;
                        else
                            eofPointer = 0;
                    }
                    else
                        return false;
                    if (domainName.Count() > 0)
                        if (hPointer == hostString.Count())
                        {
                            if (domainPointer != domainName.Count())
                                if (bytes[0] == 13 || bytes[0] == 10)
                                {
                                    NoRelated = true;
                                    return false;
                                }
                                else
                                    if (bytes[0] == domainName[domainPointer])
                                        domainPointer++;
                                    else
                                        domainPointer = 0;
                        }
                        else
                            if (bytes[0] == hostString[hPointer])
                                hPointer++;
                            else
                                hPointer = 0;
                    if (eofPointer == 4)
                    {
                        NoRelated = (domainName.Count() > 0 && domainPointer != domainName.Count());
                        return true;
                    }
                }
                else
                {
                    timeoutCounter--;
                    System.Threading.Thread.Sleep(10);
                }
            }
            if (maxBuffering <= 0)
                NoRelated = true;
            return false;
        }

        public static void SendRequest(Socket client, string hostname = "~", string file = "~", string type = "GET", string version = "HTTP/1.1")
        {
            if (file.IndexOf("~") != -1)
                file = file.Replace("~", HTTPForger.RandomFilename());
            if (hostname.IndexOf("~") != -1)
                hostname = hostname.Replace("~", HTTPForger.RandomHostname());
            string header = type + " " + file + " " + version + "\r\n" +
                            "Host: " + hostname.Trim().ToLower() + "\r\n" +
                            "User-Agent: Mozilla/5.0 (Windows NT 6.1; " + ((Math.Round((double)rnd.Next(0, 1)) == 1) ? "WOW64; " : string.Empty) + "rv:" + Math.Round((double)rnd.Next(3, 12), 1) + ") Gecko/20100101 Firefox/" + Math.Round((double)rnd.Next(3, 12), 1) + "\r\n" +
                            "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" + "\r\n" +
                            "Accept-Language: en-us,en;q=0.5" + "\r\n" +
                            "Accept-Encoding: gzip, deflate" + "\r\n" +
                            "Connection: keep-alive" + "\r\n" +
                            "\r\n";
            Byte[] byteDateLine = Encoding.ASCII.GetBytes(header.ToCharArray());
            client.Send(byteDateLine, byteDateLine.Length, 0);
        }

        public static void SendResponse(Socket client, string code = "200 OK", string version = "HTTP/1.1")
        {
            string header = version + " " + code + "\r\n" +
                            "Date: " + DateTime.Now.ToString("ddd, dd MMM yyyy hh\\:mm\\:ss \\G\\M\\T") + "\r\n" +
                            "Server: Apache/1.3." + Math.Round((double)rnd.Next(0, 35), 1) + " " + ((Math.Round((double)rnd.Next(0, 1)) == 1) ? "(Unix) (Red-Hat/Linux) " : "(Windows) (Windows 7)") + "\r\n" +
                            "Last-Modified: " + new DateTime(2000, 1, 1).AddMinutes(rnd.Next(0, 6835680)).ToString("ddd, dd MMM yyyy hh\\:mm\\:ss \\G\\M\\T") + "\r\n" +
                            "Accept-Ranges:  none" + "\r\n" +
                            "Content-Type: text/html; charset=UTF-8" + "\r\n" +
                            "Connection: close" + "\r\n" +
                            "\r\n";
            Byte[] byteDateLine = Encoding.ASCII.GetBytes(header.ToCharArray());
            client.Send(byteDateLine, byteDateLine.Length, 0);
        }

        private static string RandomFilename()
        {
            int r = rnd.Next(1, 5);
            string res = string.Empty;
            for (int i = 0; i < r; i++)
            {
                res += "/" + System.IO.Path.GetRandomFileName();
            }
            return res;
        }
        private static string RandomHostname()
        {
            string[] domainends = { "com", "net", "ir", "org", "info" };
            int r = rnd.Next(1, domainends.Length);
            string res = string.Empty;
            int p = (r % 2) + 1;
            for (int i = 0; i < p; i++)
            {
                res += System.IO.Path.GetRandomFileName().Replace(".",string.Empty);
            }
            return res + "." + domainends[r-1];
        }
    }
}
