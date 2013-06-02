using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeaRoxy.CoreProtocol.Cryptors
{
    public class Cryptor
    {
        public virtual void SetSalt(byte[] iv)
        {
            // Do Nothing
        }
        public virtual byte[] Encrypt(byte[] toEncrypt)
        {
            return toEncrypt;
        }
        public virtual byte[] Decrypt(byte[] toDecrypt)
        {
            return toDecrypt;
        }
    }
}
