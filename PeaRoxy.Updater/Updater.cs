// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Updater.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Updater
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    /// <summary>
    ///     The Updater class is responsible for updating the program, updating the SmartPear list and SmartPear rules.
    /// </summary>
    public class Updater
    {
        private XElement feedCache;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Updater" /> class.
        /// </summary>
        /// <param name="appName">
        ///     The name of the application.
        /// </param>
        /// <param name="appVersion">
        ///     The current version of the application.
        /// </param>
        /// <param name="proxy">
        ///     The proxy object to be used for sending requests to the Internet.
        /// </param>
        public Updater(string appName, Version appVersion, WebProxy proxy = null)
        {
            this.AppName = appName.Trim();
            this.AppVersion = appVersion;
            this.Proxy = proxy;
        }

        /// <summary>
        ///     Gets the name of the application.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        ///     Gets the version of the application.
        /// </summary>
        public Version AppVersion { get; private set; }

        /// <summary>
        ///     Gets the proxy object to be used for sending requests to the Internet.
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        ///     This method will give us a download address for specific SmartPear profile
        /// </summary>
        public static string GetSmartPearProfileAddress(string profile)
        {
            const string SmartUrl = "https://raw.github.com/PeaRoxy/PeaRoxy/master/SmartPearProfiles";
            return string.Format(
                "{0}/Smart{1}.xml",
                SmartUrl,
                (!string.IsNullOrWhiteSpace(profile) ? "_" + profile.ToUpper().Trim() : ""));
        }

        /// <summary>
        ///     This method will give us information about the latest version of the application
        /// </summary>
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

        /// <summary>
        ///     This method will give us information about all versions of the application
        /// </summary>
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
                                string.Equals(element.Name.LocalName, "entry", StringComparison.OrdinalIgnoreCase))
                            .Select(element => new AppVersion(element))
                            .Where(
                                version =>
                                String.Equals(version.AppName, this.AppName, StringComparison.OrdinalIgnoreCase)
                                && version.Platform == PeaRoxy.Updater.AppVersion.Platforms.Windows)
                            .ToList();
                }
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        ///     This method will tell us if there is new version available for this application
        /// </summary>
        public bool IsNewVersionAvailable()
        {
            IEnumerable<AppVersion> versions = this.GetVersions();
            return versions != null && versions.Any(version => version.Version > this.AppVersion);
        }

        /// <summary>
        ///     This method will submit a SmartPear rule file to the cloud
        /// </summary>
        public bool SubmitSmartPearProfile(string xml, string profile)
        {
            const string SmartUrl = "http://reporting.pearoxy.com/submitsmartpear.php";
            try
            {
                WebClient client = new WebClient();
                client.Proxy = this.Proxy ?? new WebProxy();
                client.UploadString(SmartUrl + "?profile=" + profile.ToLower().Trim(), xml);
                return true;
            }
            catch
            {
            }
            return false;
        }

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
                client.Proxy = this.Proxy ?? new WebProxy();
                string xml = client.DownloadString(FeedUrl);
                return this.feedCache = XDocument.Parse(xml).Root;
            }
            catch
            {
            }
            return null;
        }
    }
}