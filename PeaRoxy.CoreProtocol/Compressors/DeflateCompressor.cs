using System;
using System.IO;
using System.IO.Compression;

namespace PeaRoxy.CoreProtocol.Compressors
{
    public class DeflateCompressor : Compressor
    {
        [System.Diagnostics.DebuggerStepThrough]
        public override byte[] Compress(byte[] buffer)
        {
            byte[] retValue;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DeflateStream df = new DeflateStream(ms, CompressionMode.Compress, true))
                    df.Write(buffer, 0, buffer.Length);

                retValue = ms.ToArray();
            }
            return retValue;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override byte[] Decompress(byte[] dfBuffer)
        {
            byte[] retValue;
            using (MemoryStream msOut = new MemoryStream())
            {
                using (MemoryStream ms = new MemoryStream(dfBuffer))
                    using (DeflateStream df = new DeflateStream(ms, CompressionMode.Decompress))
                        df.CopyTo(msOut);

                retValue = msOut.ToArray();
            }
            return retValue;
        }
    }
}
