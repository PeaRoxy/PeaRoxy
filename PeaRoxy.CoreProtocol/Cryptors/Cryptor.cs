// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cryptor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The cryptor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Cryptors
{
    /// <summary>
    /// The cryptor.
    /// </summary>
    public class Cryptor
    {
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
        public virtual byte[] Decrypt(byte[] toDecrypt)
        {
            return toDecrypt;
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
        public virtual byte[] Encrypt(byte[] toEncrypt)
        {
            return toEncrypt;
        }

        /// <summary>
        /// The set salt.
        /// </summary>
        /// <param name="iv">
        /// The iv.
        /// </param>
        public virtual void SetSalt(byte[] iv)
        {
            // Do Nothing
        }

        #endregion
    }
}