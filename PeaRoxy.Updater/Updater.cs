namespace PeaRoxy.Updater
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    #endregion

    public class Updater
    {
        #region Fields

        private XElement feedCache;

        #endregion

        #region Constructors and Destructors

        public Updater(string appName, Version appVersion, WebProxy proxy = null)
        {
            this.AppName = appName.Trim();
            this.AppVersion = appVersion;
            this.Proxy = proxy;
        }

        #endregion

        #region Public Properties

        public string AppName { get; private set; }

        public Version AppVersion { get; private set; }

        public WebProxy Proxy { get; set; }

        #endregion

        #region Public Methods and Operators

        public static string GetSmartPearProfileAddress(string profile)
        {
            const string SmartUrl = "https://github.com/PeaRoxy/PeaRoxy/blob/master/SmartPearProfiles";
            return string.Format(
                "{0}/Smart_{1}.xml",
                SmartUrl,
                (!string.IsNullOrWhiteSpace(profile) ? profile.ToUpper().Trim() : "NONE"));
        }

        public AppVersion GetLatestVersion()
        {
            IEnumerable<AppVersion> versions = this.GetVersions();
            AppVersion latestVersion = null;
            if (versions != null)
            {
                foreach (AppVersion version in versions)
                {
                    if (latestVersion == null || version.Version > latestVersion.Version)
                    {
                        latestVersion = version;
                    }
                }
            }
            return latestVersion;
        }

        public IEnumerable<AppVersion> GetVersions()
        {
            try
            {
                XElement document = this.LoadFeed();
                if (document != null)
                {
                    return
                        document.Descendants()
                            .Where(
                                element =>
                                string.Equals(
                                    element.Name.LocalName,
                                    "entry",
                                    StringComparison.CurrentCultureIgnoreCase))
                            .Select(element => new AppVersion(element))
                            .Where(
                                version =>
                                String.Equals(version.AppName, this.AppName, StringComparison.CurrentCultureIgnoreCase)
                                && version.Platform == PeaRoxy.Updater.AppVersion.Platforms.Win)
                            .ToList();
                }
            }
            catch
            {
            }
            return null;
        }

        public bool IsNewVersionAvailable()
        {
            IEnumerable<AppVersion> versions = this.GetVersions();
            return versions != null && versions.Any(version => version.Version > this.AppVersion);
        }

        public bool SubmitSmartPearProfile(string xml, string profile)
        {
            const string SmartUrl = "http://reporting.pearoxy.com/submitsmartpear.php";
            try
            {
                WebClient client = new WebClient();
                if (this.Proxy != null)
                {
                    client.Proxy = this.Proxy;
                }
                client.UploadString(SmartUrl + "?profile=" + profile.ToLower().Trim(), xml);
                return true;
            }
            catch
            {
            }
            return false;
        }

        #endregion

        #region Methods

        private XElement LoadFeed()
        {
            const string FeedUrl = "https://github.com/PeaRoxy/PeaRoxy/releases.atom";
            if (this.feedCache != null)
            {
                return this.feedCache;
            }
            try
            {
                WebClient client = new WebClient();
                if (this.Proxy != null)
                {
                    client.Proxy = this.Proxy;
                }
                string xml = client.DownloadString(FeedUrl);
                return this.feedCache = XDocument.Parse(xml).Root;
            }
            catch
            {
            }
            return null;
        }

        #endregion
    }
}