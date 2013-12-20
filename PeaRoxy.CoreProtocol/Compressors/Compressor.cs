// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Compressor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The compressor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Compressors
{
    /// <summary>
    /// The compressor.
    /// </summary>
    public class Compressor
    {
        #region Public Methods and Operators

        /// <summary>
        /// The compress.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public virtual byte[] Compress(byte[] buffer)
        {
            return buffer;
        }

        /// <summary>
        /// The decompress.
        /// </summary>
        /// <param name="dfBuffer">
        /// The df buffer.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public virtual byte[] Decompress(byte[] dfBuffer)
        {
            return dfBuffer;
        }

        #endregion
    }
}