// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GZipCompressor.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CoreProtocol.Compressors
{
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    ///     The GZipCompressor is a child of Compressor class which can compress data using GZip Algorithm
    /// </summary>
    public class GZipCompressor : Compressor
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
        public override byte[] Decompress(byte[] buffer)
        {
            byte[] retValue;
            using (MemoryStream msOut = new MemoryStream())
            {
                using (MemoryStream ms = new MemoryStream(buffer)) using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress)) zip.CopyTo(msOut);

                retValue = msOut.ToArray();
            }

            return retValue;
        }
    }
}