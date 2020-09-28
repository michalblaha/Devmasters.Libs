using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Configuration;

namespace Devmasters.Imaging {

    [DefaultProperty("Text"), Serializable]
    public class TextWatermark : BaseWatermark {

        #region Properties

        private bool fullWidthBackground = true;
        private string text;
        private Font font = new Font("Verdana", 10, FontStyle.Regular, GraphicsUnit.Point);
        private Color foregroundColor = Color.White, backgroundColor = Color.Black, borderColor = Color.White;
        private int padding = 3, borderWidth = 0;
        private byte foregroundAlpha = 0xFF, backgroundAlpha = 0x66, borderAlpha = 0x66;

        [Category("Border"), Description("Gets or sets border alpha channel. Value 0 = transparent, 255 = opaque.")]
        [DefaultValue(0x66)]
        public byte BorderAlpha {
            get { return borderAlpha; }
            set { borderAlpha = value; }
        }

        [Category("Border"), Description("Gets or sets color of border line.")]
        public Color BorderColor {
            get { return borderColor; }
            set { borderColor = value; }
        }

        [Category("Border"), Description("Gets or sets width of border line. Set to 0 to disable border.")]
        public int BorderWidth {
            get { return borderWidth; }
            set {
                if (value < 0) throw new ArgumentOutOfRangeException();
                borderWidth = value;
            }
        }

        [Category("Foreground"), Description("Gets or sets foreground alpha channel. Value 0 = transparent, 255 = opaque.")]
        [DefaultValue(0xFF)]
        public byte ForegroundAlpha {
            get { return foregroundAlpha; }
            set { foregroundAlpha = value; }
        }

        [Category("Foreground"), Description("Gets or sets color of watermark foreground (text).")]
        public Color ForegroundColor {
            get { return foregroundColor; }
            set { foregroundColor = value; }
        }

        [Category("Foreground"), Description("Gets or sets font used for watermark text.")]
        public Font Font {
            get { return font; }
            set { font = value; }
        }

        [Category("Foreground"), Description("Gets or sets text of watermark.")]
        public string Text {
            get { return text; }
            set {
                if (value == null) value = string.Empty;
                text = value;
            }
        }

        [Category("Background"), Description("Gets or sets color or watermark background.")]
        public Color BackgroundColor {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        [Category("Background"), Description("Gets or sets background alpha channel. Value 0 = transparent, 255 = opaque.")]
        [DefaultValue(0x66)]
        public byte BackgroundAlpha {
            get { return backgroundAlpha; }
            set { backgroundAlpha = value; }
        }

        [Category("Background"), Description("Gets or sets number of pixels between edge of text and watermark background.")]
        [DefaultValue(3)]
        public int Padding {
            get { return padding; }
            set {
                if (value < 0) throw new ArgumentOutOfRangeException();
                padding = value;
            }
        }

        [Category("Background"), Description("Gets or sets if watermark background spans over full width of image.")]
        [DefaultValue(true)]
        public bool FullWidthBackground {
            get { return fullWidthBackground; }
            set { fullWidthBackground = value; }
        }

        [Browsable(false)]
        private Brush ForegroundBrush {
            get { return new SolidBrush(Color.FromArgb(this.ForegroundAlpha, this.ForegroundColor)); }
        }

        [Browsable(false)]
        private Brush BackgroundBrush {
            get { return new SolidBrush(Color.FromArgb(this.BackgroundAlpha, this.BackgroundColor)); }
        }

        [Browsable(false)]
        private Pen BorderPen {
            get {
                return new Pen(Color.FromArgb(this.BorderAlpha, this.BorderColor), this.BorderWidth);
            }
        }

        #endregion

        public override Image Render(Image image) {
            // Validation and preparation
            if (image == null) throw new ArgumentNullException("image");
            if (!this.Enabled || string.IsNullOrEmpty(this.Text)) return image;

            using (Graphics g = Graphics.FromImage(image)) {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Draw background
                Rectangle bgRect = Rectangle.Round(GetBackgroundRectangleF(g, this.FullWidthBackground, image.Size));
                g.FillRectangle(this.BackgroundBrush, bgRect);
                if (this.BorderWidth > 0) g.DrawRectangle(this.BorderPen, bgRect);

                // Draw foreground
                PointF fgPos = bgRect.Location;
                if (this.FullWidthBackground) fgPos = GetBackgroundRectangleF(g, false, image.Size).Location;
                fgPos.X += this.Padding;
                fgPos.Y += this.Padding;
                g.DrawString(this.Text, this.Font, this.ForegroundBrush, fgPos);
                return image;
            }
        }

        private RectangleF GetBackgroundRectangleF(Graphics g, bool fullWidth, Size s) {
            // Determine background size 
            SizeF bgSize = g.MeasureString(this.text, this.Font);
            bgSize.Width += this.Padding * 2;
            bgSize.Height += this.Padding * 2;
            if (fullWidth) bgSize.Width = s.Width - 2 * this.margin;

            // Determine background position
            RectangleF bgRect = new RectangleF(new Point(0, 0), bgSize);
            switch (this.Position) {
                case ContentAlignment.BottomCenter:
                    bgRect.X = (s.Width - bgRect.Width) / 2;
                    bgRect.Y = s.Height - bgRect.Height - this.Margin;
                    break;
                case ContentAlignment.BottomLeft:
                    bgRect.X = this.Margin;
                    bgRect.Y = s.Height - bgRect.Height - this.Margin;
                    break;
                case ContentAlignment.BottomRight:
                    bgRect.X = s.Width - bgRect.Width - this.Margin;
                    bgRect.Y = s.Height - bgRect.Height - this.Margin;
                    break;
                case ContentAlignment.MiddleCenter:
                    bgRect.X = (s.Width - bgRect.Width) / 2;
                    bgRect.Y = (s.Height - bgRect.Height) / 2 - this.Margin;
                    break;
                case ContentAlignment.MiddleLeft:
                    bgRect.X = this.Margin;
                    bgRect.Y = (s.Height - bgRect.Height) / 2 - this.Margin;
                    break;
                case ContentAlignment.MiddleRight:
                    bgRect.X = s.Width - bgRect.Width - this.Margin;
                    bgRect.Y = (s.Height - bgRect.Height) / 2 - this.Margin;
                    break;
                case ContentAlignment.TopCenter:
                    bgRect.X = (s.Width - bgRect.Width) / 2;
                    bgRect.Y = this.Margin;
                    break;
                case ContentAlignment.TopLeft:
                    bgRect.X = this.Margin;
                    bgRect.Y = this.Margin;
                    break;
                case ContentAlignment.TopRight:
                    bgRect.X = s.Width - bgRect.Width - this.Margin;
                    bgRect.Y = this.Margin;
                    break;
                default:
                    break;
            }
            return bgRect;
        }

    }
}
