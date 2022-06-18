using DGScope.Library;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeWindow
{
    public class ScopeText
    {
        private string text;
        private bool redraw = true;
        private Bitmap bitmap = new Bitmap(1,1);
        private Font font;

        public ScopeColor Color { get; set; }
        public int TextureId { get; private set; } = 0;
        public string Text
        {
            get => text;
            set
            {
                lock (bitmap)
                {
                    bool changed = text != value;
                    text = value;
                    if (changed)
                        redraw = true;
                }
            }
        }
        public Font Font
        {
            get => font;
            set
            {
                lock (bitmap)
                {
                    bool changed = font != value;
                    font = value;
                    if (changed)
                        redraw = true;
                }
            }
        }
        private Bitmap Bitmap
        {
            get
            {
                lock (bitmap)
                {
                    if (!redraw)
                        return bitmap;
                    Bitmap bmp = new Bitmap(1, 1);
                    if (font == null)
                        return bmp;
                    Graphics graphics = Graphics.FromImage(bmp);
                    SizeF stringSize = graphics.MeasureString(text, font);
                    bmp = new Bitmap(bmp, (int)stringSize.Width, (int)stringSize.Height);
                    graphics = Graphics.FromImage(bmp);
                    graphics.FillRectangle(Brushes.Transparent, 0, 0, stringSize.Width, stringSize.Height);
                    graphics.DrawString(text, font, Brushes.White, 0, 0);
                    bitmap = bmp;
                    graphics.Flush();
                    graphics.Dispose();
                }
                return bitmap;
            }
        }

        public PointF Location { get; set; }
        public SizeF GetSize(double scale)
        {
            return new SizeF((float)(scale * bitmap.Width), (float)(scale * bitmap.Height));
        }
        public RectangleF GetBounds(double scale)
        {
            return new RectangleF(Location, GetSize(scale));
        }

        public void GenerateGlTexture()
        {
            if (TextureId == 0)
                TextureId = GL.GenTexture();
            if (redraw && TextureId != 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, TextureId);
                var bmp = Bitmap;
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
                bmp.UnlockBits(data);
                redraw = false;
            }
        }

    }
}
