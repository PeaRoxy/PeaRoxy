using PeaRoxy.Windows.WPFClient.SettingTabs;
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

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsButton.xaml
    /// </summary>
    public partial class SettingsButton : UserControl
    {
        public event RoutedEventHandler SelectedChanged;
        public Base SettingsPage { get; set; }
        public bool isSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    if (_isSelected)
                    {
                        DoubleAnimation da_ShowS = new DoubleAnimation(5, 20, new Duration(TimeSpan.FromSeconds(0.3)));
                        Timeline.SetDesiredFrameRate(da_ShowS, 60); // 60 FPS
                        TranslateTransform tt2 = new TranslateTransform();
                        _button.RenderTransform = tt2;
                        da_ShowS.DecelerationRatio = 0.8;
                        tt2.BeginAnimation(TranslateTransform.XProperty, da_ShowS);
                    }
                    else
                    {
                        DoubleAnimation da_HideL = new DoubleAnimation(20, 0, new Duration(TimeSpan.FromSeconds(0.2)));
                        Timeline.SetDesiredFrameRate(da_HideL, 60); // 60 FPS
                        TranslateTransform tt = new TranslateTransform();
                        _button.RenderTransform = tt;
                        tt.BeginAnimation(TranslateTransform.XProperty, da_HideL);
                        DoubleAnimation da_IncOpa = new DoubleAnimation(1, 0.7, new Duration(TimeSpan.FromSeconds(0.3)));
                        Timeline.SetDesiredFrameRate(da_IncOpa, 60); // 60 FPS
                        _button.BeginAnimation(Button.OpacityProperty, da_IncOpa);
                    }
                    if (SelectedChanged != null)
                        SelectedChanged(this, new RoutedEventArgs());
                }
            }
        }
        private bool _isSelected = false;
        public string TooltipTitle { get { return ((Tooltip)_button.ToolTip).Title; } set { ((Tooltip)_button.ToolTip).Title = value; } }
        public string TooltipText { get { return ((Tooltip)_button.ToolTip).Text; } set { ((Tooltip)_button.ToolTip).Text = value; } }
        public string Text { get { return (string)_label.Content; } set { _label.Content = value; } }
        public ImageSource Image { get { return _image.Source; } set { _image.Source = value; } }
        public SettingsButton()
        {
            InitializeComponent();
        }

        private void _button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isSelected) return;
            if ((SettingsPage != null && !_button.Equals(SettingsPage)) || SettingsPage == null)
            {
                DoubleAnimation da_ShowS = new DoubleAnimation(0, 5, new Duration(TimeSpan.FromSeconds(0.3)));
                Timeline.SetDesiredFrameRate(da_ShowS, 60); // 60 FPS
                TranslateTransform tt = new TranslateTransform();
                _button.RenderTransform = tt;
                da_ShowS.DecelerationRatio = 0.8;
                tt.BeginAnimation(TranslateTransform.XProperty, da_ShowS);

                DoubleAnimation da_IncOpa = new DoubleAnimation(0.7, 1, new Duration(TimeSpan.FromSeconds(0.3)));
                Timeline.SetDesiredFrameRate(da_IncOpa, 60); // 60 FPS
                _button.BeginAnimation(Button.OpacityProperty, da_IncOpa);
            }
        }

        private void _button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isSelected) return;
            if ((SettingsPage != null && !_button.Equals(SettingsPage)) || SettingsPage == null)
            {
                DoubleAnimation da_HideS = new DoubleAnimation(5, 0, new Duration(TimeSpan.FromSeconds(0.2)));
                Timeline.SetDesiredFrameRate(da_HideS, 60); // 60 FPS
                da_HideS.DecelerationRatio = 0.8;
                TranslateTransform tt = new TranslateTransform();
                _button.RenderTransform = tt;
                tt.BeginAnimation(TranslateTransform.XProperty, da_HideS);

                DoubleAnimation da_IncOpa = new DoubleAnimation(1, 0.7, new Duration(TimeSpan.FromSeconds(0.3)));
                Timeline.SetDesiredFrameRate(da_IncOpa, 60); // 60 FPS
                _button.BeginAnimation(Button.OpacityProperty, da_IncOpa);
            }
        }

        private void _button_Click(object sender, RoutedEventArgs e)
        {
            isSelected = true;
        }
    }
}
