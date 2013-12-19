using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using PeaRoxy.CommonLibrary;
using System.Security.Cryptography;
namespace PeaRoxy.CoreProtocol
{
    public class PeaRoxyProtocol
    {
        public delegate void CloseDelegate(string message, bool async);
        private static RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
        private bool isDC = false;
        private byte[] pearEncryptionSalt = new byte[4];
        private byte[] encryptionKey = new byte[0];
        private byte[] waitingBuffer = new byte[0];
        private byte[] workingBuffer = new byte[0];
        private byte[] writeBuffer = new byte[0];
        private Cryptors.Cryptor Cryptor = new Cryptors.Cryptor();
        private Cryptors.Cryptor peerCryptor = new Cryptors.Cryptor();
        private Compressors.Compressor Compressor = new Compressors.Compressor();
        private Compressors.Compressor peerCompressor = new Compressors.Compressor();
        private int neededBytes = 0;
        private Common.Encryption_Type peerEncryptionType = Common.Encryption_Type.None;
        private Common.Compression_Type peerCompressionType = Common.Compression_Type.None;
        private Common.Compression_Type compressionType = Common.Compression_Type.None;
        private Common.Encryption_Type encryptionType = Common.Encryption_Type.None;
        public Socket UnderlyingSocket { get;private set; }
        public CloseDelegate CloseCallback { get; set; }
        public int ReceivePacketSize { get; set; }
        public int SendPacketSize { get; set; }
        public Common.Encryption_Type ClientSupportedEncryptionType { get; set; }
        public Common.Compression_Type ClientSupportedCompressionType { get; set; }
        public ushort PocketSent { get; private set; }
        public ushort PocketRecieved { get; private set; }
        public byte[] EncryptionKey
        {
            get { return this.encryptionKey; }
            set
            {
                if (this.encryptionKey == value)
                    return;
                this.encryptionKey = value;
                switch (encryptionType)
                {
                    case Common.Encryption_Type.TripleDES:
                        Cryptor = new Cryptors.TripleDESCryptor(encryptionKey);
                        break;
                    case Common.Encryption_Type.SimpleXOR:
                        Cryptor = new Cryptors.SimpleXORCryptor(encryptionKey);
                        break;
                }
                //if (encryptionType == peerEncryptionType)
                //    peerCryptor = Cryptor;
                //else
                switch (peerEncryptionType)
                {
                    case Common.Encryption_Type.TripleDES:
                        peerCryptor = new Cryptors.TripleDESCryptor(encryptionKey);
                        break;
                    case Common.Encryption_Type.SimpleXOR:
                        peerCryptor = new Cryptors.SimpleXORCryptor(encryptionKey);
                        break;
                }
            }
        }
        public bool BusyWrite
        {
            get
            {
                if (writeBuffer.Length > 0)
                {
                    this.Write(null, true);
                }
                return (writeBuffer.Length > 0);
            }
        }
        public PeaRoxyProtocol(Socket client, Common.Encryption_Type encType = Common.Encryption_Type.None, Common.Compression_Type comType = Common.Compression_Type.None)
        {
            this.encryptionType = encType;
            this.compressionType = comType;
            switch (compressionType)
            {
                case Common.Compression_Type.gZip:
                    Compressor = new Compressors.gZipCompressor();
                    break;
                case Common.Compression_Type.Deflate:
                    Compressor = new Compressors.DeflateCompressor();
                    break;
            }
            this.ReceivePacketSize = 8192;
            this.SendPacketSize = 8192;
            this.PocketSent = 0;
            this.PocketRecieved = 0;
            this.ClientSupportedEncryptionType = Common.Encryption_Type.Anything;
            this.ClientSupportedCompressionType = Common.Compression_Type.Anything;
            this.UnderlyingSocket = client;
            this.UnderlyingSocket.Blocking = false;
        }

        public bool isDataAvailable(bool SyncWait = false)
        {
            try
            {
                if ((UnderlyingSocket.Available > 0 || SyncWait) && waitingBuffer.Length == 0)
                {
                    int i = 10; // Time out by second, This timeout is about the max seconds that we wait for something if we don't have anything
                    i = i * 1000;
                    int _i = i; // Save timeout value
                    byte[] buffer = new byte[0];
                    while (i > 0)
                    {
                        if (!Common.IsSocketConnected(UnderlyingSocket))
                        {
                            Close();
                            return false;
                        }
                        if (UnderlyingSocket.Available > 0)
                        {
                            i = _i;
                            int bufferLastLength = buffer.Length;
                            Array.Resize(ref buffer, bufferLastLength + ReceivePacketSize);
                            int bytes = UnderlyingSocket.Receive(buffer, bufferLastLength, buffer.Length - bufferLastLength, SocketFlags.None);
                            if (bytes == 0)
                            {
                                Close();
                                return false;
                            }
                            Array.Resize(ref buffer, bytes + bufferLastLength);
                            bool doNext = true;
                            while (doNext)
                            {
                                doNext = false;
                                if (neededBytes == 0)
                                {
                                    if (workingBuffer.Length > 0)
                                    {
                                        if (ClientSupportedEncryptionType != Common.Encryption_Type.Anything && ClientSupportedEncryptionType != peerEncryptionType) // Check if we support this type of encryption
                                        {
                                            Close("Protocol 1. Unsupported encryption type.");
                                            return false; 
                                        }

                                        if (peerEncryptionType != Common.Encryption_Type.None)
                                        {
                                            peerCryptor.SetSalt(pearEncryptionSalt);
                                            workingBuffer = peerCryptor.Decrypt(workingBuffer);
                                        }

                                        if (ClientSupportedCompressionType != Common.Compression_Type.Anything && ClientSupportedCompressionType != peerCompressionType) // Check if we support this type of compression
                                        {
                                            Close("Protocol 2. Unsupported compression type.");
                                            return false;
                                        }

                                        workingBuffer = peerCompressor.Decompress(workingBuffer);

                                        Array.Resize(ref waitingBuffer, waitingBuffer.Length + workingBuffer.Length);
                                        Array.Copy(workingBuffer, 0, waitingBuffer, waitingBuffer.Length - workingBuffer.Length, workingBuffer.Length);
                                        workingBuffer = new byte[0];
                                    }

                                    if (buffer.Length >= 10)
                                    {
                                        if (PocketRecieved == 65535)
                                            PocketRecieved = 0;

                                         if ((Common.Compression_Type)buffer[1] != peerCompressionType)
                                        {
                                            peerCompressionType = (Common.Compression_Type)buffer[1];
                                            switch (peerCompressionType)
                                            {
                                                case Common.Compression_Type.gZip:
                                                    peerCompressor = new Compressors.gZipCompressor();
                                                    break;
                                                case Common.Compression_Type.Deflate:
                                                    peerCompressor = new Compressors.DeflateCompressor();
                                                    break;
                                            }
                                        }
                                        int recieveCounter = (buffer[2] * 256) + buffer[3];
                                        neededBytes = (buffer[4] * 256) + buffer[5];
                                        Array.Copy(buffer, 6, pearEncryptionSalt, 0, 4);
                                        if ((Common.Encryption_Type)buffer[0] != peerEncryptionType)
                                        {
                                            peerEncryptionType = (Common.Encryption_Type)buffer[0];
                                            //if (encryptionType == peerEncryptionType)
                                            //    peerCryptor = Cryptor;
                                            //else
                                            if (EncryptionKey.Length == 0)
                                                EncryptionKey = pearEncryptionSalt;
                                            switch (peerEncryptionType)
                                            {
                                                case Common.Encryption_Type.TripleDES:
                                                    peerCryptor = new Cryptors.TripleDESCryptor(encryptionKey);
                                                    break;
                                                case Common.Encryption_Type.SimpleXOR:
                                                    peerCryptor = new Cryptors.SimpleXORCryptor(encryptionKey);
                                                    break;
                                            }
                                        }
                                        if (recieveCounter != PocketRecieved)
                                        {
                                            Close("Protocol 3. Packet lost, Last received packet: " + recieveCounter.ToString() + ", Expected: " + PocketRecieved.ToString());
                                            return false;
                                        }
                                        PocketRecieved++;
                                        if (neededBytes <= buffer.Length - 10)
                                        {
                                            Array.Resize(ref workingBuffer, workingBuffer.Length + neededBytes);
                                            Array.Copy(buffer, 10, workingBuffer, 0, neededBytes);

                                            Array.Copy(buffer, 10 + neededBytes, buffer, 0, (buffer.Length - 10) - neededBytes);
                                            Array.Resize(ref buffer, (buffer.Length - 10) - neededBytes);
                                            neededBytes = 0;
                                            doNext = true;
                                        }
                                        else
                                        {
                                            Array.Resize(ref workingBuffer, workingBuffer.Length + (buffer.Length - 10));
                                            Array.Copy(buffer, 10, workingBuffer, 0, buffer.Length - 10);
                                            neededBytes -= buffer.Length - 10;
                                            buffer = new byte[0];
                                        }
                                    }
                                }
                                else
                                {
                                    if (buffer.Length > 0)
                                        if (neededBytes <= buffer.Length)
                                        {
                                            Array.Resize(ref workingBuffer, workingBuffer.Length + neededBytes);
                                            Array.Copy(buffer, 0, workingBuffer, workingBuffer.Length - neededBytes, neededBytes);

                                            Array.Copy(buffer, neededBytes, buffer, 0, (buffer.Length) - neededBytes);
                                            Array.Resize(ref buffer, (buffer.Length) - neededBytes);
                                            neededBytes = 0;
                                            doNext = true;
                                        }
                                        else
                                        {
                                            Array.Resize(ref workingBuffer, workingBuffer.Length + buffer.Length);
                                            Array.Copy(buffer, 0, workingBuffer, workingBuffer.Length - buffer.Length, buffer.Length);
                                            neededBytes -= buffer.Length;
                                            buffer = new byte[0];
                                        }
                                }
                            }
                        }
                        if (buffer.Length == 0 && (waitingBuffer.Length > 0 || !SyncWait))
                            return (waitingBuffer.Length > 0);
                        else if (i != _i)
                            System.Threading.Thread.Sleep(1);
                        i--;
                    }
                }
                return (waitingBuffer.Length > 0);
            }
            catch (Exception e)
            {
                Close("Protocol 4. " + e.Message);
            }
            return false;
        }

        public void Write(byte[] bytes, bool async, bool enc = true)
        {
            try
            {
                if (bytes != null)
                {
                    int peaces = (int)Math.Ceiling((double)bytes.Length / (double)SendPacketSize);
                    for (int i = 0; i < peaces; i++)
                    {
                        int len = Math.Min(SendPacketSize, bytes.Length - (i * SendPacketSize));
                        byte[] framingBody = new byte[len];
                        Array.Copy(bytes, (i * SendPacketSize), framingBody, 0, len);
                        if (PocketSent == 65535)
                            PocketSent = 0;

                        byte[] framingHeader = new byte[10];

                        framingHeader[1] = (byte)compressionType;
                        framingBody = Compressor.Compress(framingBody);

                        if (enc && encryptionType != Common.Encryption_Type.None) // If encryption is enable
                        {
                            byte[] encryptionSalt = new byte[4];
                            rnd.GetNonZeroBytes(encryptionSalt);
                            Array.Copy(encryptionSalt, 0, framingHeader, 6, 4);

                            if (EncryptionKey.Length == 0)
                                EncryptionKey = encryptionSalt;

                            framingHeader[0] = (byte)encryptionType;
                            Cryptor.SetSalt(encryptionSalt);
                            framingBody = Cryptor.Encrypt(framingBody);
                        }
                        framingHeader[2] = (byte)Math.Floor((double)PocketSent / 256);
                        framingHeader[3] = (byte)(PocketSent % 256);
                        framingHeader[4] = (byte)Math.Floor((double)framingBody.Length / 256);
                        framingHeader[5] = (byte)(framingBody.Length % 256);
                        Array.Resize(ref writeBuffer, writeBuffer.Length + framingHeader.Length + framingBody.Length);
                        Array.Copy(framingHeader, 0, writeBuffer, writeBuffer.Length - (framingHeader.Length + framingBody.Length), framingHeader.Length);
                        Array.Copy(framingBody, 0, writeBuffer, writeBuffer.Length - framingBody.Length, framingBody.Length);
                        PocketSent++;
                    }
                }
                if (writeBuffer.Length > 0 && UnderlyingSocket.Poll(0,SelectMode.SelectWrite))
                {
                    int bytesWritten = UnderlyingSocket.Send(writeBuffer, SocketFlags.None);
                    Array.Copy(writeBuffer, bytesWritten, writeBuffer, 0, writeBuffer.Length - bytesWritten);
                    Array.Resize(ref writeBuffer, writeBuffer.Length - bytesWritten);
                    if (!async && writeBuffer.Length > 0)
                        this.Write(null, false);
                }
            }
            catch (Exception)// e)
            {
                Close();//"Protocol 5. " + e.Message);
            }
        }

        public byte[] Read()
        {
            if (isDataAvailable(true))
            {
                byte[] cop = (byte[])waitingBuffer.Clone();
                waitingBuffer = new byte[0];
                return cop;
            }
            return null;
        }

        public void Close(string message = null, bool async = false)
        {
            try
            {
                if (isDC)
                    return;

                isDC = true;

                if (CloseCallback!=null)
                    CloseCallback(message, async);

                if (async)
                {
                    byte[] db = new byte[0];
                    if (UnderlyingSocket != null)
                        UnderlyingSocket.BeginSend(db, 0, db.Length, SocketFlags.None, (AsyncCallback)delegate(IAsyncResult ar)
                        {
                            try
                            {
                                UnderlyingSocket.Close(); // Close request connection it-self
                                UnderlyingSocket.EndSend(ar);
                            }
                            catch (Exception) { }
                        }, null);
                }
                else
                {
                    if (UnderlyingSocket != null)
                        UnderlyingSocket.Close();
                }
            }
            catch { }
        }
    }
}
