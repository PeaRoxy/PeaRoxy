// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolbarButtons.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for ToolbarButtons.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Shapes;

    #endregion

    /// <summary>
    ///     Interaction logic for ToolbarButtons.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
        Justification = "Reviewed. Suppression is OK here.")]
    public partial class ToolbarButtons
    {
        #region Static Fields

        /// <summary>
        ///     The back click event.
        /// </summary>
        public static readonly RoutedEvent BackClickEvent = EventManager.RegisterRoutedEvent(
            "BackClick", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(ToolbarButtons));

        /// <summary>
        ///     The grabber selected changed event.
        /// </summary>
        public static readonly RoutedEvent GrabberSelectedChangedEvent =
            EventManager.RegisterRoutedEvent(
                "GrabberSelectedChanged", 
                RoutingStrategy.Bubble, 
                typeof(RoutedEventHandler), 
                typeof(ToolbarButtons));

        /// <summary>
        ///     The option click event.
        /// </summary>
        public static readonly RoutedEvent OptionClickEvent = EventManager.RegisterRoutedEvent(
            "OptionClick", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(ToolbarButtons));

        /// <summary>
        ///     The re config click event.
        /// </summary>
        public static readonly RoutedEvent ReConfigClickEvent = EventManager.RegisterRoutedEvent(
            "ReConfigClick", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(ToolbarButtons));

        /// <summary>
        ///     The smart pear selected changed event.
        /// </summary>
        public static readonly RoutedEvent SmartPearSelectedChangedEvent =
            EventManager.RegisterRoutedEvent(
                "SmartPearSelectedChanged", 
                RoutingStrategy.Bubble, 
                typeof(RoutedEventHandler), 
                typeof(ToolbarButtons));

        /// <summary>
        ///     The smart pear update click event.
        /// </summary>
        public static readonly RoutedEvent SmartPearUpdateClickEvent =
            EventManager.RegisterRoutedEvent(
                "SmartPearUpdateClick", 
                RoutingStrategy.Bubble, 
                typeof(RoutedEventHandler), 
                typeof(ToolbarButtons));

        #endregion

        #region Fields

        /// <summary>
        ///     The navigator state.
        /// </summary>
        private State navigatorState = State.Option;
        private DoubleAnimation optionRotateButtonAnimation;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToolbarButtons" /> class.
        /// </summary>
        public ToolbarButtons()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The back click.
        /// </summary>
        public event RoutedEventHandler BackClick
        {
            add
            {
                this.AddHandler(BackClickEvent, value);
            }

            remove
            {
                this.RemoveHandler(BackClickEvent, value);
            }
        }

        /// <summary>
        ///     The grabber selected changed.
        /// </summary>
        public event RoutedEventHandler GrabberSelectedChanged
        {
            add
            {
                this.AddHandler(GrabberSelectedChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(GrabberSelectedChangedEvent, value);
            }
        }

        /// <summary>
        ///     The option click.
        /// </summary>
        public event RoutedEventHandler OptionClick
        {
            add
            {
                this.AddHandler(OptionClickEvent, value);
            }

            remove
            {
                this.RemoveHandler(OptionClickEvent, value);
            }
        }

        /// <summary>
        ///     The re config click.
        /// </summary>
        public event RoutedEventHandler ReConfigClick
        {
            add
            {
                this.AddHandler(ReConfigClickEvent, value);
            }

            remove
            {
                this.RemoveHandler(ReConfigClickEvent, value);
            }
        }

        /// <summary>
        ///     The smart pear selected changed.
        /// </summary>
        public event RoutedEventHandler SmartPearSelectedChanged
        {
            add
            {
                this.AddHandler(SmartPearSelectedChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(SmartPearSelectedChangedEvent, value);
            }
        }

        /// <summary>
        ///     The smart pear update click.
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        public event RoutedEventHandler SmartPearUpdateClick
        {
            add
            {
                this.AddHandler(SmartPearUpdateClickEvent, value);
            }

            remove
            {
                this.RemoveHandler(SmartPearUpdateClickEvent, value);
            }
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The color.
        /// </summary>
        public enum Color
        {
            /// <summary>
            ///     The white.
            /// </summary>
            White, 

            /// <summary>
            ///     The red.
            /// </summary>
            Red, 

            /// <summary>
            ///     The blue.
            /// </summary>
            Blue, 

            /// <summary>
            ///     The yellow.
            /// </summary>
            Yellow
        }

        /// <summary>
        ///     The state.
        /// </summary>
        public enum State
        {
            /// <summary>
            ///     The back.
            /// </summary>
            Back, 

            /// <summary>
            ///     The option.
            /// </summary>
            Option
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the grabber color.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public Color GrabberColor
        {
            get
            {
                return this.GetEllipseColor(this.EQbGrabber);
            }

            set
            {
                SetEllipseColor(this.EQbGrabber, value);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether grabber is enable.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool GrabberIsEnable
        {
            get
            {
                return this.GrabberGrid.IsEnabled;
            }

            set
            {
                this.GrabberGrid.IsEnabled = value;
            }
        }

        /// <summary>
        ///     Gets or sets the navigator state.
        /// </summary>
        public State NavigatorState
        {
            get
            {
                return this.navigatorState;
            }

            set
            {
                const double Time = 1;
                DoubleAnimation alignTopButtonsAnimation;
                TranslateTransform alignTopButtonsTransform;
                RotateTransform rotateTransform;
                if (this.navigatorState == value)
                {
                    return;
                }

                this.navigatorState = value;
                switch (value)
                {
                    case State.Back:
                        alignTopButtonsAnimation = new DoubleAnimation(0, 50, new Duration(TimeSpan.FromSeconds(Time)));
                        alignTopButtonsTransform = new TranslateTransform();
                        alignTopButtonsAnimation.EasingFunction = new ElasticEase();
                        ((ElasticEase)alignTopButtonsAnimation.EasingFunction).EasingMode = EasingMode.EaseOut;
                        ((ElasticEase)alignTopButtonsAnimation.EasingFunction).Oscillations = 1;
                        ((ElasticEase)alignTopButtonsAnimation.EasingFunction).Springiness = 6;
                        this.NavigatorGrid.RenderTransform = alignTopButtonsTransform;
                        alignTopButtonsTransform.BeginAnimation(TranslateTransform.XProperty, alignTopButtonsAnimation);
                        DoubleAnimation hideOptionsButAnimation = new DoubleAnimation(
                            1, 
                            0, 
                            new Duration(TimeSpan.FromSeconds(Time)));
                        DoubleAnimation showBackButAnimation = new DoubleAnimation(
                            0, 
                            1, 
                            new Duration(TimeSpan.FromSeconds(Time)));
                        hideOptionsButAnimation.RepeatBehavior = new RepeatBehavior(1);
                        showBackButAnimation.RepeatBehavior = new RepeatBehavior(1);
                        this.BackButton.Visibility = Visibility.Visible;
                        this.OptionButton.BeginAnimation(OpacityProperty, hideOptionsButAnimation);
                        this.BackButton.BeginAnimation(OpacityProperty, showBackButAnimation);
                        this.optionRotateButtonAnimation = this.optionRotateButtonAnimation ?? new DoubleAnimation();

                        this.optionRotateButtonAnimation.From = 90;
                        this.optionRotateButtonAnimation.To = 0;
                        this.optionRotateButtonAnimation.RepeatBehavior = new RepeatBehavior(1);
                        this.optionRotateButtonAnimation.Duration = new Duration(TimeSpan.FromSeconds(Time));
                        rotateTransform = this.ImgBackButton.RenderTransform as RotateTransform;
                        if (rotateTransform != null)
                        {
                            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, this.optionRotateButtonAnimation);
                        }

                        break;
                    default:
                        DoubleAnimation showOptionsButAnimation = new DoubleAnimation(
                            0, 
                            1, 
                            new Duration(TimeSpan.FromSeconds(Time)));
                        DoubleAnimation hideBackButAnimation = new DoubleAnimation(
                            1, 
                            0, 
                            new Duration(TimeSpan.FromSeconds(Time)));
                        showOptionsButAnimation.RepeatBehavior = new RepeatBehavior(1);
                        hideBackButAnimation.RepeatBehavior = new RepeatBehavior(1);
                        this.OptionButton.Visibility = Visibility.Visible;
                        this.OptionButton.BeginAnimation(OpacityProperty, showOptionsButAnimation);
                        this.BackButton.BeginAnimation(OpacityProperty, hideBackButAnimation);
                        
                        this.optionRotateButtonAnimation = this.optionRotateButtonAnimation ?? new DoubleAnimation();

                        this.optionRotateButtonAnimation.From = 0;
                        this.optionRotateButtonAnimation.To = 90;
                        this.optionRotateButtonAnimation.RepeatBehavior = new RepeatBehavior(1);
                        this.optionRotateButtonAnimation.Duration = new Duration(TimeSpan.FromSeconds(Time));
                        rotateTransform = this.ImgBackButton.RenderTransform as RotateTransform;
                        if (rotateTransform != null)
                        {
                            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, this.optionRotateButtonAnimation);
                        }

                        alignTopButtonsAnimation = new DoubleAnimation(
                            50, 
                            0, 
                            new Duration(TimeSpan.FromSeconds(Time * 0.5)));
                        alignTopButtonsTransform = new TranslateTransform();
                        alignTopButtonsAnimation.EasingFunction = new PowerEase();
                        ((PowerEase)alignTopButtonsAnimation.EasingFunction).EasingMode = EasingMode.EaseOut;
                        this.NavigatorGrid.RenderTransform = alignTopButtonsTransform;
                        alignTopButtonsTransform.BeginAnimation(TranslateTransform.XProperty, alignTopButtonsAnimation);
                        break;
                }

                new Thread(
                    delegate()
                        {
                            Thread.Sleep((int)(Time * 1000));
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)delegate
                                    {
                                        if (this.navigatorState == State.Back)
                                        {
                                            this.OptionButton.Visibility = Visibility.Hidden;
                                        }
                                        else
                                        {
                                            this.BackButton.Visibility = Visibility.Hidden;
                                        }
                                    }, 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
        }

        /// <summary>
        ///     Gets or sets the re config color.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public Color ReConfigColor
        {
            get
            {
                return this.GetEllipseColor(this.EQbReconfig);
            }

            set
            {
                SetEllipseColor(this.EQbReconfig, value);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether re config is enable.
        /// </summary>
        public bool ReConfigIsEnable
        {
            // ReSharper disable once UnusedMember.Global
            get
            {
                return this.ReConfigGrid.IsEnabled;
            }

            set
            {
                this.ReConfigGrid.IsEnabled = value;
            }
        }

        /// <summary>
        ///     Gets or sets the smart pear color.
        /// </summary>
        public Color SmartPearColor
        {
            // ReSharper disable once UnusedMember.Global
            get
            {
                return this.GetEllipseColor(this.EQbSmartpear);
            }

            set
            {
                SetEllipseColor(this.EQbSmartpear, value);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether smart pear is enable.
        /// </summary>
        public bool SmartPearIsEnable
        {
            get
            {
                return this.SmartPearGrid.IsEnabled;
            }

            set
            {
                this.SmartPearGrid.IsEnabled = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The set ellipse color.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <param name="color">
        /// The color.
        /// </param>
        private static void SetEllipseColor(Ellipse e, Color color)
        {
            if (e.Stroke.IsFrozen)
            {
                e.Stroke = e.Stroke.CloneCurrentValue();
            }

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
                default:
                    c = Colors.White;
                    break;
            }

            ColorAnimation changesmAnimation = new ColorAnimation(c, new Duration(TimeSpan.FromSeconds(0.4)));
            e.Stroke.BeginAnimation(SolidColorBrush.ColorProperty, changesmAnimation);
        }

        /// <summary>
        /// The btn_back_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnBackClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(BackClickEvent));
        }

        /// <summary>
        /// The btn_options_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnOptionsClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(OptionClickEvent));
        }

        /// <summary>
        /// The btn_qb_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnQbClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.PlacementTarget = (Button)sender;
            ((Button)sender).ContextMenu.Placement = PlacementMode.Bottom;
            ((Button)sender).ContextMenu.IsOpen = true;
        }

        /// <summary>
        /// The btn_qb_reconfig_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnQbReconfigClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(ReConfigClickEvent));
        }

        /// <summary>
        /// The get ellipse color.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <returns>
        /// The <see cref="Color"/>.
        /// </returns>
        private Color GetEllipseColor(Ellipse e)
        {
            System.Windows.Media.Color c = ((SolidColorBrush)e.Stroke).Color;
            if (c == System.Windows.Media.Color.FromRgb(180, 0, 0))
            {
                return Color.Red;
            }

            if (c == System.Windows.Media.Color.FromRgb(0, 160, 220))
            {
                return Color.Blue;
            }

            if (c == System.Windows.Media.Color.FromRgb(230, 220, 60))
            {
                return Color.Yellow;
            }

            return Color.White;
        }

        /// <summary>
        /// The img options button_ mouse enter.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ImgOptionsButtonMouseEnter(object sender, MouseEventArgs e)
        {
            this.optionRotateButtonAnimation = this.optionRotateButtonAnimation ?? new DoubleAnimation();

            this.optionRotateButtonAnimation.From = 0;
            this.optionRotateButtonAnimation.To = 360;
            this.optionRotateButtonAnimation.Duration = new Duration(TimeSpan.FromSeconds(2));
            this.optionRotateButtonAnimation.RepeatBehavior = RepeatBehavior.Forever;
            RotateTransform rotateTransform = this.ImgOptionsButton.RenderTransform as RotateTransform;
            if (rotateTransform != null)
            {
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, this.optionRotateButtonAnimation);
            }

        }

        /// <summary>
        /// The img options button_ mouse leave.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ImgOptionsButtonMouseLeave(object sender, MouseEventArgs e)
        {
            if (this.optionRotateButtonAnimation == null)
            {
                return;
            }

            this.optionRotateButtonAnimation.From = 0;
            this.optionRotateButtonAnimation.To = 360;
            this.optionRotateButtonAnimation.RepeatBehavior = new RepeatBehavior(1);
            this.optionRotateButtonAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            RotateTransform rotateTransform = this.ImgOptionsButton.RenderTransform as RotateTransform;
            if (rotateTransform != null)
            {
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, this.optionRotateButtonAnimation);
            }
        }

        /// <summary>
        /// The mi_grabber_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MiGrabberClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new MenuItemEventArgs(GrabberSelectedChangedEvent, sender as MenuItem));
        }

        /// <summary>
        /// The mi_smartpear_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MiSmartpearClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new MenuItemEventArgs(SmartPearSelectedChangedEvent, sender as MenuItem));
        }

        /// <summary>
        /// The mi_smartpear_update_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MiSmartpearUpdateClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(SmartPearUpdateClickEvent));
        }

        /// <summary>
        /// The user control_ loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", 
            Justification = "Reviewed. Suppression is OK here.")]
        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            this.ReConfigGrid.ToolTip = new Tooltip(
                "Reconfigure Windows/Applications", 
                "Cleanup and configure Windows and applications to use selected grabber to access internet.");

            this.GrabberGrid.ToolTip = new Tooltip(
                "Traffic Grabber", 
                "Select how you want us to grab traffic from applications:" + Environment.NewLine + string.Empty
                + "[None:] No grabbing, applications and Windows are free to use PeaRoxy but you need to manually configure them"
                + Environment.NewLine
                + "[TAP Adapter:] Force Windows and applications to use PeaRoxy using a Virtual Network Adapter"
                + Environment.NewLine
                + "[Hook:] Force applications to use PeaRoxy by injecting a custom code into them"
                + Environment.NewLine
                + "[Proxy:] Register PeaRoxy as active proxy in Windows settings");

            this.SmartPearGrid.ToolTip = new Tooltip(
                "SmartPear!", 
                "[Enable:] Check direct connection before using Server for forwarding traffic" + Environment.NewLine
                + "[Disable:] Always send traffic through Server" + Environment.NewLine
                + "[Update:] Download latest settings for your selected country/profile and reset all PeaRoxt configurations to defualt");

            ToolTipService.SetShowDuration(this.ReConfigGrid, 60000);
            ToolTipService.SetShowDuration(this.GrabberGrid, 60000);
            ToolTipService.SetShowDuration(this.SmartPearGrid, 60000);
        }

        #endregion

        /// <summary>
        ///     The menu item event args.
        /// </summary>
        public class MenuItemEventArgs : RoutedEventArgs
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="MenuItemEventArgs"/> class.
            /// </summary>
            /// <param name="routedEvent">
            /// The routed event.
            /// </param>
            /// <param name="sender">
            /// The sender.
            /// </param>
            public MenuItemEventArgs(RoutedEvent routedEvent, MenuItem sender)
                : base(routedEvent)
            {
                this.SenderMenuItem = sender;
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets the sender menu item.
            /// </summary>
            public MenuItem SenderMenuItem { get; private set; }

            #endregion
        }
    }
}