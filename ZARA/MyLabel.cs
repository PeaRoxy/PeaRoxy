// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyLabel.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The my label.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ZARA
{
    #region

    using System.Drawing;
    using System.Windows.Forms;

    #endregion

    /// <summary>
    ///     The my label.
    /// </summary>
    internal class MyLabel : Label
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the disabled fore color.
        /// </summary>
        public Color DisabledForeColor { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The on paint.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            this.UseCompatibleTextRendering = false;
            if (this.Enabled)
            {
                base.OnPaint(e);
            }
            else
            {
                Rectangle r = DeflateRect(this.ClientRectangle, this.Padding);
                TextFormatFlags flags = this.CreateTextFormatFlags();
                Color foreColor = this.DisabledForeColor;
                if (foreColor == Color.Empty)
                {
                    foreColor = Color.Gainsboro;
                }

                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, r, foreColor, flags);
            }
        }

        /// <summary>
        /// The deflate Rectangle.
        /// </summary>
        /// <param name="rect">
        /// The Rectangle.
        /// </param>
        /// <param name="padding">
        /// The padding.
        /// </param>
        /// <returns>
        /// The <see cref="Rectangle"/>.
        /// </returns>
        private static Rectangle DeflateRect(Rectangle rect, Padding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Top;
            rect.Width -= padding.Horizontal;
            rect.Height -= padding.Vertical;
            return rect;
        }

        /// <summary>
        /// The translate alignment for GDI.
        /// </summary>
        /// <param name="align">
        /// The align.
        /// </param>
        /// <returns>
        /// The <see cref="TextFormatFlags"/>.
        /// </returns>
        private static TextFormatFlags TranslateAlignmentForGdi(ContentAlignment align)
        {
            if (align == ContentAlignment.BottomCenter || align == ContentAlignment.BottomLeft
                || align == ContentAlignment.BottomRight)
            {
                return TextFormatFlags.Bottom;
            }

            if (align == ContentAlignment.MiddleCenter || align == ContentAlignment.MiddleLeft
                || align == ContentAlignment.MiddleRight)
            {
                return TextFormatFlags.VerticalCenter;
            }

            return TextFormatFlags.Default;
        }

        /// <summary>
        /// The translate line alignment for GDI.
        /// </summary>
        /// <param name="align">
        /// The align.
        /// </param>
        /// <returns>
        /// The <see cref="TextFormatFlags"/>.
        /// </returns>
        private static TextFormatFlags TranslateLineAlignmentForGdi(ContentAlignment align)
        {
            if (align == ContentAlignment.BottomRight || align == ContentAlignment.MiddleRight
                || align == ContentAlignment.TopRight)
            {
                return TextFormatFlags.Right;
            }

            if (align == ContentAlignment.BottomCenter || align == ContentAlignment.MiddleCenter
                || align == ContentAlignment.TopCenter)
            {
                return TextFormatFlags.HorizontalCenter;
            }

            return TextFormatFlags.Default;
        }

        /// <summary>
        ///     The create text format flags.
        /// </summary>
        /// <returns>
        ///     The <see cref="TextFormatFlags" />.
        /// </returns>
        private TextFormatFlags CreateTextFormatFlags()
        {
            TextFormatFlags flags = this.CreateTextFormatFlags(
                this, 
                this.TextAlign, 
                this.AutoEllipsis, 
                this.UseMnemonic);
            return flags;
        }

        /// <summary>
        /// The create text format flags.
        /// </summary>
        /// <param name="ctl">
        /// The Control.
        /// </param>
        /// <param name="textAlign">
        /// The text align.
        /// </param>
        /// <param name="showEllipsis">
        /// The show ellipsis.
        /// </param>
        /// <param name="useMnemonic">
        /// The use mnemonic.
        /// </param>
        /// <returns>
        /// The <see cref="TextFormatFlags"/>.
        /// </returns>
        private TextFormatFlags CreateTextFormatFlags(
            Control ctl, 
            ContentAlignment textAlign, 
            bool showEllipsis, 
            bool useMnemonic)
        {
            TextFormatFlags flags = this.TextFormatFlagsForAlignmentGdi(textAlign)
                                    | (TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            if (showEllipsis)
            {
                flags |= TextFormatFlags.EndEllipsis;
            }

            if (ctl.RightToLeft == RightToLeft.Yes)
            {
                flags |= TextFormatFlags.RightToLeft;
            }

            if (!useMnemonic)
            {
                return flags | TextFormatFlags.NoPrefix;
            }

            return flags;
        }

        /// <summary>
        /// The text format flags for alignment GDI.
        /// </summary>
        /// <param name="align">
        /// The align.
        /// </param>
        /// <returns>
        /// The <see cref="TextFormatFlags"/>.
        /// </returns>
        private TextFormatFlags TextFormatFlagsForAlignmentGdi(ContentAlignment align)
        {
            TextFormatFlags flags = TextFormatFlags.Default;
            flags |= TranslateAlignmentForGdi(align);
            return flags | TranslateLineAlignmentForGdi(align);
        }

        #endregion
    }
}