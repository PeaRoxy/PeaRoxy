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
    /// Interaction logic for ToolbarButtons.xaml
    /// </summary>
    public partial class ToolbarButtons : UserControl
    {
        public new MainWindow Parent;
        public enum State
        {
            Back,
            Option
        }
        public enum Color
        {
            White,
            Red,
            Blue,
            Yellow
        }
        public class MenuItemEventArgs : RoutedEventArgs
        {
            public MenuItem SenderMenuItem { get; private set; }
            public MenuItemEventArgs(RoutedEvent routedEvent, MenuItem sender)
                : base(routedEvent) { SenderMenuItem = sender; }
        }
        public static readonly RoutedEvent BackClickEvent = EventManager.RegisterRoutedEvent("BackClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolbarButtons));
        public event RoutedEventHandler BackClick
        {
            add { AddHandler(BackClickEvent, value); }
            remove { RemoveHandler(BackClickEvent, value); }
        }
        public static readonly RoutedEvent OptionClickEvent = EventManager.RegisterRoutedEvent("OptionClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolbarButtons));
        public event RoutedEventHandler OptionClick
        {
            add { AddHandler(OptionClickEvent, value); }
            remove { RemoveHandler(OptionClickEvent, value); }
        }
        public static readonly RoutedEvent GrabberSelectedChangedEvent = EventManager.RegisterRoutedEvent("GrabberSelectedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolbarButtons));
        public event RoutedEventHandler GrabberSelectedChanged
        {
            add { AddHandler(GrabberSelectedChangedEvent, value); }
            remove { RemoveHandler(GrabberSelectedChangedEvent, value); }
        }
        public static readonly RoutedEvent SmartPearSelectedChangedEvent = EventManager.RegisterRoutedEvent("SmartPearSelectedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolbarButtons));
        public event RoutedEventHandler SmartPearSelectedChanged
        {
            add { AddHandler(SmartPearSelectedChangedEvent, value); }
            remove { RemoveHandler(SmartPearSelectedChangedEvent, value); }
        }
        public static readonly RoutedEvent SmartPearUpdateClickEvent = EventManager.RegisterRoutedEvent("SmartPearUpdateClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolbarButtons));
        public event RoutedEventHandler SmartPearUpdateClick
        {
            add { AddHandler(SmartPearUpdateClickEvent, value); }
            remove { RemoveHandler(SmartPearUpdateClickEvent, value); }
        }
        public static readonly RoutedEvent ReConfigClickEvent = EventManager.RegisterRoutedEvent("ReConfigClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolbarButtons));
        public event RoutedEventHandler ReConfigClick
        {
            add { AddHandler(ReConfigClickEvent, value); }
            remove { RemoveHandler(ReConfigClickEvent, value); }
        }

        private void btn_options_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ToolbarButtons.OptionClickEvent));
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ToolbarButtons.BackClickEvent));
        }

        private void btn_qb_reconfig_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ToolbarButtons.ReConfigClickEvent));
        }

        private void mi_smartpear_update_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ToolbarButtons.SmartPearUpdateClickEvent));
        }

        private void mi_smartpear_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new MenuItemEventArgs(ToolbarButtons.SmartPearSelectedChangedEvent, sender as MenuItem));
        }

        private void mi_grabber_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new MenuItemEventArgs(ToolbarButtons.GrabberSelectedChangedEvent, sender as MenuItem));
        }

        public ToolbarButtons()
        {
            InitializeComponent();
        }

        private void btn_qb_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.PlacementTarget = (Button)sender;
            ((Button)sender).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            ((Button)sender).ContextMenu.IsOpen = true;
        }

        private void img_optionsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation da;
            if (img_optionsButton.Tag == null)
                da = new DoubleAnimation(0, 360, Duration.Automatic);
            else
                da = img_optionsButton.Tag as DoubleAnimation;

            da.Duration = new Duration(TimeSpan.FromSeconds(2));
            da.RepeatBehavior = RepeatBehavior.Forever;
            RotateTransform rt = img_optionsButton.RenderTransform as RotateTransform;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
            img_optionsButton.Tag = da;
        }

        private void img_optionsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (img_optionsButton.Tag == null)
                return;

            DoubleAnimation da = img_optionsButton.Tag as DoubleAnimation;
            da.RepeatBehavior = new RepeatBehavior(1);
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            RotateTransform rt = img_optionsButton.RenderTransform as RotateTransform;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Parent = (MainWindow)Window.GetWindow(this);
            ReConfigGrid.ToolTip = new UserControls.Tooltip("Reconfigure Windows/Applications",
                "Configure Windows and Firefox to use PeaRoxy Client as proxy server."
                );

            GrabberGrid.ToolTip = new UserControls.Tooltip("Traffic Grabber",
                "Select how you want us to grab traffic from applications:" + Environment.NewLine +
                "" +
                "[None:] No grabbing, applications and Windows use PeaRoxy as proxy if they want" + Environment.NewLine +
                "[TAP Adapter:] Force Windows and applications to use PeaRoxy using a Virtual Network Adapter" + Environment.NewLine +
                "[Hook:] Force applications to use PeaRoxy by injecting a code into them. Modifying them on the fly in other word"
                );

            SmartPearGrid.ToolTip = new UserControls.Tooltip("SmartPear!",
                "[Enable:] Check direct connection before using Server for forwarding traffic" + Environment.NewLine +
                "[Disable:] Always send traffic through Server" + Environment.NewLine +
                "[Update:] Download latest rulesets for improving SmartPear behavior"
                );

            ToolTipService.SetShowDuration(ReConfigGrid, 60000);
            ToolTipService.SetShowDuration(GrabberGrid, 60000);
            ToolTipService.SetShowDuration(SmartPearGrid, 60000);
        }

        

        private void SetEllipseColor(Ellipse e, Color color)
        {
            if (e.Stroke.IsFrozen)
                e.Stroke = e.Stroke.CloneCurrentValue();
            System.Windows.Media.Color c;
            switch (color)
            {
                case Color.Red:
                    c = System.Windows.Media.Color.FromRgb(180, 0, 0);
                    break;
                case Color.Blue:
                    c = System.Windows.Media.Color.FromRgb(0, 160, 220);
                    break;
                case Color.Yellow:
                    c = System.Windows.Media.Color.FromRgb(230, 220, 60);
                    break;
                case Color.White:
                default:
                    c = System.Windows.Media.Colors.White;
                    break;
            }
            ColorAnimation ca_changesm = new ColorAnimation(c, new Duration(TimeSpan.FromSeconds(0.4)));
            ((SolidColorBrush)e.Stroke).BeginAnimation(SolidColorBrush.ColorProperty, ca_changesm);
        }

        private Color GetEllipseColor(Ellipse e)
        {
            System.Windows.Media.Color c = ((SolidColorBrush)e.Stroke).Color;
            if (c == System.Windows.Media.Color.FromRgb(180, 0, 0))
                return Color.Red;
            else if (c == System.Windows.Media.Color.FromRgb(0, 160, 220))
                return Color.Blue;
            else if (c == System.Windows.Media.Color.FromRgb(230, 220, 60))
                return Color.Yellow;
            return Color.White;
        }

        public Boolean GrabberIsEnable
        {
            get { return GrabberGrid.IsEnabled; }
            set { GrabberGrid.IsEnabled = value; }
        }
        public Boolean SmartPearIsEnable
        {
            get { return SmartPearGrid.IsEnabled; }
            set { SmartPearGrid.IsEnabled = value; }
        }
        public Boolean ReConfigIsEnable
        {
            get { return ReConfigGrid.IsEnabled; }
            set { ReConfigGrid.IsEnabled = value; }
        }

        public Color GrabberColor
        {
            get { return GetEllipseColor(e_qb_grabber); }
            set { SetEllipseColor(e_qb_grabber, value); }
        }
        public Color SmartPearColor
        {
            get { return GetEllipseColor(e_qb_smartpear); }
            set { SetEllipseColor(e_qb_smartpear, value); }
        }
        public Color ReConfigColor
        {
            get { return GetEllipseColor(e_qb_reconfig); }
            set { SetEllipseColor(e_qb_reconfig, value); }
        }

        State navigatorState = State.Option;
        public State NavigatorState
        {
            get
            {
                return navigatorState;
            }
            set
            {
                double time = 1;
                DoubleAnimation da_alignTopButtons;
                TranslateTransform tt_alignTopButtons;
                RotateTransform rt;
                DoubleAnimation da_RotateBackImage;
                if (navigatorState == value)
                    return;
                else
                    navigatorState = value;
                switch (value)
                {
                    case State.Back:
                        da_alignTopButtons = new DoubleAnimation(0, 50, new Duration(TimeSpan.FromSeconds(time)));
                        tt_alignTopButtons = new TranslateTransform();
                        da_alignTopButtons.EasingFunction = new ElasticEase();
                        ((ElasticEase)da_alignTopButtons.EasingFunction).EasingMode = EasingMode.EaseOut;
                        ((ElasticEase)da_alignTopButtons.EasingFunction).Oscillations = 1;
                        ((ElasticEase)da_alignTopButtons.EasingFunction).Springiness = 6;
                        NavigatorGrid.RenderTransform = tt_alignTopButtons;
                        tt_alignTopButtons.BeginAnimation(TranslateTransform.XProperty, da_alignTopButtons);
                        DoubleAnimation da_HideOptionsBut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(time)));
                        DoubleAnimation da_ShowBackBut = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(time)));
                        da_HideOptionsBut.RepeatBehavior = new RepeatBehavior(1);
                        da_ShowBackBut.RepeatBehavior = new RepeatBehavior(1);
                        BackButton.Visibility = System.Windows.Visibility.Visible;
                        OptionButton.BeginAnimation(Button.OpacityProperty, da_HideOptionsBut);
                        BackButton.BeginAnimation(Button.OpacityProperty, da_ShowBackBut);

                        if (img_backButton.Tag == null)
                            da_RotateBackImage = new DoubleAnimation(90, 0, Duration.Automatic);
                        else
                            da_RotateBackImage = img_optionsButton.Tag as DoubleAnimation;
                        da_RotateBackImage.RepeatBehavior = new RepeatBehavior(1);
                        da_RotateBackImage.Duration = new Duration(TimeSpan.FromSeconds(time));
                        rt = img_backButton.RenderTransform as RotateTransform;
                        rt.BeginAnimation(RotateTransform.AngleProperty, da_RotateBackImage);
                        break;
                    case State.Option:
                    default:
                        DoubleAnimation da_ShowOptionsBut = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(time)));
                        DoubleAnimation da_HideBackBut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(time)));
                        da_ShowOptionsBut.RepeatBehavior = new RepeatBehavior(1);
                        da_HideBackBut.RepeatBehavior = new RepeatBehavior(1);
                        OptionButton.Visibility = System.Windows.Visibility.Visible;
                        OptionButton.BeginAnimation(Button.OpacityProperty, da_ShowOptionsBut);
                        BackButton.BeginAnimation(Button.OpacityProperty, da_HideBackBut);

                        if (img_backButton.Tag == null)
                            da_RotateBackImage = new DoubleAnimation(0, 90, Duration.Automatic);
                        else
                            da_RotateBackImage = img_optionsButton.Tag as DoubleAnimation;
                        da_RotateBackImage.RepeatBehavior = new RepeatBehavior(1);
                        da_RotateBackImage.Duration = new Duration(TimeSpan.FromSeconds(time));
                        rt = img_backButton.RenderTransform as RotateTransform;
                        rt.BeginAnimation(RotateTransform.AngleProperty, da_RotateBackImage);


                        da_alignTopButtons = new DoubleAnimation(50, 0, new Duration(TimeSpan.FromSeconds(time * 0.5)));
                        tt_alignTopButtons = new TranslateTransform();
                        da_alignTopButtons.EasingFunction = new PowerEase();
                        ((PowerEase)da_alignTopButtons.EasingFunction).EasingMode = EasingMode.EaseOut;
                        NavigatorGrid.RenderTransform = tt_alignTopButtons;
                        tt_alignTopButtons.BeginAnimation(TranslateTransform.XProperty, da_alignTopButtons);
                        break;
                }
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(time * 1000));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        if (navigatorState == State.Back)
                            OptionButton.Visibility = System.Windows.Visibility.Hidden;
                        else
                            BackButton.Visibility = System.Windows.Visibility.Hidden;
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
        }
    }
}
