// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpForger.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using PeaRoxy.CommonLibrary;

    /// <summary>
    ///     The HttpForger class is responsible for generating and detecting of fake HTTP requests as a way for hiding the real
    ///     content of request
    /// </summary>
    public class HttpForger
    {
        private static readonly Random Random = new Random();

        private readonly bool async;

        private readonly Socket client;

        private readonly byte[] domainBytes;

        private readonly bool domainSearch;

        private readonly int maxBuffering;

        private readonly int timeout;

        private int bufferingCounter;

        private int domainPointer;

        private int eofPointer;

        private byte[] headerBytes = { };

        private int timeoutCounter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpForger" /> class.
        /// </summary>
        /// <param name="client">
        ///     The connection's socket.
        /// </param>
        /// <param name="domainName">
        ///     The domain name to be used to detect or generate the request.
        /// </param>
        /// <param name="async">
        ///     This value indicates if we should write and read from the socket asynchronously.
        /// </param>
        /// <param name="maxBuffering">
        ///     The maximum buffering size.
        /// </param>
        /// <param name="asyncTimeout">
        ///     The timeout value for asynchronous operations.
        /// </param>
        public HttpForger(
            Socket client,
            string domainName = "",
            bool async = false,
            int maxBuffering = 8192,
            int asyncTimeout = 60)
        {
            if (!string.IsNullOrWhiteSpace(domainName))
            {
                this.domainSearch = true;
                this.domainBytes = Encoding.ASCII.GetBytes("\r\nHost: " + domainName.ToLower().Trim() + "\r\n");
            }
            this.async = async;
            this.timeout = this.timeoutCounter = asyncTimeout * 100;
            this.maxBuffering = maxBuffering;
            this.headerBytes = new byte[0];
            this.client = client;
        }

        /// <summary>
        ///     Gets the request's header in form of byte[]
        /// </summary>
        public byte[] HeaderBytes
        {
            get
            {
                return this.headerBytes;
            }
        }

        /// <summary>
        ///     The ReceiveRequest method will detect the HTTP request and remove it from the stream. So we can read the real
        ///     content of request.
        /// </summary>
        /// <returns>
        ///     The return value indicates if the request is a HTTP request and has the correct domain name.
        /// </returns>
        public bool ReceiveRequest()
        {
            byte[] bytes = new byte[1];
            this.bufferingCounter = this.maxBuffering;
            Array.Resize(ref this.headerBytes, this.maxBuffering);
            while (((this.async && this.client.Available > 0) || (!this.async)) && this.bufferingCounter > 0
                   && this.timeoutCounter > 0)
            {
                if (!Common.IsSocketConnected(this.client))
                {
                    return false;
                }

                if (this.client.Available > 0)
                {
                    this.timeoutCounter = this.timeout;
                    this.bufferingCounter--;
                    int i = this.client.Receive(bytes);
                    if (i > 0)
                    {
                        this.headerBytes[this.headerBytes.Length - (this.bufferingCounter + 1)] = bytes[0];
                        if (bytes[0] == ((this.eofPointer % 2 == 0) ? 13 : 10))
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
                        Array.Resize(ref this.headerBytes, this.headerBytes.Length - this.bufferingCounter);
                        return false;
                    }

                    if (this.domainSearch)
                    {
                        if (this.domainPointer < this.domainBytes.Count())
                        {
                            if (bytes[0] == this.domainBytes[this.domainPointer])
                            {
                                this.domainPointer++;
                            }
                            else
                            {
                                this.domainPointer = 0;
                            }
                        }
                    }

                    if (this.eofPointer == 4)
                    {
                        Array.Resize(ref this.headerBytes, this.headerBytes.Length - this.bufferingCounter);
                        return !this.domainSearch || this.domainPointer == this.domainBytes.Count();
                    }
                }
                else
                {
                    this.timeoutCounter--;
                    Thread.Sleep(10);
                }
            }
            Array.Resize(ref this.headerBytes, this.headerBytes.Length - this.bufferingCounter);
            return false;
        }

        /// <summary>
        ///     The ReceiveResponse method remove the unnecessary fake HTTP response to our fake HTTP request.
        /// </summary>
        /// <returns>
        ///     The return value indicates if the operation ended successfully.
        /// </returns>
        public bool ReceiveResponse()
        {
            byte[] bytes = new byte[1];
            this.bufferingCounter = this.maxBuffering;
            while (((this.async && this.client.Available > 0) || (!this.async)) && this.bufferingCounter > 0
                   && this.timeoutCounter > 0)
            {
                if (!Common.IsSocketConnected(this.client))
                {
                    return false;
                }

                if (this.client.Available > 0)
                {
                    this.timeoutCounter = this.timeout;
                    this.bufferingCounter--;
                    int i = this.client.Receive(bytes);
                    if (i > 0)
                    {
                        if (bytes[0] == ((this.eofPointer % 2 == 0) ? 13 : 10))
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
        ///     This method will generate a fake HTTP request
        /// </summary>
        /// <param name="hostname">
        ///     The hostname of the request.
        /// </param>
        /// <param name="file">
        ///     The file address of the request.
        /// </param>
        /// <param name="type">
        ///     The type of HTTP request.
        /// </param>
        /// <param name="version">
        ///     The version of the HTTP protocol.
        /// </param>
        public void SendRequest(
            string hostname = "~",
            string file = "~",
            string type = "GET",
            string version = "HTTP/1.1")
        {
            if (file.Contains("~"))
            {
                file = file.Replace("~", RandomFilename());
            }

            if (hostname.Contains("~"))
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
            byte[] byteDateLine = Encoding.ASCII.GetBytes(header);
            this.client.Send(byteDateLine, byteDateLine.Length, 0);
        }

        /// <summary>
        ///     This method will generate a fake HTTP response
        /// </summary>
        /// <param name="code">
        ///     The HTTP status code.
        /// </param>
        /// <param name="version">
        ///     The version of the HTTP protocol.
        /// </param>
        public void SendResponse(string code = "200 OK", string version = "HTTP/1.1")
        {
            string header = version + " " + code + "\r\n" + "Date: "
                            + DateTime.Now.ToString("ddd, dd MMM yyyy hh\\:mm\\:ss \\G\\M\\T") + "\r\n"
                            + "Server: Apache/1.3." + Math.Round((double)Random.Next(0, 35), 1) + " "
                            + (Random.Next(0, 1) < 0.5 ? "(Unix) (Red-Hat/Linux) " : "(Windows) (Windows 7)") + "\r\n"
                            + "Last-Modified: "
                            + new DateTime(2000, 1, 1).AddMinutes(Random.Next(0, 6835680))
                                  .ToString("ddd, dd MMM yyyy hh\\:mm\\:ss \\G\\M\\T") + "\r\n" + "Accept-Ranges:  none"
                            + "\r\n" + "Content-Type: text/html; charset=UTF-8" + "\r\n" + "Connection: close" + "\r\n"
                            + "\r\n";
            byte[] byteDateLine = Encoding.ASCII.GetBytes(header);
            this.client.Send(byteDateLine, byteDateLine.Length, 0);
        }

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

        private static string RandomHostname()
        {
            string[] domainends = { "com", "net", "ir", "org", "info", "uk", "us", "de", "ru" };
            int r = Random.Next(1, domainends.Length);
            string res = string.Empty;
            int p = (r % 2) + 1;
            for (int i = 0; i < p; i++)
            {
                res += Path.GetRandomFileName().Replace(".", string.Empty);
            }

            return res + "." + domainends[r - 1];
        }
    }
}