namespace PeaRoxy.Windows.WPFClient
{
    #region

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using PeaRoxy.Windows.WPFClient.Properties;
    using PeaRoxy.Windows.WPFClient.SettingTabs;

    using Shell32;

    #endregion

    /// <summary>
    ///     Interaction logic for WelcomeWindow.xaml
    /// </summary>
    public partial class WelcomeWindow
    {
        #region Fields

        private bool isDone;

        #endregion

        #region Constructors and Destructors

        public WelcomeWindow()
        {
            this.InitializeComponent();
            List<string> hookProcesses =
                new List<string>(
                    Settings.Default.Hook_Processes.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries));
            this.ApplicationsListBox.Items.Clear();
            foreach (string p in hookProcesses)
            {
                this.ApplicationsListBox.Items.Add(p.Trim().ToLower());
            }

            foreach (ListBoxItem item in CountryPage.Items)
            {
                if (((item.Tag as string ?? string.Empty).Trim() == Settings.Default.SelectedProfile)
                    || ((item.Tag as string ?? "NONE").Trim() == string.Empty))
                {
                    item.IsSelected = true;
                    break;
                }
            }

            switch ((Grabber.GrabberType)Settings.Default.Grabber)
            {
                case Grabber.GrabberType.Proxy:
                    this.ProxyRadioButton.IsChecked = true;
                    break;
                case Grabber.GrabberType.Tap:
                    this.TapRadioButton.IsChecked = true;
                    break;
                case Grabber.GrabberType.Hook:
                    this.HookRadioButton.IsChecked = true;
                    break;
                default:
                    this.NoneRadioButton.IsChecked = true;
                    break;
            }
        }

        #endregion

        #region Methods

        private void ApplicationsListBoxDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                try
                {
                    if (!Directory.Exists(file) && File.Exists(file))
                    {
                        string fileExtension = Path.GetExtension(file);
                        string fileName = Path.GetFileName(file);
                        if (fileExtension != null && fileName != null && fileExtension.ToLower() == ".lnk")
                        {
                            string filePath = Path.GetDirectoryName(file);
                            Shell shell = new Shell();
                            Folder folder = shell.NameSpace(filePath);
                            FolderItem folderItem = folder.ParseName(fileName);
                            if (folderItem != null)
                            {
                                string newFile = ((ShellLinkObject)folderItem.GetLink).Path;
                                fileExtension = Path.GetExtension(newFile);
                                fileName = Path.GetFileName(newFile);
                            }
                        }

                        if (fileExtension != null && fileName != null && fileExtension.ToLower() == ".exe")
                        {
                            string app = Path.GetFileNameWithoutExtension(fileName).ToLower().Trim();
                            if (app != string.Empty && !this.ApplicationsListBox.Items.Contains(app))
                            {
                                this.ApplicationsListBox.Items.Add(app);
                                this.ApplicationsListBox.ScrollIntoView(app);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void ApplicationsListBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                string[] pc = new string[this.ApplicationsListBox.SelectedItems.Count];
                this.ApplicationsListBox.SelectedItems.CopyTo(pc, 0);
                foreach (string s in pc)
                {
                    this.ApplicationsListBox.Items.Remove(s);
                }

                this.ApplicationsListBox.SelectedItems.Clear();
            }
        }

        private void GrabberRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
            {
                return;
            }
            bool? @checked = this.HookRadioButton.IsChecked;
            if (@checked != null && @checked.Value)
            {
                this.GrabberPage.NextPage = this.ApplicationsPage;
                this.FinishPage.PreviousPage = this.ApplicationsPage;
            }
            else
            {
                this.GrabberPage.NextPage = this.FinishPage;
                this.FinishPage.PreviousPage = this.GrabberPage;
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (this.isDone != true)
            {
                Environment.Exit(0);
            }
        }

        private void WizardFinish(object sender, RoutedEventArgs e)
        {
            Settings.Default.Welcome_Shown = true;
            Settings.Default.Save();
            this.isDone = true;
        }

        private void WizardPanelPageChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.WizardPanel.CurrentPage != null && this.WizardPanel.CurrentPage.Equals(this.FinishPage))
                {
                    this.WizardPanel.CanCancel = false;
                    this.SummeryListBox.Items.Clear();
                    ListBoxItem selectedItem = this.CountryPage.SelectedItem as ListBoxItem;
                    if (selectedItem != null)
                    {
                        Settings.Default.SelectedProfile = (selectedItem.Tag as string ?? string.Empty).Trim();
                        switch (selectedItem.Tag as string)
                        {
                            case "IR":
                                Settings.Default.Smart_HTTPS_Enable = true;
                                Settings.Default.Smart_HTTP_Enable = true;
                                Settings.Default.Smart_SOCKS_Enable = false;
                                Settings.Default.Smart_Direct_List = "*google.c*:*" + Environment.NewLine + "*facebook.c*:*" + Environment.NewLine + "*youtube.c*:*" + Environment.NewLine + "*twitter.c*:*";
                                Settings.Default.Smart_HTTP_List = "*google.c*" + Environment.NewLine + "*facebook.c*" + Environment.NewLine + "*youtube.c*" + Environment.NewLine + "*twitter.c*";
                                Settings.Default.Smart_HTTP_AutoRoute_Enable = true;
                                Settings.Default.Smart_HTTP_AutoRoute_Pattern =
                                    "^HTTP/1.1 403 Forbidden(\\r\\nConnection:close)*";
                                Settings.Default.Smart_AntiDNS_Enable = true;
                                Settings.Default.Smart_AntiDNSPattern = "^(10.10.*.*)$";
                                Settings.Default.Smart_Timeout_Enable = true;
                                Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP = true;
                                this.SummeryListBox.Items.Add("SmartPear for HTTP connections: Enable");
                                this.SummeryListBox.Items.Add("SmartPear for Direct connections: Enable");
                                this.SummeryListBox.Items.Add("SmartPear Profile: Iran");
                                break;
                            case "CH":
                                Settings.Default.Smart_HTTPS_Enable = false;
                                Settings.Default.Smart_HTTP_Enable = false;
                                Settings.Default.Smart_SOCKS_Enable = false;
                                Settings.Default.Smart_Direct_List = "*google.c*:*" + Environment.NewLine + "*facebook.c*:*" + Environment.NewLine + "*youtube.c*:*" + Environment.NewLine + "*twitter.c*:*";
                                Settings.Default.Smart_HTTP_List = "*google.c*" + Environment.NewLine + "*facebook.c*" + Environment.NewLine + "*youtube.c*" + Environment.NewLine + "*twitter.c*";
                                Settings.Default.Smart_HTTP_AutoRoute_Enable = false;
                                Settings.Default.Smart_HTTP_AutoRoute_Pattern = "";
                                Settings.Default.Smart_AntiDNS_Enable = true;
                                Settings.Default.Smart_AntiDNSPattern = "^(78.16.49.15|46.82.174.68|243.185.187.39|37.61.54.158|159.106.121.75|59.24.3.173|8.7.198.45|93.46.8.89|203.98.7.65)$";
                                Settings.Default.Smart_Timeout_Enable = true;
                                Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP = true;
                                this.SummeryListBox.Items.Add("SmartPear for HTTP connections: Disable");
                                this.SummeryListBox.Items.Add("SmartPear for Direct connections: Disable");
                                this.SummeryListBox.Items.Add("SmartPear Profile: China");
                                break;
                            case "AE":
                            case "SA":
                            case "PK":
                            default:
                                Settings.Default.Smart_HTTPS_Enable = false;
                                Settings.Default.Smart_HTTP_Enable = false;
                                Settings.Default.Smart_SOCKS_Enable = false;
                                Settings.Default.Smart_Direct_List = "";
                                Settings.Default.Smart_HTTP_List = "";
                                Settings.Default.Smart_HTTP_AutoRoute_Enable = false;
                                Settings.Default.Smart_HTTP_AutoRoute_Pattern = "";
                                Settings.Default.Smart_AntiDNS_Enable = false;
                                Settings.Default.Smart_AntiDNSPattern = "";
                                Settings.Default.Smart_Timeout_Enable = false;
                                Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP = false;
                                this.SummeryListBox.Items.Add("SmartPear for HTTP connections: Disable");
                                this.SummeryListBox.Items.Add("SmartPear for Direct connections: Disable");
                                break;
                        }
                    }

                    if (this.TapRadioButton.IsChecked == true)
                    {
                        Settings.Default.Grabber = (int)Grabber.GrabberType.Tap;
                    }
                    else if (this.HookRadioButton.IsChecked == true)
                    {
                        Settings.Default.Grabber = (int)Grabber.GrabberType.Hook;
                    }
                    else if (this.ProxyRadioButton.IsChecked == true)
                    {
                        Settings.Default.Grabber = (int)Grabber.GrabberType.Proxy;
                    }
                    else
                    {
                        Settings.Default.Grabber = (int)Grabber.GrabberType.None;
                    }

                    this.SummeryListBox.Items.Add(
                        "Selected Traffic Grabber: " + (Grabber.GrabberType)Settings.Default.Grabber);

                    if (this.HookRadioButton.IsChecked == true)
                    {
                        Settings.Default.Hook_Processes = string.Join(
                            Environment.NewLine,
                            this.ApplicationsListBox.Items.OfType<string>().ToArray());
                        this.SummeryListBox.Items.Add(
                            "Hook into following applications: "
                            + string.Join(" | ", this.ApplicationsListBox.Items.OfType<string>().ToArray()));
                    }
                }
                else
                {
                    this.WizardPanel.CanCancel = true;
                }
            }
            catch (Exception)
            {
            }
        }

        private void WizardSkip(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            Settings.Default.Welcome_Shown = true;
            Settings.Default.Save();
            this.isDone = true;
        }

        #endregion
    }
}