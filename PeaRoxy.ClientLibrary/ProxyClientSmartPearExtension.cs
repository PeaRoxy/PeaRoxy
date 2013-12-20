// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyClientSmartPearExtension.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The proxy_ client smart pear extention.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    #region

    using System;
    using System.Net;
    using System.Text;

    using PeaRoxy.ClientLibrary.ProxyModules;
    using PeaRoxy.ClientLibrary.ServerModules;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.Platform;

    #endregion

    /// <summary>
    /// The proxy client smart pear extension class
    /// </summary>
    public static class ProxyClientSmartPearExtension
    {
        #region Public Methods and Operators

        /// <summary>
        /// The direct_ connection status callback.
        /// </summary>
        /// <param name="currentClient">
        /// The current client.
        /// </param>
        /// <param name="currentActiveServer">
        /// The current active server.
        /// </param>
        /// <param name="success">
        /// The success.
        /// </param>
        /// <param name="isSocks">
        /// The is socks.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool DirectConnectionStatusCallback(
            this ProxyClient currentClient, 
            ServerType currentActiveServer, 
            bool success, 
            bool isSocks)
        {
            if (currentClient.IsSmartForwarderEnable)
            {
                SmartPear smart = currentClient.Controller.SmartPear;
                if (!success)
                {
                    if (smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable)
                        && smart.DetectorTimeoutEnable)
                    {
                        currentClient.IsSmartForwarderEnable = false;
                        if (isSocks)
                        {
                            ProxyModules.Socks5.DirectHandle(
                                currentClient, 
                                currentActiveServer.GetAddress(), 
                                currentActiveServer.GetPort(), 
                                currentClient.SmartRequestBuffer);
                        }
                        else
                        {
                            ProxyModules.Https.DirectHandle(
                                currentClient, 
                                currentActiveServer.GetAddress(), 
                                currentActiveServer.GetPort(), 
                                currentClient.SmartRequestBuffer);
                        }

                        return false;
                    }
                }
                else
                {
                    if (smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable)
                        && smart.DetectorDnsGrabberEnable && currentActiveServer.UnderlyingSocket != null)
                    {
                        if (currentActiveServer.UnderlyingSocket.RemoteEndPoint != null
                            && smart.DetectorDnsGrabberRegEx.IsMatch(
                                ((IPEndPoint)currentActiveServer.UnderlyingSocket.RemoteEndPoint).Address.ToString()))
                        {
                            currentClient.IsSmartForwarderEnable = false;
                            if (isSocks)
                            {
                                ProxyModules.Socks5.DirectHandle(
                                    currentClient, 
                                    currentActiveServer.GetAddress(), 
                                    currentActiveServer.GetPort(), 
                                    currentClient.SmartRequestBuffer);
                            }
                            else
                            {
                                ProxyModules.Https.DirectHandle(
                                    currentClient, 
                                    currentActiveServer.GetAddress(), 
                                    currentActiveServer.GetPort(), 
                                    currentClient.SmartRequestBuffer);
                            }

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// The direct_ data received callback.
        /// </summary>
        /// <param name="currentClient">
        /// The current client.
        /// </param>
        /// <param name="binary">
        /// The binary.
        /// </param>
        /// <param name="currentActiveServer">
        /// The current active server.
        /// </param>
        /// <param name="isSocks">
        /// The is socks.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool DirectDataReceivedCallback(
            this ProxyClient currentClient, 
            ref byte[] binary, 
            ServerType currentActiveServer, 
            bool isSocks)
        {
            SmartPear smart = currentClient.Controller.SmartPear;
            if (smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable))
            {
                // If HTTPS Forwarded
                if (!currentClient.IsSmartForwarderEnable
                    && ((smart.DetectorDirectPort80AsHttp && smart.ForwarderDirectPort80AsHttp)
                        || smart.DetectorStatusDnsGrabber || smart.DetectorStatusTimeout))
                {
                    // If using proxy, and forwarder HTTPS is enable
                    Uri url;
                    if (currentClient.RequestAddress != string.Empty
                        && Uri.TryCreate(currentClient.RequestAddress, UriKind.Absolute, out url))
                    {
                        if (smart.DetectorDirectPort80AsHttp && smart.ForwarderDirectPort80AsHttp && url.Port == 80
                            && Http.IsHttp(currentClient.SmartRequestBuffer))
                        {
                            smart.AddRuleToHttpForwarder(
                                "* | *" + url.Host.ToLower().TrimEnd(new[] { '/', '\\' }) + "*");
                        }
                        else
                        {
                            smart.AddRuleToDirectForwarder("* | *" + url.Host.ToLower() + ":" + url.Port);
                        }
                    }

                    currentActiveServer.ReceiveDataDelegate = null;
                    return true;
                }

                if (smart.ForwarderDirectPort80AsHttp && smart.DetectorDirectPort80AsHttp
                    && currentClient.RequestAddress.EndsWith(":80")
                    && Http.IsHttp(currentClient.SmartRequestBuffer))
                {
                    // If we have Forwarder Enabled
                    bool blocked = false;
                    if (smart.DetectorStatusHttp
                        && currentClient.SmartResponseBuffer.Length < smart.DetectorHttpMaxBuffering)
                    {
                        // If detector is enable and responce is less than buffer size
                        byte[] smartResponseBuffer = currentClient.SmartResponseBuffer;
                        Array.Resize(
                            ref smartResponseBuffer,
                            smartResponseBuffer.Length + binary.Length);
                        Array.Copy(
                            binary, 
                            0,
                            smartResponseBuffer,
                            smartResponseBuffer.Length - binary.Length, 
                            binary.Length);
                        Array.Resize(ref binary, 0);
                        if (
                            smart.DetectorHttpRegEx.IsMatch(
                                Encoding.ASCII.GetString(smartResponseBuffer)))
                        {
                            // If Responce is FILTERED
                            blocked = true;
                        }

                        currentClient.SmartResponseBuffer = smartResponseBuffer;
                    }

                    if (blocked)
                    {
                        currentActiveServer.ReceiveDataDelegate = null;
                        if (currentClient.IsSmartForwarderEnable)
                        {
                            // If client use NoServer
                            byte[] localReqBackup = new byte[currentClient.SmartRequestBuffer.Length];
                            Array.Copy(currentClient.SmartRequestBuffer, localReqBackup, localReqBackup.Length);
                            currentClient.ForwarderClean();
                            currentClient.IsSmartForwarderEnable = false;
                            Http.DirectHandle(
                                currentClient, 
                                currentActiveServer.GetAddress(), 
                                currentActiveServer.GetPort(), 
                                localReqBackup);
                            return false;
                        }
                    }
                    else
                    {
                        // Responce is OK
                        if (!currentClient.IsSmartForwarderEnable
                            && (smart.DetectorStatusHttp || smart.DetectorStatusDnsGrabber
                                || smart.DetectorStatusTimeout))
                        {
                            // If client use Proxy and one of possible detectors is enable
                            Uri url;
                            if (currentClient.RequestAddress != string.Empty
                                && Uri.TryCreate(currentClient.RequestAddress, UriKind.Absolute, out url))
                            {
                                smart.AddRuleToHttpForwarder(
                                    "* | *" + url.Host.ToLower().TrimEnd(new[] { '/', '\\' }) + "*");
                                    
                                    // Bug: I dont have time to solve it and also not importante as if a address is blocked, so other case of that address is also blocked.
                            }

                            currentActiveServer.ReceiveDataDelegate = null;
                        }
                    }

                    if (currentClient.SmartResponseBuffer.Length > 0
                        && (!currentClient.IsSmartForwarderEnable
                            || currentClient.SmartResponseBuffer.Length >= smart.DetectorHttpMaxBuffering))
                    {
                        // If we have any thing in Responce and (Client use Proxy or Client use NoServer but Responce buffer is bigger than buffer)
                        currentClient.ForwarderFlush(smart.ForwarderHttpEnable);
                        currentActiveServer.ReceiveDataDelegate = null;
                    }
                }
            }
            else
            {
                currentActiveServer.ReceiveDataDelegate = null;
            }

            return true;
        }

        /// <summary>
        /// The direct_ data sent callback.
        /// </summary>
        /// <param name="currentClient">
        /// The current client.
        /// </param>
        /// <param name="binary">
        /// The binary.
        /// </param>
        /// <param name="isSocks">
        /// The is socks.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool DirectDataSentCallback(this ProxyClient currentClient, byte[] binary, bool isSocks)
        {
            SmartPear smart = currentClient.Controller.SmartPear;
            if (currentClient.IsSmartForwarderEnable && smart.ForwarderHttpsEnable
                && (!isSocks || smart.ForwarderSocksEnable))
            {
                byte[] smartRequestBuffer = currentClient.SmartRequestBuffer;
                Array.Resize(
                    ref smartRequestBuffer,
                    smartRequestBuffer.Length + binary.Length);
                Array.Copy(
                    binary, 
                    0,
                    smartRequestBuffer,
                    smartRequestBuffer.Length - binary.Length, 
                    binary.Length);
                currentClient.SmartRequestBuffer = smartRequestBuffer;
            }
            else if (currentClient.SmartResponseBuffer.Length > 0)
            {
                currentClient.ForwarderFlush(
                    smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable));
            }

            return true;
        }

        /// <summary>
        /// The forwarder_ clean.
        /// </summary>
        /// <param name="thisclient">
        /// The this client.
        /// </param>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public static void ForwarderClean(this ProxyClient thisclient, bool? enable = null)
        {
            thisclient.IsSmartForwarderEnable = (enable == null)
                                                     ? thisclient.Controller.SmartPear.ForwarderHttpEnable
                                                     : (bool)enable;
            thisclient.SmartResponseBuffer = new byte[0];
            thisclient.SmartRequestBuffer = new byte[0];
        }

        /// <summary>
        /// The forwarder_ flush.
        /// </summary>
        /// <param name="thisclient">
        /// The this client.
        /// </param>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public static void ForwarderFlush(this ProxyClient thisclient, bool enable)
        {
            thisclient.Write(thisclient.SmartResponseBuffer);
            thisclient.ForwarderClean(enable);
            thisclient.IsSmartForwarderEnable = false;
        }

        /// <summary>
        /// The http connection status callback.
        /// </summary>
        /// <param name="currentClient">
        /// The current client.
        /// </param>
        /// <param name="currentActiveServer">
        /// The current active server.
        /// </param>
        /// <param name="success">
        /// The success.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool HttpConnectionStatusCallback(
            this ProxyClient currentClient, 
            ServerType currentActiveServer, 
            bool success)
        {
            if (currentClient.IsSmartForwarderEnable)
            {
                SmartPear smart = currentClient.Controller.SmartPear;
                if (!success)
                {
                    if (smart.ForwarderHttpEnable && smart.DetectorTimeoutEnable)
                    {
                        currentClient.IsSmartForwarderEnable = false;
                        Http.DirectHandle(
                            currentClient, 
                            currentActiveServer.GetAddress(), 
                            currentActiveServer.GetPort(), 
                            currentClient.SmartRequestBuffer);
                        return false;
                    }
                }
                else
                {
                    if (smart.ForwarderHttpEnable && smart.DetectorDnsGrabberEnable
                        && currentActiveServer.UnderlyingSocket != null)
                    {
                        if (currentActiveServer.UnderlyingSocket.RemoteEndPoint != null
                            && smart.DetectorDnsGrabberRegEx.IsMatch(
                                ((IPEndPoint)currentActiveServer.UnderlyingSocket.RemoteEndPoint).Address.ToString()))
                        {
                            currentClient.IsSmartForwarderEnable = false;
                            Http.DirectHandle(
                                currentClient, 
                                currentActiveServer.GetAddress(), 
                                currentActiveServer.GetPort(), 
                                currentClient.SmartRequestBuffer);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// The http data received callback.
        /// </summary>
        /// <param name="currentClient">
        /// The current client.
        /// </param>
        /// <param name="binary">
        /// The binary.
        /// </param>
        /// <param name="currentActiveServer">
        /// The current active server.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool HttpDataReceivedCallback(
            this ProxyClient currentClient, 
            ref byte[] binary, 
            ServerType currentActiveServer)
        {
            SmartPear smart = currentClient.Controller.SmartPear;
            if (smart.ForwarderHttpEnable)
            {
                // If we have Forwarder Enabled
                bool blocked = false;
                if (smart.DetectorStatusHttp
                    && currentClient.SmartResponseBuffer.Length < smart.DetectorHttpMaxBuffering)
                {
                    // If detector is enable and responce is less than buffer size
                    byte[] smartResponseBuffer = currentClient.SmartResponseBuffer;
                    Array.Resize(
                        ref smartResponseBuffer,
                        smartResponseBuffer.Length + binary.Length);
                    Array.Copy(
                        binary, 
                        0,
                        smartResponseBuffer,
                        smartResponseBuffer.Length - binary.Length, 
                        binary.Length);
                    Array.Resize(ref binary, 0);
                    if (smart.DetectorHttpRegEx.IsMatch(Encoding.ASCII.GetString(smartResponseBuffer)))
                    {
                        // If Responce is FILTERED
                        blocked = true;
                    }

                    currentClient.SmartResponseBuffer = smartResponseBuffer;
                }

                if (blocked)
                {
                    currentActiveServer.ReceiveDataDelegate = null;
                    if (currentClient.IsSmartForwarderEnable)
                    {
                        // If client use NoServer
                        byte[] localReqBackup = new byte[currentClient.SmartRequestBuffer.Length];
                        Array.Copy(currentClient.SmartRequestBuffer, localReqBackup, localReqBackup.Length);
                        currentClient.ForwarderClean();
                        currentClient.IsSmartForwarderEnable = false;
                        Http.DirectHandle(
                            currentClient, 
                            currentActiveServer.GetAddress(), 
                            currentActiveServer.GetPort(), 
                            localReqBackup);
                        return false;
                    }
                }
                else
                {
                    // Responce is OK
                    if (!currentClient.IsSmartForwarderEnable
                        && (smart.DetectorStatusHttp || smart.DetectorStatusDnsGrabber || smart.DetectorStatusTimeout))
                    {
                        // If client use Proxy and one of possible detectors is enable
                        Uri url;
                        if (currentClient.RequestAddress != string.Empty
                            && Uri.TryCreate(currentClient.RequestAddress, UriKind.Absolute, out url))
                        {
                            smart.AddRuleToHttpForwarder(
                                "* | *" + url.Host.ToLower().TrimEnd(new[] { '/', '\\' }) + "*");
                                
                                // Bug: I dont have time to solve it and also not importante as if a address is blocked, so other case of that address is also blocked.
                        }

                        currentActiveServer.ReceiveDataDelegate = null;
                    }
                }

                if (currentClient.SmartResponseBuffer.Length > 0
                    && (!currentClient.IsSmartForwarderEnable
                        || currentClient.SmartResponseBuffer.Length >= smart.DetectorHttpMaxBuffering))
                {
                    // If we have any thing in Responce and (Client use Proxy or Client use NoServer but Responce buffer is bigger than buffer)
                    currentClient.ForwarderFlush(smart.ForwarderHttpEnable);
                    currentActiveServer.ReceiveDataDelegate = null;
                }
            }

            return true;
        }

        /// <summary>
        /// The http data sent callback.
        /// </summary>
        /// <param name="currentClient">
        /// The current client.
        /// </param>
        /// <param name="binary">
        /// The binary.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool HttpDataSentCallback(this ProxyClient currentClient, byte[] binary)
        {
            SmartPear smart = currentClient.Controller.SmartPear;
            if (smart.ForwarderHttpEnable && smart.DetectorHttpEnable)
            {
                // If we have Forwarder Enabled
                if (currentClient.IsSmartForwarderEnable)
                {
                    // If Client using NoServer
                    byte[] smartRequestBuffer = currentClient.SmartRequestBuffer;
                    Array.Resize(
                        ref smartRequestBuffer,
                        smartRequestBuffer.Length + binary.Length);
                    Array.Copy(
                        binary, 
                        0,
                        smartRequestBuffer,
                        smartRequestBuffer.Length - binary.Length,
                        binary.Length);
                    currentClient.SmartRequestBuffer = smartRequestBuffer;
                }
                else if (currentClient.SmartResponseBuffer.Length > 0)
                {
                    // If client use Proxy and there is a responce already.
                    currentClient.ForwarderFlush(smart.ForwarderHttpEnable);
                }
            }

            return true;
        }

        /// <summary>
        /// The is need forwarding.
        /// </summary>
        /// <param name="currentClient">
        /// The current client.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsNeedForwarding(this ProxyClient currentClient)
        {
            if (currentClient.RequestAddress == string.Empty)
            {
                return false;
            }

            string name = "Unknown | ";

            ConnectionInfo conInfo = currentClient.GetExtendedInfo();
            if (conInfo != null && conInfo.ProcessString != string.Empty)
                name = conInfo.ProcessString + " | ";

            SmartPear smart = currentClient.Controller.SmartPear;
            string p = currentClient.RequestAddress.ToLower();
            if (p.IndexOf("http://", StringComparison.OrdinalIgnoreCase) == 0)
            {
                p = name + p.Substring(p.IndexOf("://", StringComparison.Ordinal) + 3);
                for (int i = 0; i < smart.ForwarderHttpList.Count; i++)
                    if (Common.IsMatchWildCard(p, smart.ForwarderHttpList[i]))
                        return true;
            }
            else if (p.IndexOf("socks://", StringComparison.OrdinalIgnoreCase) == 0 || p.IndexOf("https://", StringComparison.OrdinalIgnoreCase) == 0)
            {
                p = name + p.Substring(p.IndexOf("://", StringComparison.Ordinal) + 3);
                for (int i = 0; i < smart.ForwarderDirectList.Count; i++)
                    if (Common.IsMatchWildCard(p, smart.ForwarderDirectList[i]))
                        return true;

                if (smart.ForwarderDirectPort80AsHttp && p.IndexOf(":", StringComparison.Ordinal) != -1
                    && p.Substring(p.IndexOf(":", StringComparison.Ordinal) + 1) == "80")
                {
                    p = p.Substring(0, p.IndexOf(":", StringComparison.Ordinal));
                    for (int i = 0; i < smart.ForwarderHttpList.Count; i++)
                        if (Common.IsMatchWildCard(p, smart.ForwarderHttpList[i]))
                            return true;
                }
            }

            return false;
        }

        #endregion
    }
}