// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TripleDesCryptor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Cryptors
{
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography;

    /// <summary>
    ///     The TripleDesCryptor is a child of Cryptor class which can encrypt data using TripleDes Algorithm
    /// </summary>
    public class TripleDesCryptor : Cryptor, IDisposable
    {
        private readonly TripleDESCryptoServiceProvider tripleDesCryptorService;

        private ICryptoTransform decryptorTransform;

        private ICryptoTransform encryptorTransform;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TripleDesCryptor" /> class.
        /// </summary>
        /// <param name="key">
        ///     The encryption key.
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
                byte[] md5 = MD5.Create().ComputeHash(key);
                Array.Copy(md5, 0, key, 8, key.Length - 8);
            }

            this.tripleDesCryptorService = new TripleDESCryptoServiceProvider
                                               {
                                                   Key = key,
                                                   Mode = CipherMode.CBC,
                                                   Padding = PaddingMode.ANSIX923
                                               };
            this.encryptorTransform = this.tripleDesCryptorService.CreateEncryptor();
            this.decryptorTransform = this.tripleDesCryptorService.CreateDecryptor();
        }

        public void Dispose()
        {
            if (this.tripleDesCryptorService != null)
            {
                this.tripleDesCryptorService.Dispose();
            }
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
            return this.decryptorTransform.TransformFinalBlock(buffer, 0, buffer.Length);
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
            return this.encryptorTransform.TransformFinalBlock(buffer, 0, buffer.Length);
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
            if (newSalt.Length != 8)
            {
                Array.Resize(ref newSalt, 8);
            }

            this.tripleDesCryptorService.IV = newSalt;
            this.encryptorTransform = this.tripleDesCryptorService.CreateEncryptor();
            this.decryptorTransform = this.tripleDesCryptorService.CreateDecryptor();
        }
    }
}