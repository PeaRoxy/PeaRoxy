using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PeaRoxy.CommonLibrary;
using PeaRoxy.CoreProtocol;
namespace PeaRoxy.ASPear
{
    public class index : IHttpHandler
    {
        bool isValid = false;
        public void ProcessRequest(HttpContext context)
        {
            CoreProtocol.Cryptors.Cryptor cryptor = null;
            CoreProtocol.Cryptors.Cryptor peerCryptor = null;
            if (context.Request.Cookies.Count < 1)
                DoError("Request info is missing.");
            try
            {
                byte[] requestInfo = Convert.FromBase64String(context.Request.Cookies.Get(0).Value);
                byte[] encryptionSalt = new byte[4];
                Array.Copy(requestInfo, encryptionSalt, 4);
                Common.Encryption_Type ecryptionType = (Common.Encryption_Type)requestInfo[4];
                byte[] encryptedHost = new byte[requestInfo.Length - 5];
                Array.Copy(requestInfo, 5, encryptedHost, 0, requestInfo.Length - 5);
                isValid = true;
            }
            catch (Exception)
            {
                DoError("Request info is missing.");
            }


            
            
            System.IO.Stream stream = context.Request.InputStream;

            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        private void DoError(string p)
        {
            throw new NotImplementedException();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}