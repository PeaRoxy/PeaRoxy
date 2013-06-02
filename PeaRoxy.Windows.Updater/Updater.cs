using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeaRoxy.Windows
{
    public class iUpdater
    {
        public delegate void DownloadCompleted(bool success, EventArgs e);
        public delegate void ByteDownloaded(int readedBytes,int totalBytes, EventArgs e);
        //public static event DownloadCompleted downloadCompleted;
        //public static event ByteDownloaded byteDownloaded;
        private Uri uri;
        private System.Net.WebProxy proxy;
        public iUpdater(string updateURL, System.Net.WebProxy connectionProxy)
        {
            proxy = connectionProxy;
            uri = new Uri(updateURL);
        }

        public bool IsUpdateNeeded(Version currentVersion)
        {
            return false;
        }

        public void StartDownloading()
        {

        }

        public void InstallUpdate()
        {

        }
    }
}
