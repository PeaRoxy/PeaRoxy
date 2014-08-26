// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Common.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The common.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CommonLibrary
{
    #region

    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    #endregion

    /// <summary>
    /// The common.
    /// </summary>
    public static class Common
    {
        #region Enums

        /// <summary>
        /// The compression type.
        /// </summary>
        public enum CompressionType
        {
            /// <summary>
            /// The none.
            /// </summary>
            None = 0, 

            /// <summary>
            /// The GZip.
            /// </summary>
            GZip = 1, 

            /// <summary>
            /// The deflate.
            /// </summary>
            Deflate = 2, 

            /// <summary>
            /// The anything.
            /// </summary>
            Anything = -1, 
        }

        /// <summary>
        /// The encryption type.
        /// </summary>
        public enum EncryptionType
        {
            /// <summary>
            /// The none.
            /// </summary>
            None = 0, 

            /// <summary>
            /// The triple DES.
            /// </summary>
            TripleDes = 1, 

            /// <summary>
            /// The simple XOR.
            /// </summary>
            SimpleXor = 2, 

            /// <summary>
            /// The anything.
            /// </summary>
            Anything = -1, 
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The convert to html entities.
        /// </summary>
        /// <param name="plainText">
        /// The plain text.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static string ConvertToHtmlEntities(string plainText)
        {
            StringBuilder sb = new StringBuilder(plainText.Length * 6);
            foreach (char c in plainText)
            {
                if (c == ' ')
                {
                    sb.Append("&nbsp;");
                }
                else if (c == ' ')
                {
                    sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                }
                else if (c == '\r')
                {
                    sb.Append("<br />");
                }
                else if (c == '\n')
                {
                    // Do nothing
                }
                else
                {
                    sb.Append("&#").Append((ushort)c).Append(';');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// The format file size as string.
        /// </summary>
        /// <param name="len">
        /// The len.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static string FormatFileSizeAsString(long len)
        {
            if (len < 750 && len > 0)
            {
                return "0.0 B ~";
            }

            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = len;
            for (i = 0; (int)(len / 1024) > 0; i++, len /= 1024)
            {
                dblSByte = len / 1024.0;
            }

            return string.Format("{0:0.0} {1}", dblSByte, suffix[i]);
        }

        /// <summary>
        /// The String from REGEX
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static string FromRegEx(string c)
        {
            c = c.Replace("\r", "\\r");
            c = c.Replace("\n", "\\n");
            return c.Replace("(.*)", "*").Replace("(.?)", "?").Replace(@"\s", " ");
        }

        /// <summary>
        /// The get first byte pattern index.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static int GetFirstBytePatternIndex(byte[] bytes, byte[] pattern, int start = 0)
        {
            for (int i = start; i < bytes.Length; i++)
            {
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch; j++)
                    {
                        if (bytes[i + j] != pattern[j])
                        {
                            ismatch = false;
                        }
                    }

                    if (ismatch)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// The get next level domain.
        /// </summary>
        /// <param name="hostname">
        /// The hostname.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static string GetNextLevelDomain(string hostname)
        {
            try
            {
                return hostname;

                // string[] SecondUnAcceptableDomainNames = new string[] { "aero", "asia", "biz", "cat", "com", "coop", "info", "int", "jobs", "mobi", "museum", "name", "net", "org", "pro", "tel", "travel", "xxx" };
                // int pl = hostname.IndexOf(".");
                // if (pl < 0)
                // return hostname;
                // if (SecondUnAcceptableDomainNames.Contains(hostname.Substring(pl + 1)))
                // return hostname;
                // return hostname.Substring(pl + 1);
                // pl = hostname.LastIndexOf(".", pl - 1);
                // if (pl < 0)
                // return hostname;
                // if (SecondUnAcceptableDomainNames.Contains(hostname.Substring(pl + 1, hostname.LastIndexOf(".") - (pl + 1))))
                // pl = hostname.LastIndexOf(".", pl - 1);
                // if (pl < 0)
                // return hostname;
                // return hostname.Substring(pl + 1);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The is IP address.
        /// </summary>
        /// <param name="ip">
        /// The IP.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsIpAddress(string ip)
        {
            return Regex.IsMatch(
                ip, 
                @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b");
        }

        /// <summary>
        /// The is match wild card.
        /// </summary>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="wildcard">
        /// The wildcard.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsMatchWildCard(string subject, string wildcard)
        {
            if (wildcard == string.Empty)
            {
                return false;
            }

            string[] countParts = wildcard.Split(new[] { '*' });
            int lf = 0;
            for (int i = 0; i < countParts.Length; i++)
            {
                if (countParts[i] != string.Empty)
                {
                    if (i == 0 && !subject.StartsWith(countParts[i]))
                    {
                        return false;
                    }

                    if (i == countParts.Length && !subject.EndsWith(countParts[i]))
                    {
                        return false;
                    }
                }

                lf = subject.IndexOf(countParts[i], lf, StringComparison.Ordinal);
                if (lf == -1)
                {
                    return false;
                }

                lf += countParts[i].Length;
            }

            return true;
        }

        /// <summary>
        /// The is socket connected.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsSocketConnected(Socket client)
        {
            try
            {
                if (!client.Connected)
                {
                    return false;
                }

                if (client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] checkConn = new byte[1];
                    if (client.Receive(checkConn, SocketFlags.Peek) == 0)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// The is valid IP subnet.
        /// </summary>
        /// <param name="subnetIp">
        /// The IP subnet.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsValidIpSubnet(IPAddress subnetIp)
        {
            int lower = 255;
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 && subnetIp.GetAddressBytes()[i] == 0)
                {
                    return false;
                }

                if (subnetIp.GetAddressBytes()[i] > lower)
                {
                    return false;
                }

                lower = subnetIp.GetAddressBytes()[i];
            }

            return true;
        }

        /// <summary>
        /// The MD5 hasher.
        /// </summary>
        /// <param name="str">
        /// The string.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static string Md5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] dataMd5 = md5.ComputeHash(Encoding.Default.GetBytes(str));
            StringBuilder sb = new StringBuilder();
            foreach (byte d in dataMd5)
            {
                sb.AppendFormat("{0:x2}", d);
            }

            return sb.ToString();
        }

        /// <summary>
        /// The merge IP into IP subnet.
        /// </summary>
        /// <param name="ip">
        /// The IP address.
        /// </param>
        /// <param name="subnetIp">
        /// The IP subnet.
        /// </param>
        /// <param name="newIp">
        /// The new IP address.
        /// </param>
        /// <returns>
        /// The <see cref="IPAddress"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static IPAddress MergeIpIntoIpSubnet(IPAddress ip, IPAddress subnetIp, IPAddress newIp)
        {
            if (!IsValidIpSubnet(subnetIp))
            {
                return null;
            }

            string strIp = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                if (i != 0)
                {
                    strIp += ".";
                }

                if ((255 - subnetIp.GetAddressBytes()[i])
                    >= Math.Abs(newIp.GetAddressBytes()[i] - ip.GetAddressBytes()[i]))
                {
                    strIp += newIp.GetAddressBytes()[i].ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    strIp += ip.GetAddressBytes()[i].ToString(CultureInfo.InvariantCulture);
                }
            }

            return IPAddress.Parse(strIp);
        }

        /// <summary>
        /// The convert wild card to REGEX
        /// </summary>
        /// <param name="wildcard">
        /// The wild card.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static string ToRegEx(string wildcard)
        {
            wildcard = wildcard.Replace("\\r", "\r");
            wildcard = wildcard.Replace("\\n", "\n");
            return wildcard.Replace("*", "(.*)").Replace("?", "(.?)").Replace(" ", @"\s");
        }

        #endregion
    }
}