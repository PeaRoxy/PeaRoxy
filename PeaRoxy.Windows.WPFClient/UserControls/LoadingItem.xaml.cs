// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoadingItem.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for LoadingItem.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Media.Animation;

    #endregion

    /// <summary>
    ///     Interaction logic for LoadingItem.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class LoadingItem
    {
        #region Fields

        /// <summary>
        /// The story.
        /// </summary>
        private readonly Storyboard story = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingItem"/> class.
        /// </summary>
        public LoadingItem()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the delay.
        /// </summary>
        public double Delay { get; set; }

        /// <summary>
        /// Sets a value indicating whether is playing.
        /// </summary>
        public bool IsPlaying
        {
            set
            {
                if (value)
                {
                    this.Rect.Opacity = 0;
                    this.Rect.Margin = new Thickness(-5, 0, 0, 0);
                    this.story.Begin(this.Rect, true);
                }
                else
                {
                    this.story.Stop(this.Rect);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The user control_ loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            this.story.BeginTime = TimeSpan.FromSeconds(this.Delay);
            PowerEase ease = new PowerEase { Power = 0.3, EasingMode = EasingMode.EaseInOut };
            DoubleAnimation opacityAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.8)))
                                                   {
                                                       EasingFunction = ease,
                                                       AutoReverse = true
                                                   };
            ThicknessAnimation dbxPos = new ThicknessAnimation(
                new Thickness(0, 0, 0, 0),
                new Thickness(this.ActualWidth, 0, 0, 0),
                new Duration(TimeSpan.FromSeconds(1.6))) { EasingFunction = ease, AutoReverse = true };
            this.story.Children.Add(opacityAnimation);
            this.story.Children.Add(dbxPos);
            Storyboard.SetTarget(opacityAnimation, this.Rect);
            Storyboard.SetTarget(dbxPos, this.Rect);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(dbxPos, new PropertyPath(MarginProperty));
        }

        #endregion
    }
}