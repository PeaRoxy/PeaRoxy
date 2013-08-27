using PeaRoxy.Windows.WPFClient.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PeaRoxy.Windows.WPFClient
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();
        }

        private void SettingsButton_SelectedChanged(object sender, RoutedEventArgs e)
        {
            SettingsButton s = sender as SettingsButton;
            SetActivePage(s);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadSettings();
            General.isSelected = true;
        }

        public void SaveSettings()
        {
            foreach (UIElement tab in wp_OptionButtons.Children)
                if (tab is UserControls.SettingsButton && ((UserControls.SettingsButton)tab).SettingsPage != null)
                    ((UserControls.SettingsButton)tab).SettingsPage.SaveSettings();
        }

        public void ReloadSettings()
        {
            foreach (UIElement tab in wp_OptionButtons.Children)
                if (tab is UserControls.SettingsButton && ((UserControls.SettingsButton)tab).SettingsPage != null)
                    ((UserControls.SettingsButton)tab).SettingsPage.LoadSettings();
        }

        public void SetState(bool isOptionsEnable, bool? isListeningOptionsEnable = null)
        {
            if (isListeningOptionsEnable == null)
                isListeningOptionsEnable = isOptionsEnable;

            foreach (UIElement tab in wp_OptionButtons.Children)
                if (tab is UserControls.SettingsButton && ((UserControls.SettingsButton)tab).SettingsPage != null)
                    ((UserControls.SettingsButton)tab).SettingsPage.SetEnable(isOptionsEnable);
            LocalListener.SettingsPage.SetEnable(isListeningOptionsEnable.Value);
        }

        private void SetActivePage(SettingsButton page)
        {
            WrapPanel w = page.Parent as WrapPanel;
            if (page.isSelected && page.SettingsPage != null)
            {
                foreach (UIElement tab in w.Children)
                {
                    if (tab is UserControls.SettingsButton && !tab.Equals(page))
                        (tab as UserControls.SettingsButton).isSelected = false;
                }
                UIElement lastPage = cc_Options.Content as UIElement;
                DoubleAnimation da_HideLastPage = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.10)));
                lastPage.BeginAnimation(UIElement.OpacityProperty, da_HideLastPage);
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(200));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        DoubleAnimation da_ShowPage = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.10)));
                        page.SettingsPage.BeginAnimation(UIElement.OpacityProperty, da_ShowPage);
                        cc_Options.Content = page.SettingsPage;
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
        }
    }
}
