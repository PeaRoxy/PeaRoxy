// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeadPanel.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for HeadPanel.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.Panels
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Animation;

    #endregion

    /// <summary>
    ///     Interaction logic for HeadPanel.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class HeadPanel
    {
        #region Static Fields

        /// <summary>
        /// The minimize click event.
        /// </summary>
        public static readonly RoutedEvent MinimizeClickEvent = EventManager.RegisterRoutedEvent(
            "MinimizeClick", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(HeadPanel));

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HeadPanel"/> class.
        /// </summary>
        public HeadPanel()
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

        #endregion

        #region Methods

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

        /// <summary>
        /// The img_close button_ mouse enter.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ImgCloseButtonMouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation(0.5, 1, new Duration(TimeSpan.FromSeconds(0.3)));
            this.btn_close.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        /// <summary>
        /// The img_close button_ mouse leave.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ImgCloseButtonMouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation(1, 0.5, new Duration(TimeSpan.FromSeconds(0.7)));
            this.btn_close.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        #endregion
    }
}