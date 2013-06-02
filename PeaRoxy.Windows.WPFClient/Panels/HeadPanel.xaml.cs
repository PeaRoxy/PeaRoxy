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
    /// Interaction logic for HeadPanel.xaml
    /// </summary>
    public partial class HeadPanel : UserControl
    {
        public static readonly RoutedEvent MinimizeClickEvent = EventManager.RegisterRoutedEvent("MinimizeClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HeadPanel));
        public event RoutedEventHandler MinimizeClick
        {
            add { AddHandler(MinimizeClickEvent, value); }
            remove { RemoveHandler(MinimizeClickEvent, value); }
        }
        public HeadPanel()
        {
            InitializeComponent();
        }

        private void img_closeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation da_IncOpa = new DoubleAnimation(0.5, 1, new Duration(TimeSpan.FromSeconds(0.3)));
            Timeline.SetDesiredFrameRate(da_IncOpa, 60); // 60 FPS
            btn_close.BeginAnimation(Button.OpacityProperty, da_IncOpa);
        }
        private void img_closeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation da_IncOpa = new DoubleAnimation(1, 0.5, new Duration(TimeSpan.FromSeconds(0.7)));
            Timeline.SetDesiredFrameRate(da_IncOpa, 60); // 60 FPS
            btn_close.BeginAnimation(Button.OpacityProperty, da_IncOpa);
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(HeadPanel.MinimizeClickEvent));
        }

        private void cc_Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
