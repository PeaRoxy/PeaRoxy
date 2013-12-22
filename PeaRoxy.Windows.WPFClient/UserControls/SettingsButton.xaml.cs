// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsButton.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for SettingsButton.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    using PeaRoxy.Windows.WPFClient.SettingTabs;

    #endregion

    /// <summary>
    ///     Interaction logic for SettingsButton.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class SettingsButton
    {
        #region Fields

        /// <summary>
        /// The is selected.
        /// </summary>
        private bool isSelected;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsButton"/> class.
        /// </summary>
        public SettingsButton()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The selected changed.
        /// </summary>
        public event RoutedEventHandler SelectedChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        public ImageSource Image
        {
            // ReSharper disable once UnusedMember.Global
            get
            {
                return this.Img.Source;
            }

            set
            {
                this.Img.Source = value;
            }
        }

        /// <summary>
        /// Gets or sets the settings page.
        /// </summary>
        public Base SettingsPage { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text
        {
            // ReSharper disable once UnusedMember.Global
            get
            {
                return (string)this.Label.Content;
            }

            set
            {
                this.Label.Content = value;
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text.
        /// </summary>
        public string TooltipText
        {
            // ReSharper disable once UnusedMember.Global
            get
            {
                return ((Tooltip)this.Button.ToolTip).Text;
            }

            set
            {
                ((Tooltip)this.Button.ToolTip).Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the tooltip title.
        /// </summary>
        public string TooltipTitle
        {
            // ReSharper disable once UnusedMember.Global
            get
            {
                return ((Tooltip)this.Button.ToolTip).Title;
            }

            set
            {
                ((Tooltip)this.Button.ToolTip).Title = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    if (this.isSelected)
                    {
                        DoubleAnimation showAnimation = new DoubleAnimation(5, 20, new Duration(TimeSpan.FromSeconds(0.3)));
                        TranslateTransform tt2 = new TranslateTransform();
                        this.Button.RenderTransform = tt2;
                        showAnimation.DecelerationRatio = 0.8;
                        tt2.BeginAnimation(TranslateTransform.XProperty, showAnimation);
                    }
                    else
                    {
                        DoubleAnimation hideAnimation = new DoubleAnimation(20, 0, new Duration(TimeSpan.FromSeconds(0.2)));
                        TranslateTransform tt = new TranslateTransform();
                        this.Button.RenderTransform = tt;
                        tt.BeginAnimation(TranslateTransform.XProperty, hideAnimation);
                        DoubleAnimation opacityAnimation = new DoubleAnimation(1, 0.7, new Duration(TimeSpan.FromSeconds(0.3)));
                        this.Button.BeginAnimation(OpacityProperty, opacityAnimation);
                    }

                    if (this.SelectedChanged != null)
                    {
                        this.SelectedChanged(this, new RoutedEventArgs());
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The Button_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            this.IsSelected = true;
        }

        /// <summary>
        /// The Button_ mouse enter.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ButtonMouseEnter(object sender, MouseEventArgs e)
        {
            if (this.IsSelected)
            {
                return;
            }

            if ((this.SettingsPage == null || this.Button.Content.Equals(this.SettingsPage))
                && this.SettingsPage != null)
            {
                return;
            }

            DoubleAnimation showAnimation = new DoubleAnimation(0, 5, new Duration(TimeSpan.FromSeconds(0.3)));
            TranslateTransform translateTransform = new TranslateTransform();
            this.Button.RenderTransform = translateTransform;
            showAnimation.DecelerationRatio = 0.8;
            translateTransform.BeginAnimation(TranslateTransform.XProperty, showAnimation);

            DoubleAnimation opacityAnimation = new DoubleAnimation(0.7, 1, new Duration(TimeSpan.FromSeconds(0.3)));
            this.Button.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        /// <summary>
        /// The Button_ mouse leave.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ButtonMouseLeave(object sender, MouseEventArgs e)
        {
            if (this.IsSelected)
            {
                return;
            }

            if ((this.SettingsPage == null || this.Button.Content.Equals(this.SettingsPage)) && this.SettingsPage != null)
            {
                return;
            }

            DoubleAnimation hideAnimation = new DoubleAnimation(5, 0, new Duration(TimeSpan.FromSeconds(0.2)))
                                                {
                                                    DecelerationRatio
                                                        =
                                                        0.8
                                                };
            TranslateTransform translateTransform = new TranslateTransform();
            this.Button.RenderTransform = translateTransform;
            translateTransform.BeginAnimation(TranslateTransform.XProperty, hideAnimation);

            DoubleAnimation opacityAnimation = new DoubleAnimation(1, 0.7, new Duration(TimeSpan.FromSeconds(0.3)));
            this.Button.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        #endregion
    }
}