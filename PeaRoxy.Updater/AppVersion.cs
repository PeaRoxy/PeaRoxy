namespace PeaRoxy.Updater
{
    #region

    using System;
    using System.Linq;
    using System.Xml.Linq;

    #endregion

    public class AppVersion
    {
        #region Constructors and Destructors

        public AppVersion(XElement element)
        {
            const string PageDomain = "https://github.com";
            const string ProjectDownloadPath = "/PeaRoxy/PeaRoxy/releases/download";
            this.Id =
                element.Descendants()
                    .First(e => string.Equals(e.Name.LocalName, "id", StringComparison.CurrentCultureIgnoreCase))
                    .Value.Trim();
            this.PageLink = PageDomain
                            + element.Descendants()
                                  .First(
                                      e =>
                                      string.Equals(e.Name.LocalName, "link", StringComparison.CurrentCultureIgnoreCase))
                                  .Attribute("href")
                                  .Value.Trim();
            this.FullName =
                element.Descendants()
                    .First(e => string.Equals(e.Name.LocalName, "title", StringComparison.CurrentCultureIgnoreCase))
                    .Value.Trim();
            this.AppName = this.FullName.Split(' ')[0].Trim();
            if (this.FullName.IndexOf("Win", StringComparison.CurrentCultureIgnoreCase) > -1)
            {
                this.Platform = Platforms.Win;
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

        #endregion

        #region Enums

        public enum Platforms
        {
            Win,
        }

        #endregion

        #region Public Properties

        public string AppName { get; private set; }

        public string DownloadLink { get; private set; }

        public string FullName { get; private set; }

        public string PageLink { get; private set; }

        public Platforms Platform { get; private set; }

        public Version Version { get; private set; }

        #endregion

        #region Properties

        private string Id { get; set; }

        #endregion
    }
}