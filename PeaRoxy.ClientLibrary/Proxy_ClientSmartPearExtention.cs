using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeaRoxy.CommonLibrary;
using System.Net;
using PeaRoxy.ClientLibrary.Server_Types;
using PeaRoxy.ClientLibrary.Proxy_Types;
namespace PeaRoxy.ClientLibrary
{
    public static class Proxy_ClientSmartPearExtention
    {
        public static bool IsNeedForwarding(this Proxy_Client CurrentClient)
        {
            if (CurrentClient.RequestAddress == string.Empty)
                return false;
            string pName = "Unknown | ";

            Platform.ConnectionInfo conInfo = CurrentClient.GetExtendedInfo();
            if (conInfo != null && conInfo.ProcessString != string.Empty)
                pName = conInfo.ProcessString + " | ";

            SmartPear Smart = CurrentClient.Controller.SmartPear;
            string p = CurrentClient.RequestAddress.ToLower();
            if (p.IndexOf("http://") == 0)
            {
                p = pName + p.Substring(p.IndexOf("://") + 3);
                for (int i = 0; i < Smart.Forwarder_HTTP_List.Count; i++)
                    if (Common.IsMatchWildCard(p, Smart.Forwarder_HTTP_List[i]))
                        return true;
            }
            else if (p.IndexOf("socks://") == 0 || p.IndexOf("https://") == 0)
            {
                p = pName + p.Substring(p.IndexOf("://") + 3);
                for (int i = 0; i < Smart.Forwarder_Direct_List.Count; i++)
                    if (Common.IsMatchWildCard(p, Smart.Forwarder_Direct_List[i]))
                        return true;
                if (Smart.Forwarder_Direct_Port80AsHTTP && p.IndexOf(":") != -1 && p.Substring(p.IndexOf(":") + 1) == "80")
                {
                    p = p.Substring(0, p.IndexOf(":"));
                    for (int i = 0; i < Smart.Forwarder_HTTP_List.Count; i++)
                        if (Common.IsMatchWildCard(p, Smart.Forwarder_HTTP_List[i]))
                            return true;
                }
            }
            return false;
        }

        public static bool HTTP_DataSentCallback(this Proxy_Client CurrentClient, byte[] binery)
        {
            SmartPear Smart = CurrentClient.Controller.SmartPear;
            if (Smart.Forwarder_HTTP_Enable && Smart.Detector_HTTP_Enable) // If we have Forwarder Enabled
            {
                if (CurrentClient.IsSmartForwarderEnable) // If Client using NoServer
                {
                    Array.Resize(ref CurrentClient.SmartRequestBuffer, CurrentClient.SmartRequestBuffer.Length + binery.Length);
                    Array.Copy(binery, 0, CurrentClient.SmartRequestBuffer, CurrentClient.SmartRequestBuffer.Length - binery.Length, binery.Length);
                }
                else if (CurrentClient.SmartResponseBuffer.Length > 0) // If client use Proxy and there is a responce already.
                {
                    CurrentClient.Forwarder_Flush(Smart.Forwarder_HTTP_Enable);
                }
            }
            return true;
        }
        public static bool HTTP_DataReceivedCallback(this Proxy_Client CurrentClient, ref byte[] binery, ServerType CurrentActiveServer)
        {
            SmartPear Smart = CurrentClient.Controller.SmartPear;
            if (Smart.Forwarder_HTTP_Enable) // If we have Forwarder Enabled
            {
                bool blocked = false;
                if (Smart.DetectorStatus_HTTP &&
                    CurrentClient.SmartResponseBuffer.Length < Smart.Detector_HTTP_MaxBuffering) // If detector is enable and responce is less than buffer size
                {
                    Array.Resize(ref CurrentClient.SmartResponseBuffer, CurrentClient.SmartResponseBuffer.Length + binery.Length);
                    Array.Copy(binery, 0, CurrentClient.SmartResponseBuffer, CurrentClient.SmartResponseBuffer.Length - binery.Length, binery.Length);
                    Array.Resize(ref binery, 0);
                    if (Smart.Detector_HTTP_RegEX.IsMatch(System.Text.Encoding.ASCII.GetString(CurrentClient.SmartResponseBuffer))) // If Responce is FILTERED
                        blocked = true;
                }

                if (blocked)
                {
                    CurrentActiveServer.RcvCallback = null;
                    if (CurrentClient.IsSmartForwarderEnable) // If client use NoServer
                    {
                        byte[] localReqBackup = new byte[CurrentClient.SmartRequestBuffer.Length];
                        Array.Copy(CurrentClient.SmartRequestBuffer, localReqBackup, localReqBackup.Length);
                        CurrentClient.Forwarder_Clean();
                        CurrentClient.IsSmartForwarderEnable = false;
                        Proxy_HTTP.DirectHandle(CurrentClient, CurrentActiveServer.GetAddress(), CurrentActiveServer.GetPort(), localReqBackup);
                        return false;
                    }
                }
                else // Responce is OK
                {
                    if (!CurrentClient.IsSmartForwarderEnable && (Smart.DetectorStatus_HTTP || Smart.DetectorStatus_DNSGrabber || Smart.DetectorStatus_Timeout)) // If client use Proxy and one of possible detectors is enable
                    {
                        Uri url;
                        if (CurrentClient.RequestAddress != string.Empty && Uri.TryCreate(CurrentClient.RequestAddress, UriKind.Absolute, out url))
                            Smart.AddRuleTo_HTTP_Forwarder("* | *" + url.Host.ToLower().TrimEnd(new char[] { '/', '\\' }) + "*"); // I know it is a bug, Dont tell me that. But i dont have time to solve it and also not importante as if a address is blocked, so other case of that address is also blocked.
                        CurrentActiveServer.RcvCallback = null;
                    }
                }
                if (CurrentClient.SmartResponseBuffer.Length > 0 &&
                    (!CurrentClient.IsSmartForwarderEnable ||
                    CurrentClient.SmartResponseBuffer.Length >= Smart.Detector_HTTP_MaxBuffering)) // If we have any thing in Responce and (Client use Proxy or Client use NoServer but Responce buffer is bigger than buffer)
                {
                    CurrentClient.Forwarder_Flush(Smart.Forwarder_HTTP_Enable);
                    CurrentActiveServer.RcvCallback = null;
                }
            }
            return true;
        }
        public static bool HTTP_ConnectionStatusCallback(this Proxy_Client CurrentClient, ServerType CurrentActiveServer, bool success)
        {
            if (CurrentClient.IsSmartForwarderEnable)
            {
                SmartPear Smart = CurrentClient.Controller.SmartPear;
                if (!success)
                {
                    if (Smart.Forwarder_HTTP_Enable && Smart.Detector_Timeout_Enable)
                    {
                        CurrentClient.IsSmartForwarderEnable = false;
                        Proxy_HTTP.DirectHandle(CurrentClient, CurrentActiveServer.GetAddress(), CurrentActiveServer.GetPort(), CurrentClient.SmartRequestBuffer);
                        return false;
                    }
                }
                else
                {
                    if (Smart.Forwarder_HTTP_Enable && Smart.Detector_DNSGrabber_Enable && CurrentActiveServer.UnderlyingSocket != null)
                    {
                        if (CurrentActiveServer.UnderlyingSocket.RemoteEndPoint != null && Smart.Detector_DNSGrabber_RegEX.IsMatch(((IPEndPoint)CurrentActiveServer.UnderlyingSocket.RemoteEndPoint).Address.ToString()))
                        {
                            CurrentClient.IsSmartForwarderEnable = false;
                            Proxy_HTTP.DirectHandle(CurrentClient, CurrentActiveServer.GetAddress(), CurrentActiveServer.GetPort(), CurrentClient.SmartRequestBuffer);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static bool Direct_DataSentCallback(this Proxy_Client CurrentClient, byte[] binery, bool isSocks)
        {
            SmartPear Smart = CurrentClient.Controller.SmartPear;
            if (CurrentClient.IsSmartForwarderEnable && Smart.Forwarder_HTTPS_Enable && (!isSocks || Smart.Forwarder_SOCKS_Enable))
            {
                Array.Resize(ref CurrentClient.SmartRequestBuffer, CurrentClient.SmartRequestBuffer.Length + binery.Length);
                Array.Copy(binery, 0, CurrentClient.SmartRequestBuffer, CurrentClient.SmartRequestBuffer.Length - binery.Length, binery.Length);
            }
            else if (CurrentClient.SmartResponseBuffer.Length > 0)
            {
                CurrentClient.Forwarder_Flush(Smart.Forwarder_HTTPS_Enable && (!isSocks || Smart.Forwarder_SOCKS_Enable));
            }
            return true;
        }
        public static bool Direct_DataReceivedCallback(this Proxy_Client CurrentClient, ref byte[] binery, ServerType CurrentActiveServer, bool isSocks)
        {
            SmartPear Smart = CurrentClient.Controller.SmartPear;
            if (Smart.Forwarder_HTTPS_Enable && (!isSocks || Smart.Forwarder_SOCKS_Enable)) // If HTTPS Forwarded
            {
                if (!CurrentClient.IsSmartForwarderEnable && ((Smart.Detector_Direct_Port80AsHTTP && Smart.Forwarder_Direct_Port80AsHTTP) || Smart.DetectorStatus_DNSGrabber || Smart.DetectorStatus_Timeout)) // If using proxy, and forwarder HTTPS is enable
                {
                    Uri url;
                    if (CurrentClient.RequestAddress != string.Empty && Uri.TryCreate(CurrentClient.RequestAddress, UriKind.Absolute, out url))
                        if (Smart.Detector_Direct_Port80AsHTTP && Smart.Forwarder_Direct_Port80AsHTTP && url.Port == 80 && Proxy_HTTP.IsHTTP(CurrentClient.SmartRequestBuffer))
                            Smart.AddRuleTo_HTTP_Forwarder("* | *" + url.Host.ToLower().TrimEnd(new char[] { '/', '\\' }) + "*");
                        else
                            Smart.AddRuleTo_Direct_Forwarder("* | *" + url.Host.ToLower() + ":" + url.Port);
                    CurrentActiveServer.RcvCallback = null;
                    return true;
                }

                if (Smart.Forwarder_Direct_Port80AsHTTP &&
                    Smart.Detector_Direct_Port80AsHTTP &&
                    CurrentClient.RequestAddress.EndsWith(":80") &&
                    Proxy_HTTP.IsHTTP(CurrentClient.SmartRequestBuffer)) // If we have Forwarder Enabled
                {
                    bool blocked = false;
                    if (Smart.DetectorStatus_HTTP &&
                        CurrentClient.SmartResponseBuffer.Length < Smart.Detector_HTTP_MaxBuffering) // If detector is enable and responce is less than buffer size
                    {
                        Array.Resize(ref CurrentClient.SmartResponseBuffer, CurrentClient.SmartResponseBuffer.Length + binery.Length);
                        Array.Copy(binery, 0, CurrentClient.SmartResponseBuffer, CurrentClient.SmartResponseBuffer.Length - binery.Length, binery.Length);
                        Array.Resize(ref binery, 0);
                        if (Smart.Detector_HTTP_RegEX.IsMatch(System.Text.Encoding.ASCII.GetString(CurrentClient.SmartResponseBuffer))) // If Responce is FILTERED
                            blocked = true;
                    }

                    if (blocked)
                    {
                        CurrentActiveServer.RcvCallback = null;
                        if (CurrentClient.IsSmartForwarderEnable) // If client use NoServer
                        {
                            byte[] localReqBackup = new byte[CurrentClient.SmartRequestBuffer.Length];
                            Array.Copy(CurrentClient.SmartRequestBuffer, localReqBackup, localReqBackup.Length);
                            CurrentClient.Forwarder_Clean();
                            CurrentClient.IsSmartForwarderEnable = false;
                            Proxy_HTTP.DirectHandle(CurrentClient, CurrentActiveServer.GetAddress(), CurrentActiveServer.GetPort(), localReqBackup);
                            return false;
                        }
                    }
                    else // Responce is OK
                    {
                        if (!CurrentClient.IsSmartForwarderEnable && (Smart.DetectorStatus_HTTP || Smart.DetectorStatus_DNSGrabber || Smart.DetectorStatus_Timeout)) // If client use Proxy and one of possible detectors is enable
                        {
                            Uri url;
                            if (CurrentClient.RequestAddress != string.Empty && Uri.TryCreate(CurrentClient.RequestAddress, UriKind.Absolute, out url))
                                Smart.AddRuleTo_HTTP_Forwarder("* | *" + url.Host.ToLower().TrimEnd(new char[] { '/', '\\' }) + "*"); // I know it is a bug, Dont tell me that. But i dont have time to solve it and also not importante as if a address is blocked, so other case of that address is also blocked.
                            CurrentActiveServer.RcvCallback = null;
                        }
                    }
                    if (CurrentClient.SmartResponseBuffer.Length > 0 &&
                        (!CurrentClient.IsSmartForwarderEnable ||
                        CurrentClient.SmartResponseBuffer.Length >= Smart.Detector_HTTP_MaxBuffering)) // If we have any thing in Responce and (Client use Proxy or Client use NoServer but Responce buffer is bigger than buffer)
                    {
                        CurrentClient.Forwarder_Flush(Smart.Forwarder_HTTP_Enable);
                        CurrentActiveServer.RcvCallback = null;
                    }
                }
            }
            else
            {
                CurrentActiveServer.RcvCallback = null;
            }
            return true;
        }
        public static bool Direct_ConnectionStatusCallback(this Proxy_Client CurrentClient, ServerType CurrentActiveServer, bool success, bool isSocks)
        {
            if (CurrentClient.IsSmartForwarderEnable)
            {
                SmartPear Smart = CurrentClient.Controller.SmartPear;
                if (!success)
                {
                    if (Smart.Forwarder_HTTPS_Enable && (!isSocks || Smart.Forwarder_SOCKS_Enable) && Smart.Detector_Timeout_Enable)
                    {
                        CurrentClient.IsSmartForwarderEnable = false;
                        if (isSocks)
                            Proxy_SOCKS.DirectHandle(CurrentClient, CurrentActiveServer.GetAddress(), CurrentActiveServer.GetPort(), CurrentClient.SmartRequestBuffer);
                        else
                            Proxy_HTTPS.DirectHandle(CurrentClient, CurrentActiveServer.GetAddress(), CurrentActiveServer.GetPort(), CurrentClient.SmartRequestBuffer);
                        return false;
                    }
                }
                else
                {
                    if (Smart.Forwarder_HTTPS_Enable && (!isSocks || Smart.Forwarder_SOCKS_Enable) && Smart.Detector_DNSGrabber_Enable && CurrentActiveServer.UnderlyingSocket != null)
                    {
                        if (CurrentActiveServer.UnderlyingSocket.RemoteEndPoint != null && Smart.Detector_DNSGrabber_RegEX.IsMatch(((IPEndPoint)CurrentActiveServer.UnderlyingSocket.RemoteEndPoint).Address.ToString()))
                        {
                            CurrentClient.IsSmartForwarderEnable = false;
                            if (isSocks)
                                Proxy_SOCKS.DirectHandle(CurrentClient, CurrentActiveServer.GetAddress(), CurrentActiveServer.GetPort(), CurrentClient.SmartRequestBuffer);
                            else
                                Proxy_HTTPS.DirectHandle(CurrentClient, CurrentActiveServer.GetAddress(), CurrentActiveServer.GetPort(), CurrentClient.SmartRequestBuffer);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void Forwarder_Clean(this Proxy_Client thisclient, Nullable<bool> enable = null)
        {
            thisclient.IsSmartForwarderEnable = ((enable == null) ? thisclient.Controller.SmartPear.Forwarder_HTTP_Enable : (bool)enable);
            Array.Resize(ref thisclient.SmartResponseBuffer, 0);
            Array.Resize(ref thisclient.SmartRequestBuffer, 0);
        }
        public static void Forwarder_Flush(this Proxy_Client thisclient, bool enable)
        {
            thisclient.Write(thisclient.SmartResponseBuffer);
            thisclient.Forwarder_Clean(enable);
            thisclient.IsSmartForwarderEnable = false;
        }
    }
}
