// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeaRoxyProtocol.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The pea roxy protocol.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol
{
    #region

    using System;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Threading;

    using PeaRoxy.CommonLibrary;
    using PeaRoxy.CoreProtocol.Compressors;
    using PeaRoxy.CoreProtocol.Cryptors;

    #endregion

    /// <summary>
    /// The pea roxy protocol.
    /// </summary>
    public class PeaRoxyProtocol
    {
        #region Static Fields

        /// <summary>
        /// The rnd.
        /// </summary>
        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        #endregion

        #region Fields

        /// <summary>
        /// The compressor.
        /// </summary>
        private readonly Compressor compressor = new Compressor();

        /// <summary>
        /// The compression type.
        /// </summary>
        private readonly Common.CompressionTypes compressionType = Common.CompressionTypes.None;

        /// <summary>
        /// The encryption type.
        /// </summary>
        private readonly Common.EncryptionTypes encryptionType = Common.EncryptionTypes.None;

        /// <summary>
        /// The pear encryption salt.
        /// </summary>
        private readonly byte[] pearEncryptionSalt = new byte[4];

        /// <summary>
        /// The cryptor.
        /// </summary>
        private Cryptor cryptor = new Cryptor();

        /// <summary>
        /// The encryption key.
        /// </summary>
        private byte[] encryptionKey = new byte[0];

        /// <summary>
        /// The is disconnected.
        /// </summary>
        private bool isDisconnected;

        /// <summary>
        /// The needed bytes.
        /// </summary>
        private int neededBytes;

        /// <summary>
        /// The peer compression type.
        /// </summary>
        private Common.CompressionTypes peerCompressionType = Common.CompressionTypes.None;

        /// <summary>
        /// The peer compressor.
        /// </summary>
        private Compressor peerCompressor = new Compressor();

        /// <summary>
        /// The peer cryptor.
        /// </summary>
        private Cryptor peerCryptor = new Cryptor();

        /// <summary>
        /// The peer encryption type.
        /// </summary>
        private Common.EncryptionTypes peerEncryptionType = Common.EncryptionTypes.None;

        /// <summary>
        /// The waiting buffer.
        /// </summary>
        private byte[] waitingBuffer = new byte[0];

        /// <summary>
        /// The working buffer.
        /// </summary>
        private byte[] workingBuffer = new byte[0];

        /// <summary>
        /// The write buffer.
        /// </summary>
        private byte[] writeBuffer = new byte[0];

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaRoxyProtocol"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="encType">
        /// The enc type.
        /// </param>
        /// <param name="comType">
        /// The com type.
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
            this.ClientSupportedEncryptionType = Common.EncryptionTypes.AllDefaults;
            this.ClientSupportedCompressionType = Common.CompressionTypes.AllDefaults;
            this.UnderlyingSocket = client;
            this.UnderlyingSocket.Blocking = false;
        }

        #endregion

        #region Delegates

        /// <summary>
        /// The close delegate.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        public delegate void CloseDelegate(string message, bool async);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether busy write.
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
        /// Gets or sets the client supported compression type.
        /// </summary>
        public Common.CompressionTypes ClientSupportedCompressionType { get; set; }

        /// <summary>
        /// Gets or sets the client supported encryption type.
        /// </summary>
        public Common.EncryptionTypes ClientSupportedEncryptionType { get; set; }

        /// <summary>
        /// Gets or sets the close callback.
        /// </summary>
        public CloseDelegate CloseCallback { get; set; }

        /// <summary>
        /// Gets or sets the encryption key.
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

                // if (encryptionType == peerEncryptionType)
                // peerCryptor = Cryptor;
                // else
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
        /// Gets the pocket received.
        /// </summary>
        public ushort PocketReceived { get; private set; }

        /// <summary>
        /// Gets the pocket sent.
        /// </summary>
        public ushort PocketSent { get; private set; }

        /// <summary>
        /// Gets or sets the receive packet size.
        /// </summary>
        public int ReceivePacketSize { get; set; }

        /// <summary>
        /// Gets or sets the send packet size.
        /// </summary>
        public int SendPacketSize { get; set; }

        /// <summary>
        /// Gets the underlying socket.
        /// </summary>
        public Socket UnderlyingSocket { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="async">
        /// The async.
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
        /// The read.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
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
        /// The write.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        /// <param name="enc">
        /// The enc.
        /// </param>
        public void Write(byte[] bytes, bool async, bool enc = true)
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

                        if (enc && this.encryptionType != Common.EncryptionTypes.None)
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
            catch (Exception)
            {
                // e)
                this.Close(); // "Protocol 5. " + e.Message);
            }
        }

        /// <summary>
        /// The is data available.
        /// </summary>
        /// <param name="syncWait">
        /// The sync wait.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
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

                                            // if (encryptionType == peerEncryptionType)
                                            // peerCryptor = Cryptor;
                                            // else
                                            if (this.EncryptionKey.Length == 0)
                                            {
                                                this.EncryptionKey = this.pearEncryptionSalt;
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
                this.Close("Protocol 4. " + e.Message);
            }

            return false;
        }

        #endregion
    }
}