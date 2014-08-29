// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Common.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CommonLibrary
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     The Common class contains the common and independent methods
    /// </summary>
    public static class Common
    {
        /// <summary>
        ///     The Compression Types
        /// </summary>
        [Flags]
        public enum CompressionTypes : byte
        {
            None = 1,

            GZip = 2,

            Deflate = 4,

            AllDefaults = None | GZip | Deflate,
        }

        /// <summary>
        ///     The Encryption Types
        /// </summary>
        [Flags]
        public enum EncryptionTypes : byte
        {
            None = 1,

            TripleDes = 2,

            SimpleXor = 4,

            AllDefaults = None | TripleDes | SimpleXor,
        }
        
        /// <summary>
        ///     Convert string to HTML entities.
        /// </summary>
        /// <param name="plainText">
        ///     The plain text.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
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
        ///     Format the file size as string.
        /// </summary>
        /// <param name="fileSize">
        ///     The file size.
        /// </param>
        /// <returns>
        ///     The formated file size as <see cref="string" />.
        /// </returns>
        [DebuggerStepThrough]
        public static string FormatFileSizeAsString(long fileSize)
        {
            if (fileSize < 750 && fileSize > 0)
            {
                return "0.0 B ~";
            }

            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = fileSize;
            for (i = 0; (int)(fileSize / 1024) > 0; i++, fileSize /= 1024)
            {
                dblSByte = fileSize / 1024.0;
            }

            return string.Format("{0:0.0} {1}", dblSByte, suffix[i]);
        }

        /// <summary>
        ///     Searches for specified pattern in the byte array
        /// </summary>
        /// <param name="bytes">
        ///     The bytes to check.
        /// </param>
        /// <param name="pattern">
        ///     The pattern to search for.
        /// </param>
        /// <param name="start">
        ///     The starting index.
        /// </param>
        /// <returns>
        ///     The index of the first byte of the pattern in the array as <see cref="int" />. -1 if not found.
        /// </returns>
        [DebuggerStepThrough]
        public static int IndexOfPatternInArray(byte[] bytes, byte[] pattern, int start = 0)
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
        ///     Check if the provided string is an IP address
        /// </summary>
        /// <param name="ip">
        ///     The string.
        /// </param>
        /// <returns>
        ///     A <see cref="bool" /> value indicating if the provided string could be an IP address.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsIpAddress(string ip)
        {
            return Regex.IsMatch(
                ip,
                @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b");
        }

        /// <summary>
        ///     Check if the provided string match the specified wild card
        /// </summary>
        /// <param name="subject">
        ///     The string to check.
        /// </param>
        /// <param name="wildcard">
        ///     The wild card to check against.
        /// </param>
        /// <returns>
        ///     The result as a <see cref="bool" /> value.
        /// </returns>
        [DebuggerStepThrough]
        public static bool DoesMatchWildCard(string subject, string wildcard)
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
        ///     Check if the specific Net.Sockets.Socket object is still connected and valid
        /// </summary>
        /// <param name="client">
        ///     The Net.Sockets.Socket object.
        /// </param>
        /// <returns>
        ///     The connection state as a <see cref="bool" /> value.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsSocketConnected(Socket client)
        {
            try
            {
                if (client == null || !client.Connected)
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
        ///     Check whether the specified IPSubnet is valid
        /// </summary>
        /// <param name="subnetIp">
        ///     The IP subnet.
        /// </param>
        /// <returns>
        ///     The result as a <see cref="bool" /> value.
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
        ///     MD5 hashing of the string
        /// </summary>
        /// <param name="str">
        ///     The string.
        /// </param>
        /// <returns>
        ///     The hash as a <see cref="string" /> value.
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
        ///     Merge an IP address into the specified IP subnet based on another IP address
        /// </summary>
        /// <param name="ip">
        ///     The IP address.
        /// </param>
        /// <param name="subnetIp">
        ///     The IP subnet.
        /// </param>
        /// <param name="newIp">
        ///     The new IP address.
        /// </param>
        /// <returns>
        ///     The resulting <see cref="IPAddress" />.
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
        ///     Convert the wild card string to a RegEx pattern
        /// </summary>
        /// <param name="wildcard">
        ///     The wild card string.
        /// </param>
        /// <returns>
        ///     The RegEx pattern as a <see cref="string" /> value.
        /// </returns>
        [DebuggerStepThrough]
        public static string ToRegEx(string wildcard)
        {
            wildcard = wildcard.Replace(@"\r", "\r");
            wildcard = wildcard.Replace(@"\n", "\n");
            return wildcard.Replace("*", "(.*)").Replace("?", "(.?)").Replace(" ", @"\s");
        }
    }
}