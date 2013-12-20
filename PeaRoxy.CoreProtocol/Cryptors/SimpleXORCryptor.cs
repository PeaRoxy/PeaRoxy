// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleXorCryptor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The simple xor cryptor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Cryptors
{
    #region

    using System.Diagnostics;

    #endregion

    /// <summary>
    /// The simple xor cryptor.
    /// </summary>
    public class SimpleXorCryptor : Cryptor
    {
        #region Fields

        /// <summary>
        /// The key.
        /// </summary>
        private readonly byte[] key;

        /// <summary>
        /// The self sync.
        /// </summary>
        private readonly bool selfSync = true;

        /// <summary>
        /// The key pos_dec.
        /// </summary>
        private int keyPosDec;

        /// <summary>
        /// The key pos_enc.
        /// </summary>
        private int keyPosEnc;

        /// <summary>
        /// The salt.
        /// </summary>
        private byte[] salt;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleXorCryptor"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="selfSync">
        /// The self sync.
        /// </param>
        [DebuggerStepThrough]
        public SimpleXorCryptor(byte[] key, bool selfSync = true)
        {
            this.key = (byte[])key.Clone();
            this.selfSync = selfSync;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The decrypt.
        /// </summary>
        /// <param name="toDecrypt">
        /// The to decrypt.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        [DebuggerStepThrough]
        public override byte[] Decrypt(byte[] toDecrypt)
        {
            if (this.selfSync)
            {
                this.keyPosDec = 0;
            }

            byte[] resultArray = new byte[toDecrypt.Length];
            for (int i = 0; i < toDecrypt.Length; i++)
            {
                resultArray[i] = (byte)(toDecrypt[i] ^ this.key[(i + this.keyPosDec) % this.key.Length]);
                if (this.selfSync)
                {
                    this.key[(i + (this.key.Length / 2)) % this.key.Length] =
                        (byte)(this.key[i % this.key.Length] ^ this.salt[i % this.salt.Length]);
                }
            }

            if (!this.selfSync)
            {
                this.keyPosDec = (this.keyPosDec + toDecrypt.Length) % this.key.Length;
            }

            return resultArray;
        }

        /// <summary>
        /// The encrypt.
        /// </summary>
        /// <param name="toEncrypt">
        /// The to encrypt.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        [DebuggerStepThrough]
        public override byte[] Encrypt(byte[] toEncrypt)
        {
            if (this.selfSync)
            {
                this.keyPosEnc = 0;
            }

            byte[] resultArray = new byte[toEncrypt.Length];
            for (int i = 0; i < toEncrypt.Length; i++)
            {
                resultArray[i] = (byte)(toEncrypt[i] ^ this.key[(i + this.keyPosEnc) % this.key.Length]);
                if (this.selfSync)
                {
                    this.key[(i + (this.key.Length / 2)) % this.key.Length] =
                        (byte)(this.key[i % this.key.Length] ^ this.salt[i % this.salt.Length]);
                }
            }

            if (!this.selfSync)
            {
                this.keyPosEnc = (this.keyPosEnc + toEncrypt.Length) % this.key.Length;
            }

            return resultArray;
        }

        /// <summary>
        /// The set newSalt.
        /// </summary>
        /// <param name="newSalt">
        /// The newSalt.
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

        #endregion
    }
}