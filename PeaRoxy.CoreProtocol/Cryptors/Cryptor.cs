// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cryptor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Cryptors
{
    using System.Diagnostics;

    /// <summary>
    ///     The Cryptor class is the base class of all encryption classes.
    /// </summary>
    public class Cryptor
    {
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
        public virtual byte[] Decrypt(byte[] buffer)
        {
            return buffer;
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
        public virtual byte[] Encrypt(byte[] buffer)
        {
            return buffer;
        }

        /// <summary>
        ///     The SetSalt method is used to change the encryption salt value.
        /// </summary>
        /// <param name="newSalt">
        ///     The encryption salt value in form of byte[].
        /// </param>
        [DebuggerStepThrough]
        public virtual void SetSalt(byte[] newSalt)
        {
            // Do Nothing
        }
    }
}