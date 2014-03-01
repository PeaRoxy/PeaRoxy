// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Grabber.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for Grabber.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    using LukeSw.Windows.Forms;

    using PeaRoxy.Windows.WPFClient.Properties;

    using Shell32;

    using DataFormats = System.Windows.DataFormats;
    using DragEventArgs = System.Windows.DragEventArgs;

    #endregion

    /// <summary>
    ///     Interaction logic for Grabber.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    public partial class Grabber
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Grabber" /> class.
        /// </summary>
        public Grabber()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Enums

        public enum GrabberType
        {
            None = 0,

            Tap = 1,

            Hook = 2,

            Proxy = 3,
        }

        #endregion

        #region Public Properties

        public GrabberType SelectedGrabber
        {
            get
            {
                return (GrabberType)this.ActiveGrabber.SelectedIndex;
            }
            set
            {
                this.ActiveGrabber.SelectedIndex = (int)value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The hook_ add item.
        /// </summary>
        public void HookAddItem()
        {
            this.TxtHookEditApp.Text = "";
            this.ShowOptionsDialog();
        }

        /// <summary>
        ///     The hook_ save item.
        /// </summary>
        public void HookSaveItem()
        {
            try
            {
                string app = this.TxtHookEditApp.Text.ToLower().Trim();
                if (app != string.Empty && !this.LbHookProcesses.Items.Contains(app))
                {
                    this.LbHookProcesses.Items.Add(app);
                }

                this.SaveSettings();
                this.HideOptionsDialog();
            }
            catch (Exception e)
            {
                VDialog.Show(
                    "Can't add process name: " + e.Message,
                    "PeaRoxy Client - Hook Processes Update",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
            }
        }

        /// <summary>
        ///     The load settings.
        /// </summary>
        public override void LoadSettings()
        {
            this.IsLoading = true;

            List<string> hookProcesses =
                new List<string>(
                    Settings.Default.Hook_Processes.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries));
            this.LbHookProcesses.Items.Clear();
            foreach (string p in hookProcesses)
            {
                this.LbHookProcesses.Items.Add(p);
            }

            this.TxtTapIpaddress.Text = Settings.Default.TAP_IPRange;
            this.ActiveGrabber.SelectedIndex = Settings.Default.Grabber;
            this.IsLoading = false;
        }

        /// <summary>
        ///     The save settings.
        /// </summary>
        public override void SaveSettings()
        {
            if (this.IsLoading)
            {
                return;
            }

            Settings.Default.Grabber = this.ActiveGrabber.SelectedIndex;
            Settings.Default.TAP_IPRange = this.TxtTapIpaddress.Text;
            Settings.Default.Hook_Processes = string.Join(Environment.NewLine, this.LbHookProcesses.Items.OfType<string>().ToArray());

            Settings.Default.Save();
        }

        /// <summary>
        ///     The set enable.
        /// </summary>
        /// <param name="enable">
        ///     The enable.
        /// </param>
        public override void SetEnable(bool enable)
        {
            this.SettingsGrid.IsEnabled = enable;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The hook add click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void BtnHookAddClick(object sender, RoutedEventArgs e)
        {
            this.HookAddItem();
        }

        /// <summary>
        ///     The hook cancel-edit click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void BtnHookEditCancelClick(object sender, RoutedEventArgs e)
        {
            this.HideOptionsDialog();
        }

        /// <summary>
        ///     The hook ok-edit click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void BtnHookEditOkClick(object sender, RoutedEventArgs e)
        {
            this.HookSaveItem();
        }

        /// <summary>
        ///     The hook remove click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void BtnHookRemoveClick(object sender, RoutedEventArgs e)
        {
            string[] pc = new string[this.LbHookProcesses.SelectedItems.Count];
            this.LbHookProcesses.SelectedItems.CopyTo(pc, 0);
            foreach (string s in pc)
            {
                this.LbHookProcesses.Items.Remove(s);
            }

            this.LbHookProcesses.SelectedItems.Clear();
            this.SaveSettings();
        }

        /// <summary>
        ///     The grabber selection changed.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void CbGrabberActiveSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SaveSettings();
        }

        /// <summary>
        ///     The hide options dialog.
        /// </summary>
        private void HideOptionsDialog()
        {
            this.GridOptionsdialog.IsEnabled = false;
            DoubleAnimation showAnimation = new DoubleAnimation(0, -250, new Duration(TimeSpan.FromSeconds(0.5)))
                                                {
                                                    EasingFunction
                                                        =
                                                        new ElasticEase
                                                        (
                                                        )
                                                };
            ((ElasticEase)showAnimation.EasingFunction).EasingMode = EasingMode.EaseIn;
            ((ElasticEase)showAnimation.EasingFunction).Oscillations = 1;
            TranslateTransform tt2 = new TranslateTransform();
            this.GridOptionsdialog.RenderTransform = tt2;
            tt2.BeginAnimation(TranslateTransform.YProperty, showAnimation);
            new Thread(
                delegate()
                    {
                        Thread.Sleep(500);
                        this.Dispatcher.Invoke(
                            (App.SimpleVoidDelegate)
                            delegate
                                {
                                    this.GbHook.IsEnabled =
                                        this.gb_tap.IsEnabled =
                                        this.ActiveGrabber.IsEditable = this.LblGrabberActive.IsEnabled = true;
                                },
                            new object[] { });
                    }) { IsBackground = true }.Start();
        }

        private void HookProcessesDrop(object sender, DragEventArgs e)
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
                            if (app != string.Empty && !this.LbHookProcesses.Items.Contains(app))
                            {
                                this.LbHookProcesses.Items.Add(app);
                                this.LbHookProcesses.ScrollIntoView(app);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            this.SaveSettings();
        }

        /// <summary>
        ///     The show options dialog.
        /// </summary>
        private void ShowOptionsDialog()
        {
            this.GbHook.IsEnabled =
                this.gb_tap.IsEnabled = this.ActiveGrabber.IsEditable = this.LblGrabberActive.IsEnabled = false;
            DoubleAnimation showAnimation = new DoubleAnimation(-250, 0, new Duration(TimeSpan.FromSeconds(0.5)))
                                                {
                                                    EasingFunction
                                                        =
                                                        new ElasticEase
                                                        (
                                                        )
                                                };
            ((ElasticEase)showAnimation.EasingFunction).EasingMode = EasingMode.EaseOut;
            ((ElasticEase)showAnimation.EasingFunction).Oscillations = 50;
            TranslateTransform tt2 = new TranslateTransform();
            this.GridOptionsdialog.RenderTransform = tt2;
            tt2.BeginAnimation(TranslateTransform.YProperty, showAnimation);
            new Thread(
                delegate()
                    {
                        Thread.Sleep(1000);
                        this.Dispatcher.Invoke(
                            (App.SimpleVoidDelegate)delegate { this.GridOptionsdialog.IsEnabled = true; },
                            new object[] { });
                    }) { IsBackground = true }.Start();
        }

        /// <summary>
        ///     The TAP IPAddress lost focus.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void TxtTapIpaddressLostFocus(object sender, RoutedEventArgs e)
        {
            IPAddress ip;
            if (this.TxtTapIpaddress.Text.Split('.').Length != 4
                || !IPAddress.TryParse(this.TxtTapIpaddress.Text, out ip) || ip.GetAddressBytes()[3] != 0)
            {
                this.TxtTapIpaddress.Text = "10.0.0.0";
                VDialog.Show(
                    "IP address is not acceptable.",
                    "Data Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtTapIpaddress.Focus()),
                                new object[] { });
                        }) { IsBackground = true }.Start();
            }
            else
            {
                this.TxtTapIpaddress.Text = ip.ToString();
            }

            this.SaveSettings();
        }

        #endregion
    }
}