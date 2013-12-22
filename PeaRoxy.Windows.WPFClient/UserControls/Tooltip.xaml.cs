// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tooltip.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for Tooltip.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Documents;

    #endregion

    /// <summary>
    ///     Interaction logic for Tooltip.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class Tooltip
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Tooltip"/> class.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public Tooltip(string title, string text)
        {
            this.InitializeComponent();
            this.Text = text;
            this.Title = title;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tooltip"/> class.
        /// </summary>
        public Tooltip()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text
        {
            get
            {
                return this.DataBlock.Text;
            }

            set
            {
                this.DataBlock.Text = string.Empty;

                string[] textBoldParts = value.Replace("[N]", Environment.NewLine).Split(new[] { '[' });
                foreach (string b in textBoldParts)
                {
                    string buggyMs = b;
                    int stringEnd = buggyMs.IndexOf("]", StringComparison.Ordinal);
                    if (stringEnd > -1)
                    {
                        this.DataBlock.Inlines.Add(new Bold(new Run(buggyMs.Substring(0, stringEnd))));
                        buggyMs = buggyMs.Substring(stringEnd + 1);
                    }

                    this.DataBlock.Inlines.Add(new Run(buggyMs));
                }
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get
            {
                return (string)this.TitleLabel.Content;
            }

            set
            {
                this.TitleLabel.Content = value;
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
            if (this.DataBlock.Text == string.Empty && this.DataBlock.Inlines.Count == 0)
            {
                this.TextLabel.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}