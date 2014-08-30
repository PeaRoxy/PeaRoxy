// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Compressor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Compressors
{
    using System.Diagnostics;

    /// <summary>
    ///     The Compressor class is the base class of all compression classes.
    /// </summary>
    public class Compressor
    {
        /// <summary>
        ///     The compress method is used to compress the data.
        /// </summary>
        /// <param name="buffer">
        ///     The data in form of byte[].
        /// </param>
        /// <returns>
        ///     The compressed data in form of
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        [DebuggerStepThrough]
        public virtual byte[] Compress(byte[] buffer)
        {
            return buffer;
        }

        /// <summary>
        ///     The decompress method is used to decompress the compressed data.
        /// </summary>
        /// <param name="buffer">
        ///     The compressed data in form of byte[].
        /// </param>
        /// <returns>
        ///     The decompressed data in form of
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        [DebuggerStepThrough]
        public virtual byte[] Decompress(byte[] buffer)
        {
            return buffer;
        }
    }
}