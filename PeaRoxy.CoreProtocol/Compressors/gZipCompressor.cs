using System;
using System.IO;
using System.IO.Compression;

namespace PeaRoxy.CoreProtocol.Compressors
{
    public class gZipCompressor : Compressor
    {
        [System.Diagnostics.DebuggerStepThrough]
        public override byte[] Compress(byte[] buffer)
        {
            byte[] retValue;
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                    zip.Write(buffer, 0, buffer.Length);

                retValue = ms.ToArray();
            }
            return retValue;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public override byte[] Decompress(byte[] gzBuffer)
        {
            byte[] retValue;
            using (MemoryStream msOut = new MemoryStream())
            {
                using (MemoryStream ms = new MemoryStream(gzBuffer))
                    using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                        zip.CopyTo(msOut);

                retValue = msOut.ToArray();
            }
            return retValue;
        }
    }
}
