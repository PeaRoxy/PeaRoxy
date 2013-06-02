using System;

namespace PeaRoxy.CoreProtocol.Compressors
{
    public class Compressor
    {
        public virtual byte[] Compress(byte[] buffer)
        {
            return buffer;
        }

        public virtual byte[] Decompress(byte[] dfBuffer)
        {
            return dfBuffer;
        }
    }
}

