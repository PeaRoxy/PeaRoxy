// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartStopButton.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for StartStop.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    #region

    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Windows;

    #endregion

    /// <summary>
    ///     Interaction logic for StartStop.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class StartStopButton
    {
        #region Static Fields

        /// <summary>
        /// The minimize click event.
        /// </summary>
        public static readonly RoutedEvent MinimizeClickEvent = EventManager.RegisterRoutedEvent(
            "MinimizeClick", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(StartStopButton));

        /// <summary>
        /// The start click event.
        /// </summary>
        public static readonly RoutedEvent StartClickEvent = EventManager.RegisterRoutedEvent(
            "StartClick", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(StartStopButton));

        /// <summary>
        /// The stop click event.
        /// </summary>
        public static readonly RoutedEvent StopClickEvent = EventManager.RegisterRoutedEvent(
            "StopClick", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(StartStopButton));

        #endregion

        #region Fields

        /// <summary>
        /// The current status.
        /// </summary>
        private Status currentStatus = Status.Hide;

        /// <summary>
        /// The desired status.
        /// </summary>
        private Status desiredStatus = Status.Hide;

        /// <summary>
        /// The in animation.
        /// </summary>
        private bool inAnimation;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StartStopButton"/> class.
        /// </summary>
        public StartStopButton()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The minimize click.
        /// </summary>
        public event RoutedEventHandler MinimizeClick
        {
            add
            {
                this.AddHandler(MinimizeClickEvent, value);
            }

            remove
            {
                this.RemoveHandler(MinimizeClickEvent, value);
            }
        }

        /// <summary>
        /// The start click.
        /// </summary>
        public event RoutedEventHandler StartClick
        {
            add
            {
                this.AddHandler(StartClickEvent, value);
            }

            remove
            {
                this.RemoveHandler(StartClickEvent, value);
            }
        }

        /// <summary>
        /// The stop click.
        /// </summary>
        public event RoutedEventHandler StopClick
        {
            add
            {
                this.AddHandler(StopClickEvent, value);
            }

            remove
            {
                this.RemoveHandler(StopClickEvent, value);
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The status.
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// The hide.
            /// </summary>
            Hide, 

            /// <summary>
            /// The show start.
            /// </summary>
            ShowStart, 

            /// <summary>
            /// The show stop.
            /// </summary>
            ShowStop
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Sets the current status.
        /// </summary>
        public Status CurrentStatus
        {
            set
            {
                this.desiredStatus = value;
                if (this.inAnimation)
                {
                    return;
                }

                if (this.currentStatus != value)
                {
                    this.inAnimation = true;
                    this.BtnDisconnect.IsEnabled = false;
                    this.BtnConnect.IsEnabled = false;
                    this.BtnExit.IsEnabled = false;
                    this.BtnDisconnect.Visibility = Visibility.Hidden;
                    this.BtnConnect.Visibility = Visibility.Hidden;
                    this.BtnExit.Visibility = Visibility.Hidden;
                    if (value == Status.ShowStop)
                    {
                        this.BtnDisconnect.Visibility = Visibility.Visible;
                        this.BtnExit.Visibility = Visibility.Visible;
                        if (this.currentStatus != Status.Hide)
                        {
                            this.BtnConnect.Visibility = Visibility.Visible;
                        }
                    }
                    else if (value == Status.ShowStart)
                    {
                        this.BtnConnect.Visibility = Visibility.Visible;
                        this.BtnExit.Visibility = Visibility.Visible;
                        if (this.currentStatus != Status.Hide)
                        {
                            this.BtnDisconnect.Visibility = Visibility.Visible;
                        }
                    }
                    else if (value == Status.Hide)
                    {
                        if (this.currentStatus == Status.ShowStart)
                        {
                            this.BtnConnect.Visibility = Visibility.Visible;
                        }
                        else if (this.currentStatus == Status.ShowStop)
                        {
                            this.BtnDisconnect.Visibility = Visibility.Visible;
                        }

                        this.BtnExit.Visibility = Visibility.Visible;
                    }

                    if (value == Status.Hide)
                    {
                        this.BtnExit.Visibility = Visibility.Hidden;
                    }
                    else if (this.currentStatus == Status.Hide)
                    {
                        this.BtnExit.Visibility = Visibility.Visible;
                    }

                    if (value == Status.ShowStart || (value == Status.Hide && this.currentStatus == Status.ShowStop))
                    {
                        this.BtnDisconnect.Visibility = Visibility.Hidden;
                    }
                    else if (value == Status.ShowStop || (value == Status.Hide && this.currentStatus == Status.ShowStart))
                    {
                        this.BtnConnect.Visibility = Visibility.Hidden;
                    }

                    this.currentStatus = value;
                }

                new Thread(
                    delegate()
                        {
                            while (this.inAnimation)
                            {
                                this.Dispatcher.Invoke(
                                    (App.SimpleVoidDelegate)delegate
                                        {
                                            if (this.currentStatus == Status.ShowStop)
                                            {
                                                this.BtnDisconnect.IsEnabled = true;
                                                this.BtnExit.IsEnabled = true;
                                            }
                                            else if (this.currentStatus == Status.ShowStart)
                                            {
                                                this.BtnConnect.IsEnabled = true;
                                                this.BtnExit.IsEnabled = true;
                                            }

                                            this.inAnimation = false;
                                        }, 
                                    new object[] { });
                                Thread.Sleep(100);
                            }

                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)delegate
                                    {
                                        if (this.currentStatus != this.desiredStatus)
                                        {
                                            this.CurrentStatus = this.desiredStatus;
                                        }
                                    });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The btn_connect_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnConnectClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(StartClickEvent));
        }

        /// <summary>
        /// The btn_dissconnect_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnDissconnectClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(StopClickEvent));
        }

        /// <summary>
        /// The btn_exit_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnExitClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(MinimizeClickEvent));
        }

        #endregion
    }
}