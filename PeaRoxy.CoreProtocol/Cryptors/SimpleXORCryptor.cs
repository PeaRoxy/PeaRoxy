// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleXorCryptor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Cryptors
{
    using System.Diagnostics;

    /// <summary>
    ///     The SimpleXorCryptor is a child of Cryptor class which can encrypt data using a custom algorithm based on XOR
    ///     operations
    /// </summary>
    public class SimpleXorCryptor : Cryptor
    {
        private readonly byte[] key;

        private readonly bool selfSync = true;

        private int keyPositionDecryption;

        private int keyPositionEncription;

        private byte[] salt;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SimpleXorCryptor" /> class.
        /// </summary>
        /// <param name="key">
        ///     The encryption key.
        /// </param>
        /// <param name="selfSync">
        ///     The self sync argument indicates if we should use "self key synchronization" or not.
        /// </param>
        [DebuggerStepThrough]
        public SimpleXorCryptor(byte[] key, bool selfSync = true)
        {
            this.key = (byte[])key.Clone();
            this.selfSync = selfSync;
        }

        /// <summary>
        ///     The decrypt.
        /// </summary>
        /// <param name="buffer">
        ///     The to decrypt.
        /// </param>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        [DebuggerStepThrough]
        public override byte[] Decrypt(byte[] buffer)
        {
            if (this.selfSync)
            {
                this.keyPositionDecryption = 0;
            }

            byte[] resultArray = new byte[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                resultArray[i] = (byte)(buffer[i] ^ this.key[(i + this.keyPositionDecryption) % this.key.Length]);
                if (this.selfSync)
                {
                    this.key[(i + (this.key.Length / 2)) % this.key.Length] =
                        (byte)(this.key[i % this.key.Length] ^ this.salt[i % this.salt.Length]);
                }
            }

            if (!this.selfSync)
            {
                this.keyPositionDecryption = (this.keyPositionDecryption + buffer.Length) % this.key.Length;
            }

            return resultArray;
        }

        /// <summary>
        ///     The encrypt method is used to encrypt the data.
        /// </summary>
        /// <param name="buffer">
        ///     The data in form of byte[].
        /// </param>
        /// <returns>
        ///     The encrypted data in form of
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        [DebuggerStepThrough]
        public override byte[] Encrypt(byte[] buffer)
        {
            if (this.selfSync)
            {
                this.keyPositionEncription = 0;
            }

            byte[] resultArray = new byte[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                resultArray[i] = (byte)(buffer[i] ^ this.key[(i + this.keyPositionEncription) % this.key.Length]);
                if (this.selfSync)
                {
                    this.key[(i + (this.key.Length / 2)) % this.key.Length] =
                        (byte)(this.key[i % this.key.Length] ^ this.salt[i % this.salt.Length]);
                }
            }

            if (!this.selfSync)
            {
                this.keyPositionEncription = (this.keyPositionEncription + buffer.Length) % this.key.Length;
            }

            return resultArray;
        }

        /// <summary>
        ///     The SetSalt method is used to change the encryption salt value.
        /// </summary>
        /// <param name="newSalt">
        ///     The encryption salt value in form of byte[].
        /// </param>
        [DebuggerStepThrough]
        public override void SetSalt(byte[] newSalt)
        {
            this.salt = (byte[])newSalt.Clone();
            if (!this.selfSync)
            {
                for (int i = 0; i < this.key.Length; i++)
                {
                    byte nv = (byte)(this.key[i] ^ this.salt[(i + 1) % this.salt.Length]);
                    if (nv != 0)
                    {
                        this.key[i] = nv;
                    }
                }
            }
        }
    }
}