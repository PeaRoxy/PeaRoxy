// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeaRoxyProtocol.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol
{
    using System;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Threading;

    using PeaRoxy.CommonLibrary;
    using PeaRoxy.CoreProtocol.Compressors;
    using PeaRoxy.CoreProtocol.Cryptors;

    /// <summary>
    ///     The PeaRoxy Protocol is responsible for sending and receiving of data between two instances of it-self in correct
    ///     order and with specified encryption and compression algorithms
    /// </summary>
    public class PeaRoxyProtocol
    {
        public delegate void CloseDelegate(string message, bool async);

        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        private readonly Common.CompressionTypes compressionType = Common.CompressionTypes.None;

        private readonly Compressor compressor = new Compressor();

        private readonly Common.EncryptionTypes encryptionType = Common.EncryptionTypes.None;

        private readonly byte[] pearEncryptionSalt = new byte[4];

        private Cryptor cryptor = new Cryptor();

        private byte[] encryptionKey = new byte[0];

        private bool isDisconnected;

        private int neededBytes;

        private Common.CompressionTypes peerCompressionType = Common.CompressionTypes.None;

        private Compressor peerCompressor = new Compressor();

        private Cryptor peerCryptor = new Cryptor();

        private Common.EncryptionTypes peerEncryptionType = Common.EncryptionTypes.None;

        private byte[] waitingBuffer = new byte[0];

        private byte[] workingBuffer = new byte[0];

        private byte[] writeBuffer = new byte[0];

        /// <summary>
        ///     Initializes a new instance of the <see cref="PeaRoxyProtocol" /> class.
        /// </summary>
        /// <param name="client">
        ///     The underlying socket.
        /// </param>
        /// <param name="encType">
        ///     The encryption type.
        /// </param>
        /// <param name="comType">
        ///     The compression type.
        /// </param>
        public PeaRoxyProtocol(
            Socket client,
            Common.EncryptionTypes encType = Common.EncryptionTypes.None,
            Common.CompressionTypes comType = Common.CompressionTypes.None)
        {
            this.encryptionType = encType;
            this.compressionType = comType;
            switch (this.compressionType)
            {
                case Common.CompressionTypes.GZip:
                    this.compressor = new GZipCompressor();
                    break;
                case Common.CompressionTypes.Deflate:
                    this.compressor = new DeflateCompressor();
                    break;
            }

            this.ReceivePacketSize = 8192;
            this.SendPacketSize = 8192;
            this.PocketSent = 0;
            this.PocketReceived = 0;
            this.ClientSupportedEncryptionType = Common.EncryptionTypes.None | Common.EncryptionTypes.SimpleXor | Common.EncryptionTypes.TripleDes;
            this.ClientSupportedCompressionType = Common.CompressionTypes.None | Common.CompressionTypes.GZip | Common.CompressionTypes.Deflate;
            this.UnderlyingSocket = client;
            this.UnderlyingSocket.Blocking = false;
        }

        /// <summary>
        ///     Gets a value indicating whether we are busy writing.
        /// </summary>
        public bool BusyWrite
        {
            get
            {
                if (this.writeBuffer.Length > 0)
                {
                    this.Write(null, true);
                }

                return this.writeBuffer.Length > 0;
            }
        }

        /// <summary>
        ///     Gets or sets the supported compression types
        /// </summary>
        public Common.CompressionTypes ClientSupportedCompressionType { get; set; }

        /// <summary>
        ///     Gets or sets the supported encryption types
        /// </summary>
        public Common.EncryptionTypes ClientSupportedEncryptionType { get; set; }

        /// <summary>
        ///     Gets or sets the close callback which will be executed when we receive a close request or decide it our-self
        /// </summary>
        public CloseDelegate CloseCallback { get; set; }

        /// <summary>
        ///     Gets or sets the encryption key in form of byte[].
        /// </summary>
        public byte[] EncryptionKey
        {
            get
            {
                return this.encryptionKey;
            }

            set
            {
                if (this.encryptionKey == value)
                {
                    return;
                }

                this.encryptionKey = value;
                switch (this.encryptionType)
                {
                    case Common.EncryptionTypes.TripleDes:
                        this.cryptor = new TripleDesCryptor(this.encryptionKey);
                        break;
                    case Common.EncryptionTypes.SimpleXor:
                        this.cryptor = new SimpleXorCryptor(this.encryptionKey);
                        break;
                }

                switch (this.peerEncryptionType)
                {
                    case Common.EncryptionTypes.TripleDes:
                        this.peerCryptor = new TripleDesCryptor(this.encryptionKey);
                        break;
                    case Common.EncryptionTypes.SimpleXor:
                        this.peerCryptor = new SimpleXorCryptor(this.encryptionKey);
                        break;
                }
            }
        }

        /// <summary>
        ///     Gets the pockets received till now.
        /// </summary>
        public ushort PocketReceived { get; private set; }

        /// <summary>
        ///     Gets the pocket sent till now.
        /// </summary>
        public ushort PocketSent { get; private set; }

        /// <summary>
        ///     Gets or sets the maximum receive packet size.
        /// </summary>
        public int ReceivePacketSize { get; set; }

        /// <summary>
        ///     Gets or sets the maximum send packet size.
        /// </summary>
        public int SendPacketSize { get; set; }

        /// <summary>
        ///     Gets the underlying socket.
        /// </summary>
        public Socket UnderlyingSocket { get; private set; }

        /// <summary>
        ///     The close method which supports mentioning a message about the reason
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="async">
        ///     Indicating if the closing process should treat the client as an asynchronous client
        /// </param>
        public void Close(string message = null, bool async = false)
        {
            try
            {
                if (this.isDisconnected)
                {
                    return;
                }

                this.isDisconnected = true;

                if (this.CloseCallback != null)
                {
                    this.CloseCallback(message, async);
                }

                if (async)
                {
                    byte[] db = new byte[0];
                    if (this.UnderlyingSocket != null)
                    {
                        this.UnderlyingSocket.BeginSend(
                            db,
                            0,
                            db.Length,
                            SocketFlags.None,
                            delegate(IAsyncResult ar)
                                {
                                    try
                                    {
                                        this.UnderlyingSocket.Close(); // Close request connection it-self
                                        this.UnderlyingSocket.EndSend(ar);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                },
                            null);
                    }
                }
                else
                {
                    if (this.UnderlyingSocket != null)
                    {
                        this.UnderlyingSocket.Close();
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        ///     The read method to read the data from the other end
        /// </summary>
        /// <returns>
        ///     Received data in form of
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        /// </returns>
        public byte[] Read()
        {
            if (this.IsDataAvailable(true))
            {
                byte[] cop = (byte[])this.waitingBuffer.Clone();
                this.waitingBuffer = new byte[0];
                return cop;
            }

            return null;
        }

        /// <summary>
        ///     The write method to write the data to the other end.
        /// </summary>
        /// <param name="bytes">
        ///     The data to write.
        /// </param>
        /// <param name="async">
        ///     Indicating if the writing process should treat the client as an asynchronous client
        /// </param>
        /// <param name="encryption">
        ///     Indicate if writing process should encrypt the data before sending
        /// </param>
        public void Write(byte[] bytes, bool async, bool encryption = true)
        {
            try
            {
                if (bytes != null)
                {
                    int peaces = (int)Math.Ceiling(bytes.Length / (double)this.SendPacketSize);
                    for (int i = 0; i < peaces; i++)
                    {
                        int len = Math.Min(this.SendPacketSize, bytes.Length - (i * this.SendPacketSize));
                        byte[] framingBody = new byte[len];
                        Array.Copy(bytes, i * this.SendPacketSize, framingBody, 0, len);
                        if (this.PocketSent == 65535)
                        {
                            this.PocketSent = 0;
                        }

                        byte[] framingHeader = new byte[10];

                        framingHeader[1] = (byte)this.compressionType;
                        framingBody = this.compressor.Compress(framingBody);

                        if (encryption && this.encryptionType != Common.EncryptionTypes.None)
                        {
                            // If encryption is enable
                            byte[] encryptionSalt = new byte[4];
                            Random.GetNonZeroBytes(encryptionSalt);
                            Array.Copy(encryptionSalt, 0, framingHeader, 6, 4);

                            if (this.EncryptionKey.Length == 0)
                            {
                                this.EncryptionKey = encryptionSalt;
                            }

                            framingHeader[0] = (byte)this.encryptionType;
                            this.cryptor.SetSalt(encryptionSalt);
                            framingBody = this.cryptor.Encrypt(framingBody);
                        }

                        framingHeader[2] = (byte)Math.Floor((double)this.PocketSent / 256);
                        framingHeader[3] = (byte)(this.PocketSent % 256);
                        framingHeader[4] = (byte)Math.Floor((double)framingBody.Length / 256);
                        framingHeader[5] = (byte)(framingBody.Length % 256);
                        Array.Resize(
                            ref this.writeBuffer,
                            this.writeBuffer.Length + framingHeader.Length + framingBody.Length);
                        Array.Copy(
                            framingHeader,
                            0,
                            this.writeBuffer,
                            this.writeBuffer.Length - (framingHeader.Length + framingBody.Length),
                            framingHeader.Length);
                        Array.Copy(
                            framingBody,
                            0,
                            this.writeBuffer,
                            this.writeBuffer.Length - framingBody.Length,
                            framingBody.Length);
                        this.PocketSent++;
                    }
                }

                if (this.writeBuffer.Length > 0 && this.UnderlyingSocket.Poll(0, SelectMode.SelectWrite))
                {
                    int bytesWritten = this.UnderlyingSocket.Send(this.writeBuffer, SocketFlags.None);
                    Array.Copy(
                        this.writeBuffer,
                        bytesWritten,
                        this.writeBuffer,
                        0,
                        this.writeBuffer.Length - bytesWritten);
                    Array.Resize(ref this.writeBuffer, this.writeBuffer.Length - bytesWritten);
                    if (!async && this.writeBuffer.Length > 0)
                    {
                        this.Write(null, false);
                    }
                }
            }
            catch
            {
                this.Close();
            }
        }

        /// <summary>
        ///     This method let us know if there is new data available to read from the underlying client. Should be executed
        ///     repeatedly.
        /// </summary>
        /// <param name="syncWait">
        ///     Indicates if we should wait until we have something to read.
        /// </param>
        /// <returns>
        ///     Indicates if we have new data available to read.
        /// </returns>
        public bool IsDataAvailable(bool syncWait = false)
        {
            try
            {
                if ((this.UnderlyingSocket.Available > 0 || syncWait) && this.waitingBuffer.Length == 0)
                {
                    int i = 10;

                    // Time out by second, This timeout is about the max seconds that we wait for something if we don't have anything
                    i = i * 1000;
                    int i2 = i;
                    byte[] buffer = new byte[0];
                    while (i > 0)
                    {
                        if (!Common.IsSocketConnected(this.UnderlyingSocket))
                        {
                            this.Close();
                            return false;
                        }

                        if (this.UnderlyingSocket.Available > 0)
                        {
                            i = i2;
                            int bufferLastLength = buffer.Length;
                            Array.Resize(ref buffer, bufferLastLength + this.ReceivePacketSize);
                            int bytes = this.UnderlyingSocket.Receive(
                                buffer,
                                bufferLastLength,
                                buffer.Length - bufferLastLength,
                                SocketFlags.None);
                            if (bytes == 0)
                            {
                                this.Close();
                                return false;
                            }

                            Array.Resize(ref buffer, bytes + bufferLastLength);
                            bool doNext = true;
                            while (doNext)
                            {
                                doNext = false;
                                if (this.neededBytes == 0)
                                {
                                    if (this.workingBuffer.Length > 0)
                                    {
                                        if (!this.ClientSupportedEncryptionType.HasFlag(this.peerEncryptionType))
                                        {
                                            // Check if we support this type of encryption
                                            this.Close("Protocol 1. Unsupported encryption type.");
                                            return false;
                                        }

                                        if (this.peerEncryptionType != Common.EncryptionTypes.None)
                                        {
                                            this.peerCryptor.SetSalt(this.pearEncryptionSalt);
                                            this.workingBuffer = this.peerCryptor.Decrypt(this.workingBuffer);
                                        }

                                        if (!this.ClientSupportedCompressionType.HasFlag(this.peerCompressionType))
                                        {
                                            // Check if we support this type of compression
                                            this.Close("Protocol 2. Unsupported compression type.");
                                            return false;
                                        }

                                        this.workingBuffer = this.peerCompressor.Decompress(this.workingBuffer);

                                        Array.Resize(
                                            ref this.waitingBuffer,
                                            this.waitingBuffer.Length + this.workingBuffer.Length);
                                        Array.Copy(
                                            this.workingBuffer,
                                            0,
                                            this.waitingBuffer,
                                            this.waitingBuffer.Length - this.workingBuffer.Length,
                                            this.workingBuffer.Length);
                                        this.workingBuffer = new byte[0];
                                    }

                                    if (buffer.Length >= 10)
                                    {
                                        if (this.PocketReceived == 65535)
                                        {
                                            this.PocketReceived = 0;
                                        }

                                        if ((Common.CompressionTypes)buffer[1] != this.peerCompressionType)
                                        {
                                            this.peerCompressionType = (Common.CompressionTypes)buffer[1];
                                            switch (this.peerCompressionType)
                                            {
                                                case Common.CompressionTypes.GZip:
                                                    this.peerCompressor = new GZipCompressor();
                                                    break;
                                                case Common.CompressionTypes.Deflate:
                                                    this.peerCompressor = new DeflateCompressor();
                                                    break;
                                            }
                                        }

                                        int receiveCounter = (buffer[2] * 256) + buffer[3];
                                        this.neededBytes = (buffer[4] * 256) + buffer[5];
                                        Array.Copy(buffer, 6, this.pearEncryptionSalt, 0, 4);
                                        if ((Common.EncryptionTypes)buffer[0] != this.peerEncryptionType)
                                        {
                                            this.peerEncryptionType = (Common.EncryptionTypes)buffer[0];

                                            switch (this.peerEncryptionType)
                                            {
                                                case Common.EncryptionTypes.TripleDes:
                                                    if (this.EncryptionKey.Length == 0)
                                                    {
                                                        this.EncryptionKey = this.pearEncryptionSalt;
                                                    }
                                                    this.peerCryptor = new TripleDesCryptor(this.encryptionKey);
                                                    break;
                                                case Common.EncryptionTypes.SimpleXor:
                                                    if (this.EncryptionKey.Length == 0)
                                                    {
                                                        this.EncryptionKey = this.pearEncryptionSalt;
                                                    }
                                                    this.peerCryptor = new SimpleXorCryptor(this.encryptionKey);
                                                    break;
                                            }
                                        }

                                        if (receiveCounter != this.PocketReceived)
                                        {
                                            this.Close(
                                                "Protocol 3. Packet lost, Last received packet: " + receiveCounter
                                                + ", Expected: " + this.PocketReceived);
                                            return false;
                                        }

                                        this.PocketReceived++;
                                        if (this.neededBytes <= buffer.Length - 10)
                                        {
                                            Array.Resize(
                                                ref this.workingBuffer,
                                                this.workingBuffer.Length + this.neededBytes);
                                            Array.Copy(buffer, 10, this.workingBuffer, 0, this.neededBytes);

                                            Array.Copy(
                                                buffer,
                                                10 + this.neededBytes,
                                                buffer,
                                                0,
                                                (buffer.Length - 10) - this.neededBytes);
                                            Array.Resize(ref buffer, (buffer.Length - 10) - this.neededBytes);
                                            this.neededBytes = 0;
                                            doNext = true;
                                        }
                                        else
                                        {
                                            Array.Resize(
                                                ref this.workingBuffer,
                                                this.workingBuffer.Length + (buffer.Length - 10));
                                            Array.Copy(buffer, 10, this.workingBuffer, 0, buffer.Length - 10);
                                            this.neededBytes -= buffer.Length - 10;
                                            buffer = new byte[0];
                                        }
                                    }
                                }
                                else
                                {
                                    if (buffer.Length > 0)
                                    {
                                        if (this.neededBytes <= buffer.Length)
                                        {
                                            Array.Resize(
                                                ref this.workingBuffer,
                                                this.workingBuffer.Length + this.neededBytes);
                                            Array.Copy(
                                                buffer,
                                                0,
                                                this.workingBuffer,
                                                this.workingBuffer.Length - this.neededBytes,
                                                this.neededBytes);

                                            Array.Copy(
                                                buffer,
                                                this.neededBytes,
                                                buffer,
                                                0,
                                                buffer.Length - this.neededBytes);
                                            Array.Resize(ref buffer, buffer.Length - this.neededBytes);
                                            this.neededBytes = 0;
                                            doNext = true;
                                        }
                                        else
                                        {
                                            Array.Resize(
                                                ref this.workingBuffer,
                                                this.workingBuffer.Length + buffer.Length);
                                            Array.Copy(
                                                buffer,
                                                0,
                                                this.workingBuffer,
                                                this.workingBuffer.Length - buffer.Length,
                                                buffer.Length);
                                            this.neededBytes -= buffer.Length;
                                            buffer = new byte[0];
                                        }
                                    }
                                }
                            }
                        }

                        if (buffer.Length == 0 && (this.waitingBuffer.Length > 0 || !syncWait))
                        {
                            return this.waitingBuffer.Length > 0;
                        }

                        if (i != i2)
                        {
                            Thread.Sleep(1);
                        }

                        i--;
                    }
                }

                return this.waitingBuffer.Length > 0;
            }
            catch (Exception e)
            {
                this.Close("Protocol 4. " + e);
            }

            return false;
        }
    }
}