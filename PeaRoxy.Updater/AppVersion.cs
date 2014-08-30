// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppVersion.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Updater
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    ///     The AppVersion class is representation of a version of program.
    /// </summary>
    public class AppVersion
    {
        /// <summary>
        ///     A list of all platforms
        /// </summary>
        public enum Platforms
        {
            Windows,

            Macintosh,

            Linux
        }

        internal AppVersion(XElement element)
        {
            const string PageDomain = "https://github.com";
            const string ProjectDownloadPath = "/PeaRoxy/PeaRoxy/releases/download";
            this.Id =
                element.Descendants()
                    .First(e => string.Equals(e.Name.LocalName, "id", StringComparison.OrdinalIgnoreCase))
                    .Value.Trim();
            this.PageLink = PageDomain
                            + element.Descendants()
                                  .First(
                                      e => string.Equals(e.Name.LocalName, "link", StringComparison.OrdinalIgnoreCase))
                                  .Attribute("href")
                                  .Value.Trim();
            this.FullName =
                element.Descendants()
                    .First(e => string.Equals(e.Name.LocalName, "title", StringComparison.OrdinalIgnoreCase))
                    .Value.Trim();
            this.AppName = this.FullName.Split(' ')[0].Trim();
            if (this.FullName.IndexOf("Win", StringComparison.OrdinalIgnoreCase) > -1)
            {
                this.Platform = Platforms.Windows;
            }
            else if (this.FullName.IndexOf("Mac", StringComparison.OrdinalIgnoreCase) > -1)
            {
                this.Platform = Platforms.Macintosh;
            }
            else if (this.FullName.IndexOf("Linux", StringComparison.OrdinalIgnoreCase) > -1)
            {
                this.Platform = Platforms.Linux;
            }
            this.Version = new Version(this.Id.Split('/').Last().Trim());
            this.DownloadLink = string.Format(
                "{0}{1}/{2}/{3}-{4}-v{2}.exe",
                PageDomain,
                ProjectDownloadPath,
                this.Version,
                this.AppName,
                this.Platform);
        }

        /// <summary>
        ///     Gets the name of the application.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        ///     Gets the download link of the application.
        /// </summary>
        public string DownloadLink { get; private set; }

        /// <summary>
        ///     Gets the full name of the application.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        ///     Gets the download page address of the application.
        /// </summary>
        public string PageLink { get; private set; }

        /// <summary>
        ///     Gets the platform of the application.
        /// </summary>
        public Platforms Platform { get; private set; }

        /// <summary>
        ///     Gets the version of the application.
        /// </summary>
        public Version Version { get; private set; }

        private string Id { get; set; }
    }
}