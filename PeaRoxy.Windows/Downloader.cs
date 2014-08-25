namespace PeaRoxy.Windows
{
    #region

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Windows.Forms;

    using LukeSw.Windows.Forms;

    using PeaRoxy.Updater;

    #endregion

    public partial class Downloader : Form
    {
        #region Fields

        private WebClient downloader;

        #endregion

        #region Constructors and Destructors

        public Downloader(AppVersion appVersion, WebProxy proxy = null)
        {
            this.InitializeComponent();
            this.Proxy = proxy;
            this.label.Text = this.label.Text.Replace("%s", appVersion.FullName);
            this.Filename = Path.Combine(
                Path.GetTempPath(),
                Process.GetCurrentProcess().Id + "_" + appVersion.AppName + "_Updater.exe");

            this.DownloadUrl = appVersion.DownloadLink;
            this.ExecuteAtEnd = true;
        }

        public Downloader(Uri downloadUrl, bool execute = false, WebProxy proxy = null)
        {
            this.InitializeComponent();
            this.Proxy = proxy;
            this.label.Text = this.label.Text.Replace("%s", Path.GetFileName(downloadUrl.AbsolutePath));
            this.Filename = Path.Combine(
                Path.GetTempPath(),
                Process.GetCurrentProcess().Id + "_" + Path.GetFileName(downloadUrl.AbsolutePath));
            this.DownloadUrl = downloadUrl.AbsoluteUri;
            this.ExecuteAtEnd = execute;
        }

        #endregion

        #region Public Properties

        public string DownloadUrl { get; private set; }

        public bool ExecuteAtEnd { get; private set; }

        public string Filename { get; private set; }

        public WebProxy Proxy { get; set; }

        #endregion

        #region Public Methods and Operators

        public new DialogResult ShowDialog()
        {
            this.Download();
            return base.ShowDialog();
        }

        #endregion

        #region Methods

        private void Download()
        {
            this.downloader = new WebClient { Proxy = this.Proxy ?? new WebProxy() };
            this.downloader.DownloadProgressChanged +=
                (DownloadProgressChangedEventHandler)delegate(object s, DownloadProgressChangedEventArgs ea)
                    {
                        // ReSharper disable once RedundantCheckBeforeAssignment
                        if (this.progressBar.Style != ProgressBarStyle.Continuous)
                        {
                            this.progressBar.Style = ProgressBarStyle.Continuous;
                        }
                        this.progressBar.Value = ea.ProgressPercentage;
                    };
            this.downloader.DownloadFileCompleted +=
                (AsyncCompletedEventHandler)delegate(object s, AsyncCompletedEventArgs ea)
                    {
                        if (!this.Visible)
                        {
                            return;
                        }
                        this.progressBar.Style = ProgressBarStyle.Marquee;
                        this.progressBar.Value = 0;
                        if (ea.Cancelled || ea.Error != null)
                        {
                            if (VDialog.Show(
                                this,
                                @"Failed to download update file.",
                                @"Download Error",
                                MessageBoxButtons.RetryCancel,
                                MessageBoxIcon.Exclamation) == DialogResult.Retry)
                            {
                                this.Download();
                            }
                            else
                            {
                                this.DialogResult = DialogResult.Abort;
                                this.Close();
                            }
                        }
                        else if (this.ExecuteAtEnd)
                        {
                            try
                            {
                                if (File.Exists(this.Filename))
                                {
                                    Process.Start(this.Filename);
                                    Environment.Exit(0);
                                }
                            }
                            catch (Exception)
                            {
                                VDialog.Show(
                                    this,
                                    @"Failed to execute update file.",
                                    @"Update Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                            this.DialogResult = DialogResult.Abort;
                            this.Close();
                        }
                        else
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    };
            this.downloader.DownloadFileAsync(new Uri(this.DownloadUrl), this.Filename, this.downloader);
        }

        private void DownloaderFormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.downloader != null && this.downloader.IsBusy)
            {
                this.downloader.CancelAsync();
                this.DialogResult = DialogResult.Cancel;
            }
        }

        #endregion
    }
}