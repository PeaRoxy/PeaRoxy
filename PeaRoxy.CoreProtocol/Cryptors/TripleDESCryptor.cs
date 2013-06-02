using System;
using System.Security.Cryptography;

namespace PeaRoxy.CoreProtocol.Cryptors
{
    public class TripleDESCryptor : Cryptor, IDisposable
    {
        TripleDESCryptoServiceProvider tdes;
        ICryptoTransform cTransform_enc;
        ICryptoTransform cTransform_dec;

        [System.Diagnostics.DebuggerStepThrough]
        public TripleDESCryptor(byte[] key)
        {
            if (key.Length != 24)
                Array.Resize(ref key, 24);

            while (TripleDES.IsWeakKey(key))
            {
                var md5 = MD5.Create().ComputeHash(key);
                Array.Copy(md5, 0, key, 8, key.Length - 8);
            }

            tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.ANSIX923;
            cTransform_enc = tdes.CreateEncryptor();
            cTransform_dec = tdes.CreateDecryptor();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override void SetSalt(byte[] salt)
        {
            if (salt.Length != 8)
                Array.Resize(ref salt, 8);

            tdes.IV = salt;
            cTransform_enc = tdes.CreateEncryptor();
            cTransform_dec = tdes.CreateDecryptor();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override byte[] Encrypt(byte[] toEncrypt)
        {
            return cTransform_enc.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override byte[] Decrypt(byte[] toDecrypt)
        {
            return cTransform_dec.TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);
        }

        public void Dispose()
        {
            if (this.tdes != null)
                this.tdes.Dispose();
        }
    }
}
