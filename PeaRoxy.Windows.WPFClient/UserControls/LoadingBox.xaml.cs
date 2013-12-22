// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoadingBox.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for LoadingBox.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media.Animation;

    #endregion

    /// <summary>
    ///     Interaction logic for LoadingBox.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class LoadingBox
    {
        #region Fields

        /// <summary>
        /// The _is visible.
        /// </summary>
        private bool isVisible;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingBox"/> class.
        /// </summary>
        public LoadingBox()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether is visible.
        /// </summary>
        public new bool IsVisible
        {
            // ReSharper disable once UnusedMember.Global
            get
            {
                return this.isVisible;
            }

            set
            {
                if (this.isVisible == value)
                {
                    return;
                }

                DoubleAnimation loadingAnimation = new DoubleAnimation(
                    (!value).GetHashCode(), 
                    value.GetHashCode(), 
                    new Duration(TimeSpan.FromSeconds(0.5)));
                this.BeginAnimation(OpacityProperty, loadingAnimation);
                foreach (LoadingItem loadingItem in this.LoadingItemsGrid.Children.OfType<LoadingItem>())
                {
                    loadingItem.IsPlaying = value;
                }

                this.isVisible = value;
            }
        }

        #endregion
    }
}