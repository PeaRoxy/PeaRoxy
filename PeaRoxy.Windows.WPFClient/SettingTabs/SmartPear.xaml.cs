// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmartPear.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for SmartPear.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    using LukeSw.Windows.Forms;

    using PeaRoxy.Windows.WPFClient.Properties;

    #endregion

    /// <summary>
    ///     Interaction logic for SmartPear.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class SmartPear
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartPear"/> class.
        /// </summary>
        public SmartPear()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add to smart list.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="https">
        /// The https.
        /// </param>
        public void AddToSmartList(string item, bool https)
        {
            if (https)
            {
                this.SmartList.Items.Add("(Direct) " + item.ToLower());
            }
            else
            {
                this.SmartList.Items.Add("(Http) " + item.ToLower());
            }

            this.SaveSettings();
        }

        /// <summary>
        /// The fill smart list.
        /// </summary>
        public void FillSmartList()
        {
            this.SmartList.Items.Clear();
            IEnumerable<string> httpListAnalysed =
                ClientLibrary.SmartPear.AnalyzeHttpList(
                    new List<string>(
                        Settings.Default.Smart_HTTP_List.Split(
                            new[] { Environment.NewLine }, 
                            StringSplitOptions.RemoveEmptyEntries)));
            IEnumerable<string> httpsListAnalysed =
                ClientLibrary.SmartPear.AnalyzeDirectList(
                    new List<string>(
                        Settings.Default.Smart_Direct_List.Split(
                            new[] { Environment.NewLine }, 
                            StringSplitOptions.RemoveEmptyEntries)));
            foreach (string s in httpListAnalysed)
            {
                this.SmartList.Items.Add("(Http) " + s.ToLower());
            }

            foreach (string s in httpsListAnalysed)
            {
                this.SmartList.Items.Add("(Direct) " + s.ToLower());
            }
        }

        /// <summary>
        /// The load settings.
        /// </summary>
        public override void LoadSettings()
        {
            this.IsLoading = true;
            this.HttpEnable.IsChecked = Settings.Default.Smart_HTTP_Enable;
            this.CbSmartHttpAutoroute.IsChecked = Settings.Default.Smart_HTTP_AutoRoute_Enable;
            this.TxtSmartHttpAutoroute.Text = Settings.Default.Smart_HTTP_AutoRoute_Pattern;
            this.HttpsEnable.IsChecked = Settings.Default.Smart_HTTPS_Enable;
            this.CbSmartTimeout.IsChecked = Settings.Default.Smart_Timeout_Enable;
            this.TxtSmartTimeout.Text = Settings.Default.Smart_Timeout_Value.ToString(CultureInfo.InvariantCulture);
            this.CbSmartAntidns.IsChecked = Settings.Default.Smart_AntiDNS_Enable;
            this.TxtSmartAntidns.Text = Settings.Default.Smart_AntiDNSPattern;
            this.CbSmartPort80Ashttp.IsChecked = Settings.Default.Smart_Direct_Port80Router;
            this.SocketEnable.IsChecked = Settings.Default.Smart_SOCKS_Enable;
            this.CbSmartPort80Checkhttpautoroutepattern.IsChecked =
                Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP;
            this.FillSmartList();
            this.IsLoading = false;
        }

        /// <summary>
        /// The save settings.
        /// </summary>
        public override void SaveSettings()
        {
            if (this.IsLoading)
            {
                return;
            }

            Settings.Default.Smart_HTTP_Enable = this.HttpEnable.IsChecked ?? false;
            Settings.Default.Smart_HTTP_AutoRoute_Enable = this.CbSmartHttpAutoroute.IsChecked ?? false;
            Settings.Default.Smart_HTTP_AutoRoute_Pattern = this.TxtSmartHttpAutoroute.Text;
            Settings.Default.Smart_AntiDNS_Enable = this.CbSmartAntidns.IsChecked ?? false;
            Settings.Default.Smart_AntiDNSPattern = this.TxtSmartAntidns.Text;
            Settings.Default.Smart_HTTPS_Enable = this.HttpsEnable.IsChecked ?? false;
            Settings.Default.Smart_Timeout_Enable = this.CbSmartTimeout.IsChecked ?? false;
            Settings.Default.Smart_Timeout_Value = Convert.ToUInt16(this.TxtSmartTimeout.Text);
            Settings.Default.Smart_Direct_Port80Router = this.CbSmartPort80Ashttp.IsChecked ?? false;
            Settings.Default.Smart_SOCKS_Enable = this.SocketEnable.IsChecked ?? false;
            Settings.Default.Smart_Direct_List = string.Empty;
            Settings.Default.Smart_HTTP_List = string.Empty;
            Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP = this.CbSmartPort80Checkhttpautoroutepattern.IsChecked ?? false;
            foreach (string s in this.SmartList.Items)
            {
                string ps = s.ToLower();
                if (ps.IndexOf("(http)", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ps = ps.Substring(ps.IndexOf(") ", StringComparison.Ordinal) + 2);
                    Settings.Default.Smart_HTTP_List += ps + Environment.NewLine;
                }

                if (ps.IndexOf("(direct)", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ps = ps.Substring(ps.IndexOf(") ", StringComparison.Ordinal) + 2);
                    Settings.Default.Smart_Direct_List += ps + Environment.NewLine;
                }
            }

            Settings.Default.Save();
        }

        /// <summary>
        /// The set enable.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public override void SetEnable(bool enable)
        {
            this.SettingsGrid.IsEnabled =
                this.GridOptionsdialog.IsEnabled =
                this.AddButton.IsEnabled = this.EditButton.IsEnabled = this.RemoveButton.IsEnabled = enable;
        }

        /// <summary>
        /// The smart_ add item.
        /// </summary>
        public void SmartAddItem()
        {
            this.TxtSmartEditRule.Text = string.Empty;
            this.TxtSmartEditRule.Tag = null;
            this.CbSmartEditType.SelectedItem = this.CbSmartEditType.Items[0];
            this.ShowOptionsDialog();
        }

        /// <summary>
        /// The smart_ edit item.
        /// </summary>
        public void SmartEditItem()
        {
            if (this.SmartList.SelectedItems.Count < 1)
            {
                return;
            }

            string rule = this.SmartList.SelectedItems[0].ToString();
            string ps = rule.ToLower();
            if (ps.IndexOf("(http)", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.CbSmartEditType.SelectedItem = this.CbSmartEditType.Items[0];
            }

            if (ps.IndexOf("(direct)", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.CbSmartEditType.SelectedItem = this.CbSmartEditType.Items[1];
            }

            this.TxtSmartEditRule.Tag = rule;
            ps = ps.Substring(ps.IndexOf(") ", StringComparison.Ordinal) + 2);
            this.TxtSmartEditRule.Text = ps;
            this.ShowOptionsDialog();
        }

        /// <summary>
        /// The smart_ save or add item.
        /// </summary>
        /// <exception cref="Exception">
        /// Empty rule detected
        /// </exception>
        public void SmartSaveOrAddItem()
        {
            try
            {
                string rule = this.TxtSmartEditRule.Text.ToLower();
                if (rule.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    rule = rule.Substring(rule.IndexOf("://", StringComparison.Ordinal) + 3);
                }

                if (rule.IndexOf("|", StringComparison.Ordinal) != -1)
                {
                    rule = rule.Substring(rule.IndexOf("|", StringComparison.Ordinal) + 1).Trim();
                }

                rule = rule.Replace("\\", "/").Replace("//", "/").Replace("//", "/").Replace("//", "/");
                if (rule == string.Empty)
                {
                    throw new Exception("Rule can not be empty.");
                }

                bool https = false;
                switch (((ComboBoxItem)this.CbSmartEditType.SelectedItem).Tag.ToString().ToLower())
                {
                    case "direct":
                        https = true;
                        if (rule.IndexOf("/", StringComparison.Ordinal) != -1)
                        {
                            throw new Exception("Direct rules cannot contain slash.");
                        }

                        if (rule.IndexOf(":", StringComparison.Ordinal) == -1)
                        {
                            throw new Exception("Direct rules must have port number.");
                        }

                        break;
                    case "http":

                        break;
                    default:
                        return;
                }

                int cp = -1;
                if (this.TxtSmartEditRule.Tag != null)
                {
                    cp = this.SmartList.Items.IndexOf(this.TxtSmartEditRule.Tag);
                    this.SmartList.Items.RemoveAt(cp);
                }

                if (cp == -1)
                {
                    cp = this.SmartList.Items.Count;
                }

                if (https)
                {
                    this.SmartList.Items.Insert(cp, "(Direct) " +  rule);
                }
                else
                {
                    this.SmartList.Items.Insert(cp, "(Http) " + rule);
                }

                this.SaveSettings();
                this.HideOptionsDialog();
            }
            catch (Exception e)
            {
                VDialog.Show(
                    "Can't add or edit rule: " + e.Message, 
                    "PeaRoxy Client - Smart List Update", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Stop);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The hide options dialog.
        /// </summary>
        private void HideOptionsDialog()
        {
            this.GridOptionsdialog.IsEnabled = false;
            DoubleAnimation showAnimation = new DoubleAnimation(0, -250, new Duration(TimeSpan.FromSeconds(0.5)))
                                                {
                                                    EasingFunction = new ElasticEase()
                                                };
            ((ElasticEase)showAnimation.EasingFunction).EasingMode = EasingMode.EaseIn;
            ((ElasticEase)showAnimation.EasingFunction).Oscillations = 1;
            TranslateTransform translateTransform = new TranslateTransform();
            this.GridOptionsdialog.RenderTransform = translateTransform;
            translateTransform.BeginAnimation(TranslateTransform.YProperty, showAnimation);
            new Thread(
                delegate()
                    {
                        Thread.Sleep(500);
                        this.Dispatcher.Invoke(
                            (App.SimpleVoidDelegate)
                            delegate
                                {
                                    this.SettingsGrid.IsEnabled =
                                        this.SmartList.IsEnabled =
                                        this.AddButton.IsEnabled =
                                        this.EditButton.IsEnabled = this.RemoveButton.IsEnabled = true;
                                }, 
                            new object[] { });
                    }) {
                          IsBackground = true 
                       }.Start();
        }

        /// <summary>
        /// The show options dialog.
        /// </summary>
        private void ShowOptionsDialog()
        {
            this.SettingsGrid.IsEnabled =
                this.SmartList.IsEnabled =
                this.AddButton.IsEnabled = this.EditButton.IsEnabled = this.RemoveButton.IsEnabled = false;
            DoubleAnimation showAnimation = new DoubleAnimation(-250, 0, new Duration(TimeSpan.FromSeconds(0.5)))
                                                {
                                                    EasingFunction = new ElasticEase()
                                                };
            ((ElasticEase)showAnimation.EasingFunction).EasingMode = EasingMode.EaseOut;
            ((ElasticEase)showAnimation.EasingFunction).Oscillations = 50;
            TranslateTransform translateTransform = new TranslateTransform();
            this.GridOptionsdialog.RenderTransform = translateTransform;
            translateTransform.BeginAnimation(TranslateTransform.YProperty, showAnimation);
            new Thread(
                delegate()
                    {
                        Thread.Sleep(1000);
                        this.Dispatcher.Invoke(
                            (App.SimpleVoidDelegate)delegate { this.GridOptionsdialog.IsEnabled = true; }, 
                            new object[] { });
                    }) {
                          IsBackground = true 
                       }.Start();
        }

        /// <summary>
        /// The btn_smart_add_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnSmartAddClick(object sender, RoutedEventArgs e)
        {
            this.SmartAddItem();
        }

        /// <summary>
        /// The btn_smart_edit_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnSmartEditClick(object sender, RoutedEventArgs e)
        {
            this.SmartEditItem();
        }

        /// <summary>
        /// The btn_smart_edit_cancel_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnSmartEditCancelClick(object sender, RoutedEventArgs e)
        {
            this.HideOptionsDialog();
        }

        /// <summary>
        /// The btn_smart_edit_ok_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnSmartEditOkClick(object sender, RoutedEventArgs e)
        {
            this.SmartSaveOrAddItem();
        }

        /// <summary>
        /// The btn_smart_remove_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnSmartRemoveClick(object sender, RoutedEventArgs e)
        {
            string[] pc = new string[this.SmartList.SelectedItems.Count];
            this.SmartList.SelectedItems.CopyTo(pc, 0);
            foreach (string s in pc)
            {
                this.SmartList.Items.Remove(s);
            }

            this.SmartList.SelectedItems.Clear();
            this.SaveSettings();
        }

        /// <summary>
        /// The cb_smart_antidns_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbSmartAntidnsChecked(object sender, RoutedEventArgs e)
        {
            this.TxtSmartAntidns.IsEnabled = this.CbSmartAntidns.IsChecked ?? false;
            this.SaveSettings();
        }

        /// <summary>
        /// The cb_smart_http_autoroute_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbSmartHttpAutorouteChecked(object sender, RoutedEventArgs e)
        {
            this.TxtSmartHttpAutoroute.IsEnabled = this.CbSmartHttpAutoroute.IsChecked ?? false;
            this.CbSmartPort80Checkhttpautoroutepattern.IsChecked = this.CbSmartPort80Checkhttpautoroutepattern.IsEnabled =
                (this.CbSmartPort80Ashttp.IsChecked ?? false) && (this.CbSmartHttpAutoroute.IsChecked ?? false);
            this.SaveSettings();
        }

        /// <summary>
        /// The cb_smart_http_enable_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbSmartHttpEnableChecked(object sender, RoutedEventArgs e)
        {
            this.TxtSmartHttpAutoroute.IsEnabled = this.HttpEnable.IsChecked ?? false;
            this.CbSmartHttpAutoroute.IsEnabled = this.HttpEnable.IsChecked ?? false;
            this.CbSmartPort80Ashttp.IsChecked = this.CbSmartPort80Checkhttpautoroutepattern.IsChecked =
                this.CbSmartPort80Ashttp.IsEnabled = this.CbSmartPort80Checkhttpautoroutepattern.IsEnabled =
                (this.HttpsEnable.IsChecked ?? false) && (this.HttpEnable.IsChecked ?? false);
            this.SaveSettings();
        }

        /// <summary>
        /// The cb_smart_https_enable_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbSmartHttpsEnableChecked(object sender, RoutedEventArgs e)
        {
            this.CbSmartPort80Ashttp.IsChecked =
                this.CbSmartPort80Checkhttpautoroutepattern.IsChecked =
                this.CbSmartPort80Ashttp.IsEnabled =
                this.CbSmartPort80Checkhttpautoroutepattern.IsEnabled =
                (this.HttpsEnable.IsChecked ?? false) && (this.HttpEnable.IsChecked ?? false);
            this.SocketEnable.IsEnabled = this.HttpsEnable.IsChecked ?? false;
            this.SaveSettings();
        }

        /// <summary>
        /// The cb_smart_port 80 ashttp_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbSmartPort80AshttpChecked(object sender, RoutedEventArgs e)
        {
            this.CbSmartPort80Checkhttpautoroutepattern.IsChecked =
                this.CbSmartPort80Checkhttpautoroutepattern.IsEnabled =
                (this.CbSmartPort80Ashttp.IsChecked ?? false) && (this.CbSmartHttpAutoroute.IsChecked ?? false);
            this.SaveSettings();
        }

        /// <summary>
        /// The cb_smart_timeout_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbSmartTimeoutChecked(object sender, RoutedEventArgs e)
        {
            this.TxtSmartTimeout.IsEnabled = this.CbSmartTimeout.IsChecked ?? false;
            this.SaveSettings();
        }

        /// <summary>
        /// The lb_smart_ mouse dc.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void SmartListMouseDc(object sender, MouseButtonEventArgs e)
        {
            if (this.EditButton.IsEnabled)
            {
                this.SmartEditItem();
            }
        }

        /// <summary>
        /// The txt_ text box_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            this.SaveSettings();
        }

        /// <summary>
        /// The txt_smart_timeout_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtSmartTimeoutLostFocus(object sender, RoutedEventArgs e)
        {
            short u;
            if (!short.TryParse(this.TxtSmartTimeout.Text, out u))
            {
                VDialog.Show(
                    "Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.TxtSmartTimeout.Text = 20.ToString(CultureInfo.InvariantCulture);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtSmartTimeout.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.TxtSmartTimeout.Text = u.ToString(CultureInfo.InvariantCulture);
            }

            this.SaveSettings();
        }

        #endregion
    }
}