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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    /// <summary>
    /// Interaction logic for StartStop.xaml
    /// </summary>
    public partial class StartStopButton : UserControl
    {
        public enum Status
        {
            Hide,
            ShowStart,
            ShowStop
        }
        public static readonly RoutedEvent MinimizeClickEvent = EventManager.RegisterRoutedEvent("MinimizeClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartStopButton));
        public event RoutedEventHandler MinimizeClick
        {
            add { AddHandler(MinimizeClickEvent, value); }
            remove { RemoveHandler(MinimizeClickEvent, value); }
        }
        public static readonly RoutedEvent StopClickEvent = EventManager.RegisterRoutedEvent("StopClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartStopButton));
        public event RoutedEventHandler StopClick
        {
            add { AddHandler(StopClickEvent, value); }
            remove { RemoveHandler(StopClickEvent, value); }
        }
        public static readonly RoutedEvent StartClickEvent = EventManager.RegisterRoutedEvent("StartClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartStopButton));
        public event RoutedEventHandler StartClick
        {
            add { AddHandler(StartClickEvent, value); }
            remove { RemoveHandler(StartClickEvent, value); }
        }
        Status desiredStatus = Status.Hide;
        Status currentStatus = Status.Hide;
        bool inAnimation = false;
        public Status CurrentStatus
        {
            set
            {
                desiredStatus = value;
                if (inAnimation)
                    return;

                if (currentStatus != value)
                {
                    inAnimation = true;
                    btn_disconnect.IsEnabled = false;
                    btn_connect.IsEnabled = false;
                    btn_exit.IsEnabled = false;
                    btn_disconnect.Visibility = System.Windows.Visibility.Hidden;
                    btn_connect.Visibility = System.Windows.Visibility.Hidden;
                    btn_exit.Visibility = System.Windows.Visibility.Hidden;
                    cc3d_Exit.AnimationLength = 800;
                    cc3d_Stop_Start.AnimationLength = 900;
                    if (value == Status.ShowStop)
                    {
                        btn_disconnect.Visibility = System.Windows.Visibility.Visible;
                        btn_exit.Visibility = System.Windows.Visibility.Visible;
                        if (currentStatus != Status.Hide)
                            btn_connect.Visibility = System.Windows.Visibility.Visible;
                    }
                    else if (value == Status.ShowStart)
                    {
                        btn_connect.Visibility = System.Windows.Visibility.Visible;
                        btn_exit.Visibility = System.Windows.Visibility.Visible;
                        if (currentStatus != Status.Hide)
                            btn_disconnect.Visibility = System.Windows.Visibility.Visible;
                    }
                    else if (value == Status.Hide)
                    {
                        if (currentStatus == Status.ShowStart)
                            btn_connect.Visibility = System.Windows.Visibility.Visible;
                        else if (currentStatus == Status.ShowStop)
                            btn_disconnect.Visibility = System.Windows.Visibility.Visible;
                        btn_exit.Visibility = System.Windows.Visibility.Visible;
                    }

                    if (value == Status.Hide)
                        cc3d_Exit.BringFrontSideIntoView();
                    else if (currentStatus == Status.Hide)
                        cc3d_Exit.BringBackSideIntoView();

                    if (value == Status.ShowStart || (value == Status.Hide && currentStatus == Status.ShowStop))
                        cc3d_Stop_Start.BringBackSideIntoView();
                    else if (value == Status.ShowStop || (value == Status.Hide && currentStatus == Status.ShowStart))
                        cc3d_Stop_Start.BringFrontSideIntoView();

                    currentStatus = value;
                }
                new System.Threading.Thread(delegate()
                {
                    //int timeOut = 0;
                    while (inAnimation) // && timeOut < 20)
                    {
                        this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                        {
                            if (cc3d_Stop_Start.IsRotating || cc3d_Exit.IsRotating)
                                return;

                            if (currentStatus == Status.ShowStop)
                            {
                                btn_disconnect.IsEnabled = true;
                                btn_exit.IsEnabled = true;
                            }
                            else if (currentStatus == Status.ShowStart)
                            {
                                btn_connect.IsEnabled = true;
                                btn_exit.IsEnabled = true;
                            }
                            inAnimation = false;
                        }, new object[] { });
                        System.Threading.Thread.Sleep((int)(100));
                        //timeOut++;
                    }
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        if (currentStatus != desiredStatus)
                            CurrentStatus = desiredStatus;
                    });
                }) { IsBackground = true }.Start();
            }
        }
        public StartStopButton()
        {
            InitializeComponent();
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(StartStopButton.MinimizeClickEvent));
        }

        private void btn_dissconnect_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(StartStopButton.StopClickEvent));
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(StartStopButton.StartClickEvent));
        }
    }
}
