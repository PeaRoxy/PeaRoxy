// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpForger.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The http forger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol
{
    #region

    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using PeaRoxy.CommonLibrary;

    #endregion

    /// <summary>
    /// The http forger.
    /// </summary>
    public class HttpForger
    {
        #region Static Fields

        /// <summary>
        /// The rnd.
        /// </summary>
        private static readonly Random Random = new Random();

        #endregion

        #region Fields

        /// <summary>
        /// The header bytes.
        /// </summary>
        public byte[] HeaderBytes = { };

        /// <summary>
        /// The async.
        /// </summary>
        private readonly bool async;

        /// <summary>
        /// The client.
        /// </summary>
        private readonly Socket client;

        /// <summary>
        /// The domain name.
        /// </summary>
        private readonly byte[] domainName;

        /// <summary>
        /// The host string.
        /// </summary>
        private readonly byte[] hostString = Encoding.ASCII.GetBytes("Host: ");

        /// <summary>
        /// The timeout.
        /// </summary>
        private readonly int timeout;

        /// <summary>
        /// The domain pointer.
        /// </summary>
        private int domainPointer;

        /// <summary>
        /// The eof pointer.
        /// </summary>
        private int eofPointer;

        /// <summary>
        /// The h pointer.
        /// </summary>
        private int hPointer;

        /// <summary>
        /// The max buffering.
        /// </summary>
        private int maxBuffering;

        /// <summary>
        /// The timeout counter.
        /// </summary>
        private int timeoutCounter;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpForger"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="domainName">
        /// The domain name.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        /// <param name="maxBuffering">
        /// The max buffering.
        /// </param>
        /// <param name="asyncTimeout">
        /// The async timeout.
        /// </param>
        public HttpForger(
            Socket client, 
            string domainName = "", 
            bool async = false, 
            int maxBuffering = 8192, 
            int asyncTimeout = 60)
        {
            this.domainName = Encoding.ASCII.GetBytes(domainName.ToLower().Trim());
            this.async = async;
            this.timeout = this.timeoutCounter = asyncTimeout * 100;
            this.maxBuffering = maxBuffering;
            this.client = client;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The receive request.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ReceiveRequest()
        {
            bool p;
            return this.ReceiveRequest(out p);
        }

        /// <summary>
        /// The receive request.
        /// </summary>
        /// <param name="noRelated">
        /// The no related.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ReceiveRequest(out bool noRelated)
        {
            byte[] bytes = new byte[1];
            noRelated = false;
            while (((this.async && this.client.Available > 0) || (!this.async)) && this.maxBuffering > 0
                   && this.timeoutCounter > 0)
            {
                if (this.client.Available > 0)
                {
                    this.timeoutCounter = this.timeout;
                    this.maxBuffering--;
                    int i = this.client.Receive(bytes);
                    if (i > 0)
                    {
                        Array.Resize(ref this.HeaderBytes, this.HeaderBytes.Length + 1);
                        this.HeaderBytes[this.HeaderBytes.Length - 1] = bytes[0];
                        if (bytes[0] == 13 || bytes[0] == 10)
                        {
                            this.eofPointer += 1;
                        }
                        else
                        {
                            this.eofPointer = 0;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    if (this.domainName.Count() > 0)
                    {
                        if (this.hPointer == this.hostString.Count())
                        {
                            if (this.domainPointer != this.domainName.Count())
                            {
                                if (bytes[0] == 13 || bytes[0] == 10)
                                {
                                    noRelated = true;
                                    return false;
                                }

                                if (bytes[0] == this.domainName[this.domainPointer])
                                {
                                    this.domainPointer++;
                                }
                                else
                                {
                                    this.domainPointer = 0;
                                }
                            }
                        }
                        else if (bytes[0] == this.hostString[this.hPointer])
                        {
                            this.hPointer++;
                        }
                        else
                        {
                            this.hPointer = 0;
                        }
                    }

                    if (this.eofPointer == 4)
                    {
                        noRelated = this.domainName.Count() > 0 && this.domainPointer != this.domainName.Count();
                        return true;
                    }
                }
                else
                {
                    this.timeoutCounter--;
                    Thread.Sleep(10);
                }
            }

            if (this.maxBuffering <= 0)
            {
                noRelated = true;
            }

            return false;
        }

        /// <summary>
        /// The receive response.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ReceiveResponse()
        {
            byte[] bytes = new byte[1];
            while (((this.async && this.client.Available > 0) || (!this.async)) && this.maxBuffering > 0
                   && this.timeoutCounter > 0)
            {
                if (!Common.IsSocketConnected(this.client))
                {
                    return false;
                }

                if (this.client.Available > 0)
                {
                    this.timeoutCounter = this.timeout;
                    this.maxBuffering--;
                    int i = this.client.Receive(bytes, 1, SocketFlags.None);
                    if (i > 0)
                    {
                        if (bytes[0] == 13 || bytes[0] == 10)
                        {
                            this.eofPointer += 1;
                        }
                        else
                        {
                            this.eofPointer = 0;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    if (this.eofPointer == 4)
                    {
                        return true;
                    }
                }
                else
                {
                    this.timeoutCounter--;
                    Thread.Sleep(10);
                }
            }

            return false;
        }

        /// <summary>
        /// The send request.
        /// </summary>
        /// <param name="hostname">
        /// The hostname.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        public void SendRequest(
            string hostname = "~", 
            string file = "~", 
            string type = "GET", 
            string version = "HTTP/1.1")
        {
            if (file.IndexOf("~", StringComparison.Ordinal) != -1)
            {
                file = file.Replace("~", RandomFilename());
            }

            if (hostname.IndexOf("~", StringComparison.Ordinal) != -1)
            {
                hostname = hostname.Replace("~", RandomHostname());
            }

            string header = type + " " + file + " " + version + "\r\n" + "Host: " + hostname.Trim().ToLower() + "\r\n"
                            + "User-Agent: Mozilla/5.0 (Windows NT 6.1; "
                            + ((Random.Next(0, 1) < 0.5) ? "WOW64; " : string.Empty) + "rv:"
                            + Math.Round((double)Random.Next(3, 12), 1) + ") Gecko/20100101 Firefox/"
                            + Math.Round((double)Random.Next(3, 12), 1) + "\r\n"
                            + "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" + "\r\n"
                            + "Accept-Language: en-us,en;q=0.5" + "\r\n" + "Accept-Encoding: gzip, deflate" + "\r\n"
                            + "Connection: keep-alive" + "\r\n" + "\r\n";
            byte[] byteDateLine = Encoding.ASCII.GetBytes(header.ToCharArray());
            this.client.Send(byteDateLine, byteDateLine.Length, 0);
        }

        /// <summary>
        /// The send response.
        /// </summary>
        /// <param name="code">
        /// The code.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        public void SendResponse(string code = "200 OK", string version = "HTTP/1.1")
        {
            string header = version + " " + code + "\r\n" + "Date: "
                            + DateTime.Now.ToString("ddd, dd MMM yyyy hh\\:mm\\:ss \\G\\M\\T") + "\r\n"
                            + "Server: Apache/1.3." + Math.Round((double)Random.Next(0, 35), 1) + " "
                            + (Random.Next(0, 1) < 0.5 ? "(Unix) (Red-Hat/Linux) " : "(Windows) (Windows 7)") + "\r\n" + "Last-Modified: "
                            + new DateTime(2000, 1, 1).AddMinutes(Random.Next(0, 6835680))
                                  .ToString("ddd, dd MMM yyyy hh\\:mm\\:ss \\G\\M\\T") + "\r\n" + "Accept-Ranges:  none"
                            + "\r\n" + "Content-Type: text/html; charset=UTF-8" + "\r\n" + "Connection: close" + "\r\n"
                            + "\r\n";
            byte[] byteDateLine = Encoding.ASCII.GetBytes(header.ToCharArray());
            this.client.Send(byteDateLine, byteDateLine.Length, 0);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The random filename.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string RandomFilename()
        {
            int r = Random.Next(1, 5);
            string res = string.Empty;
            for (int i = 0; i < r; i++)
            {
                res += "/" + Path.GetRandomFileName();
            }

            return res;
        }

        /// <summary>
        /// The random hostname.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string RandomHostname()
        {
            string[] domainends = { "com", "net", "ir", "org", "info" };
            int r = Random.Next(1, domainends.Length);
            string res = string.Empty;
            int p = (r % 2) + 1;
            for (int i = 0; i < p; i++)
            {
                res += Path.GetRandomFileName().Replace(".", string.Empty);
            }

            return res + "." + domainends[r - 1];
        }

        #endregion
    }
}