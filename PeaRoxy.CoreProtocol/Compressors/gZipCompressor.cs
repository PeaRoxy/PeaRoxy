﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GZipCompressor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The g zip compressor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Compressors
{
    #region

    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;

    #endregion

    /// <summary>
    /// The GZip compressor.
    /// </summary>
    public class GZipCompressor : Compressor
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
        [DebuggerStepThrough]
        public override byte[] Compress(byte[] buffer)
        {
            byte[] retValue;
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true)) zip.Write(buffer, 0, buffer.Length);

                retValue = ms.ToArray();
            }

            return retValue;
        }

        /// <summary>
        /// The decompress.
        /// </summary>
        /// <param name="gzBuffer">
        /// The gz buffer.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        [DebuggerStepThrough]
        public override byte[] Decompress(byte[] gzBuffer)
        {
            byte[] retValue;
            using (MemoryStream msOut = new MemoryStream())
            {
                using (MemoryStream ms = new MemoryStream(gzBuffer)) using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress)) zip.CopyTo(msOut);

                retValue = msOut.ToArray();
            }

            return retValue;
        }

        #endregion
    }
}