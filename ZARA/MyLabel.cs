using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.Drawing.Text;
namespace ZARA
{
    class MyLabel : Label
    {
        public Color DisabledForeColor { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.UseCompatibleTextRendering = false;
            if (base.Enabled)
                base.OnPaint(e);
            else
            {
                Rectangle r = DeflateRect(base.ClientRectangle, base.Padding);
                TextFormatFlags flags = this.CreateTextFormatFlags();
                Color foreColor = DisabledForeColor;
                if (foreColor == Color.Empty)
                   foreColor = Color.Gainsboro;
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, r, foreColor, flags);
            }
        }

        private Rectangle DeflateRect(Rectangle rect, Padding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Top;
            rect.Width -= padding.Horizontal;
            rect.Height -= padding.Vertical;
            return rect;
        }

        private TextFormatFlags CreateTextFormatFlags()
        {
            return this.CreateTextFormatFlags(base.Size - this.GetBordersAndPadding());
        }

        private TextFormatFlags CreateTextFormatFlags(Size constrainingSize)
        {
            TextFormatFlags flags = CreateTextFormatFlags(this, this.TextAlign, this.AutoEllipsis, this.UseMnemonic);
            return flags;
        }

        private Size GetBordersAndPadding()
        {
            Size size = base.Padding.Size;
            size += this.SizeFromClientSize(Size.Empty);
            if (this.BorderStyle == BorderStyle.Fixed3D)
                size += new Size(2, 2);
            return size;
        }

        private TextFormatFlags CreateTextFormatFlags(Control ctl, ContentAlignment textAlign, bool showEllipsis, bool useMnemonic)
        {
            TextFormatFlags flags = TextFormatFlagsForAlignmentGDI(textAlign) | (TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            if (showEllipsis)
                flags |= TextFormatFlags.EndEllipsis;
            if (ctl.RightToLeft == RightToLeft.Yes)
                flags |= TextFormatFlags.RightToLeft;
            if (!useMnemonic)
                return (flags | TextFormatFlags.NoPrefix);
            return flags;
        }

        private TextFormatFlags TextFormatFlagsForAlignmentGDI(ContentAlignment align)
        {
            TextFormatFlags flags = TextFormatFlags.Default;
            flags |= TranslateAlignmentForGDI(align);
            return (flags | TranslateLineAlignmentForGDI(align));
        }

        private TextFormatFlags TranslateAlignmentForGDI(ContentAlignment align)
        {
            if (align == ContentAlignment.BottomCenter || align == ContentAlignment.BottomLeft || align == ContentAlignment.BottomRight)
                return TextFormatFlags.Bottom;
            if (align == ContentAlignment.MiddleCenter || align == ContentAlignment.MiddleLeft || align == ContentAlignment.MiddleRight)
                return TextFormatFlags.VerticalCenter;
            return TextFormatFlags.Default;
        }

        private TextFormatFlags TranslateLineAlignmentForGDI(ContentAlignment align)
        {
            if (align == ContentAlignment.BottomRight || align == ContentAlignment.MiddleRight || align == ContentAlignment.TopRight)
                return TextFormatFlags.Right;
            if (align == ContentAlignment.BottomCenter || align == ContentAlignment.MiddleCenter || align == ContentAlignment.TopCenter)
                return TextFormatFlags.HorizontalCenter;
            return TextFormatFlags.Default;
        }
    }
}
