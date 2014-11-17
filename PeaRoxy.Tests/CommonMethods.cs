namespace PeaRoxy.Tests
{
    using System;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    internal class CommonMethods
    {
        public static WebProxy CreateProxy(IPAddress ip, ushort port)
        {
            return new WebProxy(ip + ":" + port, true);
        }

        public static void StringDownloadTest(WebProxy proxy = null)
        {
            using (WebClient client = new WebClient { Proxy = proxy })
            {
                string md5Hash = Md5Hash(client.DownloadString("http://httpbin.org/xml"));
                if (!md5Hash.Equals("6aa4dfbabc8fa0bff8367353966f5ac0", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Integrity check: Invalid string MD5, " + md5Hash);
                }
            }
        }

        public static void UploadStringTest(WebProxy proxy = null)
        {
            using (WebClient client = new WebClient { Proxy = proxy })
            {
                const string Data =
                    "njGCQNQnEE4xZJIlSuaooaqpUCnsIHvci0JAhZNgwKoKgJmHlCrxcpCVclQPGEjrApqQgMvh0YOxevxaIA83V2T2y4PCvimuorhQ";
                string result = client.UploadString("http://httpbin.org/post", Data);
                if (result.IndexOf("\"files\"", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    result = result.Substring(0, result.IndexOf("\"files\"", StringComparison.OrdinalIgnoreCase));
                }
                if (result.IndexOf("\"data\"", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    result = result.Substring(result.IndexOf("\"data\"", StringComparison.OrdinalIgnoreCase) + 7);
                }
                string md5Hash = Md5Hash(result.Trim().Trim(new[] { ',', '\"', '\'' }));
                if (!md5Hash.Equals("931d375c8da3e9bd660943b4cbceab07", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Integrity check: Invalid string MD5, " + md5Hash);
                }
            }
        }

        public static void SmallFileDownloadTest(WebProxy proxy = null)
        {
            using (WebClient client = new WebClient { Proxy = proxy })
            {
                string md5Hash = Md5Hash(client.DownloadData("http://httpbin.org/bytes/10240?seed=1"));
                if (!md5Hash.Equals("b5f899461451f9e47d199489e2125229", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Integrity check: Invalid small file MD5, " + md5Hash);
                }
            }
        }

        public static void BigFileDownloadTest(WebProxy proxy = null)
        {
            using (WebClient client = new WebClient { Proxy = proxy })
            {
                string md5Hash = Md5Hash(client.DownloadData("http://mirror.internode.on.net/pub/test/1meg.test"));
                if (!md5Hash.Equals("e6527b4d5db05226f40f9f2e7750abfb", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Integrity check: Invalid big file MD5, " + md5Hash);
                }
            }
        }

        public static string Md5Hash(string str)
        {
            return Md5Hash(Encoding.ASCII.GetBytes(str));
        }

        public static string Md5Hash(byte[] bytes)
        {
            using (MD5 md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToLower();
            }
        }
    }
}