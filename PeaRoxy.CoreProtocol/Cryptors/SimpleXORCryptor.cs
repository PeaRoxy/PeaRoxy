using System;
using System.Security.Cryptography;

namespace PeaRoxy.CoreProtocol.Cryptors
{
    public class SimpleXORCryptor : Cryptor
    {
        byte[] key;
        byte[] salt;
        bool selfSync = true;
        int keyPos_dec = 0;
        int keyPos_enc = 0;

        [System.Diagnostics.DebuggerStepThrough]
        public SimpleXORCryptor(byte[] key, bool selfSync = true)
        {
            this.key = (byte[])key.Clone();
            this.selfSync = selfSync;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override void SetSalt(byte[] salt)
        {
            this.salt = (byte[])salt.Clone();
            if (!selfSync)
                for (int i = 0; i < key.Length; i++)
                {
                    byte nv = (byte)(key[i] ^ this.salt[(i + 1) % this.salt.Length]);
                    if (nv != 0)
                        key[i] = nv;
                }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override byte[] Encrypt(byte[] toEncrypt)
        {
            if (selfSync)
                keyPos_enc = 0;
            byte[] resultArray = new byte[toEncrypt.Length];
            for (int i = 0; i < toEncrypt.Length; i++)
            {
                resultArray[i] = (byte)(toEncrypt[i] ^ key[(i + keyPos_enc) % key.Length]);
                if (selfSync)
                    key[(i + (key.Length / 2)) % key.Length] = (byte)(key[i % key.Length] ^ salt[i % salt.Length]);
            }
            if (!selfSync)
                keyPos_enc = (keyPos_enc + toEncrypt.Length) % key.Length;
            return resultArray;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override byte[] Decrypt(byte[] toDecrypt)
        {
            if (selfSync)
                keyPos_dec = 0;
            byte[] resultArray = new byte[toDecrypt.Length];
            for (int i = 0; i < toDecrypt.Length; i++)
            {
                resultArray[i] = (byte)(toDecrypt[i] ^ key[(i + keyPos_dec) % key.Length]);
                if (selfSync)
                    key[(i + (key.Length / 2)) % key.Length] = (byte)(key[i % key.Length] ^ salt[i % salt.Length]);
            }
            if (!selfSync)
                keyPos_dec = (keyPos_dec + toDecrypt.Length) % key.Length;
            return resultArray;
        }
    }
}