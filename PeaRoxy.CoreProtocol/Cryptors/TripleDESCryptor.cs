// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TripleDesCryptor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The triple des cryptor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Cryptors
{
    #region

    using System;
    using System.Diagnostics;
    using System.Security.Cryptography;

    #endregion

    /// <summary>
    /// The triple DES cryptor.
    /// </summary>
    public class TripleDesCryptor : Cryptor, IDisposable
    {
        #region Fields

        /// <summary>
        /// The tdes.
        /// </summary>
        private readonly TripleDESCryptoServiceProvider tdes;

        /// <summary>
        /// The c transform_dec.
        /// </summary>
        private ICryptoTransform cTransformDec;

        /// <summary>
        /// The c transform_enc.
        /// </summary>
        private ICryptoTransform cTransformEnc;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDesCryptor"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        [DebuggerStepThrough]
        public TripleDesCryptor(byte[] key)
        {
            if (key.Length != 24)
            {
                Array.Resize(ref key, 24);
            }

            while (TripleDES.IsWeakKey(key))
            {
                var md5 = MD5.Create().ComputeHash(key);
                Array.Copy(md5, 0, key, 8, key.Length - 8);
            }

            this.tdes = new TripleDESCryptoServiceProvider
                            {
                                Key = key,
                                Mode = CipherMode.CBC,
                                Padding = PaddingMode.ANSIX923
                            };
            this.cTransformEnc = this.tdes.CreateEncryptor();
            this.cTransformDec = this.tdes.CreateDecryptor();
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
            return this.cTransformDec.TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            if (this.tdes != null)
            {
                this.tdes.Dispose();
            }
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
            return this.cTransformEnc.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
        }

        /// <summary>
        /// The set salt.
        /// </summary>
        /// <param name="newSalt">
        /// The newSalt.
        /// </param>
        [DebuggerStepThrough]
        public override void SetSalt(byte[] newSalt)
        {
            if (newSalt.Length != 8)
            {
                Array.Resize(ref newSalt, 8);
            }

            this.tdes.IV = newSalt;
            this.cTransformEnc = this.tdes.CreateEncryptor();
            this.cTransformDec = this.tdes.CreateDecryptor();
        }

        #endregion
    }
}