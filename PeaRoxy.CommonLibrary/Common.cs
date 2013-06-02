using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Security.Cryptography;

namespace PeaRoxy.CommonLibrary
{
    public class Common
    {
        public enum Encryption_Type
        {
            None = 0,
            TripleDES = 1,
            SimpleXOR = 2,
            Anything = -1,
        }

        public enum Compression_Type
        {
            None = 0,
            gZip = 1,
            Deflate = 2,
            Anything = -1,
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static int GetFirstBytePatternIndex(byte[] bytes, byte[] pattern,int start = 0)
        {
            for (int i = start; i < bytes.Length; i++)
            {
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch == true; j++)
                    {
                        if (bytes[i + j] != pattern[j])
                            ismatch = false;
                    }
                    if (ismatch)
                        return i;
                }
            }
            return -1;
        }


        [System.Diagnostics.DebuggerStepThrough]
        public static bool IsIPAddress(string IP)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(
                IP,
                @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b"
                );
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static string ConvertToHtmlEntities(string plainText)
        {
            StringBuilder sb = new StringBuilder(plainText.Length * 6);
            foreach (char c in plainText)
            {
                if (c == ' ')
                    sb.Append("&nbsp;");
                else if (c == ' ')
                    sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                else if (c == '\r')
                    sb.Append("<br />");
                else if (c == '\n')
                {
                    // Do nothing
                }
                else
                    sb.Append("&#").Append((ushort)c).Append(';');
            }
            return sb.ToString();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static bool IsSocketConnected(Socket client)
        {
            try
            {
                if (!client.Connected)
                    return false;
                if (client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] checkConn = new byte[1];
                    if (client.Receive(checkConn, SocketFlags.Peek) == 0)
                        return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static string FormatFileSizeAsString(long len)
        {
            if (len < 750 && len > 0)
                return "0.0 B ~";
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = len;
            for (i = 0; (int)(len / 1024) > 0; i++, len /= 1024)
                dblSByte = len / 1024.0;
            return String.Format("{0:0.0} {1}", dblSByte, Suffix[i]);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static string ToRegEX(string c)
        {
            c = c.Replace("\\r", "\r");
            c = c.Replace("\\n", "\n");
            return c.Replace("*", "(.*)").Replace("?", "(.?)").Replace(" ", @"\s");
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static string FromRegEX(string c)
        {
            c = c.Replace("\r", "\\r");
            c = c.Replace("\n", "\\n");
            return c.Replace("(.*)","*").Replace("(.?)","?").Replace(@"\s"," ");
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static string GetNextLevelDomain(string hostname)
        {
            try
            {
                return hostname;
                //string[] SecondUnAcceptableDomainNames = new string[] { "aero", "asia", "biz", "cat", "com", "coop", "info", "int", "jobs", "mobi", "museum", "name", "net", "org", "pro", "tel", "travel", "xxx" };
                //int pl = hostname.IndexOf(".");
                //if (pl < 0)
                //    return hostname;
                //if (SecondUnAcceptableDomainNames.Contains(hostname.Substring(pl + 1)))
                //    return hostname;
                //return hostname.Substring(pl + 1);
                //
                //pl = hostname.LastIndexOf(".", pl - 1);
                //if (pl < 0)
                //    return hostname;
                //if (SecondUnAcceptableDomainNames.Contains(hostname.Substring(pl + 1, hostname.LastIndexOf(".") - (pl + 1))))
                //    pl = hostname.LastIndexOf(".", pl - 1);
                //if (pl < 0)
                //    return hostname;
                //return hostname.Substring(pl + 1);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static bool IsMatchWildCard(string subject, string wildcard)
        {
            if (wildcard == string.Empty)
                return false;
            string[] countParts = wildcard.Split(new char[] { '*' });
            int lf = 0;
            for (int i = 0; i < countParts.Length; i++)
            {
                if (countParts[i] != string.Empty)
                    if (i == 0 && !subject.StartsWith(countParts[i]))
                        return false;
                    else if (i == countParts.Length && !subject.EndsWith(countParts[i]))
                        return false;
                lf = subject.IndexOf(countParts[i], lf);
                if (lf == -1)
                    return false;
                else
                    lf += countParts[i].Length;
            }
            return true;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static bool IsValidIPSubnet(IPAddress ipSubnet)
        {
            int lower = 255;
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 && ipSubnet.GetAddressBytes()[i] == 0)
                    return false;
                if (ipSubnet.GetAddressBytes()[i] > lower)
                    return false;
                else
                    lower = ipSubnet.GetAddressBytes()[i];
            }
            return true;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static IPAddress MergeIPIntoIPSubnet(IPAddress ipAddress, IPAddress ipSubnet, IPAddress NewipAddress)
        {
            if (!IsValidIPSubnet(ipSubnet))
                return null;
            string ip = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                if (i != 0)
                    ip += ".";
                if ((255 - ipSubnet.GetAddressBytes()[i]) >= Math.Abs(NewipAddress.GetAddressBytes()[i] - ipAddress.GetAddressBytes()[i]))
                    ip += NewipAddress.GetAddressBytes()[i].ToString();
                else
                    ip += ipAddress.GetAddressBytes()[i].ToString();
            }
            return IPAddress.Parse(ip);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static string MD5(string str)
        {
            MD5 md5 = MD5CryptoServiceProvider.Create();
            byte[] dataMd5 = md5.ComputeHash(Encoding.Default.GetBytes(str));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dataMd5.Length; i++)
                sb.AppendFormat("{0:x2}", dataMd5[i]);
            return sb.ToString();
        }
    }
}
