using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Devmasters.Imaging
{
    public class InMemoryImage : IDisposable
    {
        private const int DEFAULT_JPEG_QUALITY = 85;

        #region Enum
        /// <summary>
        /// Sign Position is used by SignImage function to determine the placement of signature
        /// </summary>
        public enum SignPosition
        {
            LeftUp = 1,
            LeftDown = 2,
            RightUp = 3,
            RightDown = 4,
            Center = 5
        }

        public enum InterpolationsQuality
        {
            High,
            Regular,
            Low
        }
        #endregion


        System.Drawing.Bitmap image = null;
        byte[] data = null;

        public InMemoryImage(string filename)
        {
            if (!System.IO.File.Exists(filename))
                throw new FileNotFoundException(filename);

            FileStream fs = null;

            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                long size = fs.Length;
                byte[] buff = new byte[size];
                fs.Read(buff, 0, (int)size);
                data = buff;
                fs.Close();
                initBitmap();
            }
            catch (Exception)
            {
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public InMemoryImage(System.Drawing.Image bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, bitmap.RawFormat);
            data = ms.ToArray();
            ms.Close();
            bitmap.Dispose();
            initBitmap();
        }

        public InMemoryImage(System.IO.Stream stream)
        {
            byte[] buff = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buff, 0, (int)stream.Length);
            data = buff;
            initBitmap();
        }

        public InMemoryImage(byte[] bytes)
        {
            data = bytes;
            initBitmap();
        }


        private void initBitmap()
        {
            MemoryStream ms = new MemoryStream(data);
            this.image = new System.Drawing.Bitmap(ms);
            data = null;

        }

        public System.Drawing.Bitmap Image
        {
            get
            {
                return this.image;
            }
            private set
            {
                Bitmap previous = this.image;
                this.image = value;
                if (!this.image.Equals(previous))
                    if (previous != null)
                        previous.Dispose();
                    else
                        previous = null;
            }
        }

        public void Dispose()
        {
            if (this.data != null)
                this.data = null;

            if (this.image != null)
                image.Dispose();
        }

        public void Crop(int x1, int y1, int width, int height, float ratio)
        {
            x1 = (int)(x1 * ratio); y1 = (int)(y1 * ratio); width = (int)(width * ratio); height = (int)(height * ratio);

            if ((width == 0) || (height == 0))
                this.Image = null;

            // Fixing ratio float mistake
            System.Drawing.Size oSize = this.Image.Size;
            if (width > oSize.Width)
                width = oSize.Width;

            if (height > oSize.Height)
                height = oSize.Height;


            // Cropping area
            Point startPoint = new Point(x1, y1);
            Size rectSize = new Size(width, height);
            Rectangle cropArea = new Rectangle(startPoint, rectSize);
            Crop(cropArea);
        }

        public void Crop(Rectangle cropArea)
        {
            Bitmap croppedImg = this.Image.Clone(cropArea, this.Image.PixelFormat);
            this.Image = croppedImg;
        }

        public RectangleF PositionInBox(Size boundingBox, bool keepAspectRatio)
        {
            RectangleF ret = PositionInBoxF(boundingBox, keepAspectRatio);
            return new Rectangle(Convert.ToInt32(ret.X), Convert.ToInt32(ret.Y), Convert.ToInt32(ret.Width), Convert.ToInt32(ret.Height));
        }
 
        public RectangleF PositionInBoxF(Size boundingBox, bool keepAspectRatio)
        {
            // helpers
            int nw;
            int nh;
            float ar;
            float mar;
            int x;
            int y;
            Bitmap bmp = null;
            Bitmap mybmp = this.Image;

            Size newSize;

            //  we have real bitmap...
            int w = mybmp.Width;
            int h = mybmp.Height;
            int MaxWidth = boundingBox.Width;
            int MaxHeight = boundingBox.Height;

            //  if there is request for recalculation of an bitmap size with aspect ratio... do it
            if (keepAspectRatio)
            {
                ar = ((float)w / (float)h);
                mar = ((float)MaxWidth / (float)MaxHeight);
                if ((ar < mar))
                {
                    nh = MaxHeight;
                    nw = (int)(((float)w / (float)h) * (float)MaxHeight);
                    y = 0;
                    x = (int)(Math.Abs(MaxWidth - nw) / 2);
                }
                else
                {
                    nw = MaxWidth;
                    nh = (int)(((float)h / (float)w) * (float)MaxWidth);
                    x = 0;
                    y = (int)(Math.Abs(MaxHeight - nh) / 2);
                }
            }
            else
            {
                //  we suppose that width and height are already in requested aspect ratio
                nh = MaxHeight;
                nw = MaxWidth;
                x = 0;
                y = 0;

            }

            return new Rectangle(x,y,nw,nh);
        }



        /// <summary>
        /// will take the bitmap and resize it to fit the boundarybox
        /// </summary>
        /// <param name="mybmp">bitmap to resize</param>
        /// <param name="boundingBox">bouding box</param>
        /// <param name="keepAspectRatio">keep aspect ratio ?</param>
        /// <param name="quality">resize quality</param>
        public void Resize(Size boundingBox, bool keepAspectRatio, InterpolationsQuality quality)
        {
            Resize(boundingBox, keepAspectRatio, quality, false);
        }

        public void Resize(Size boundingBox, bool keepAspectRatio, InterpolationsQuality quality, bool crop)
        {
            // helpers
            int nw;
            int nh;
            float ar;
            float mar;
            Bitmap bmp = null;
            Bitmap mybmp = this.Image;

            if (mybmp == null)
            {
                //log.Error("Graphite:ResizeBitmap function null bitmap");
                //return;
                throw new NullReferenceException("this.Image is null");
            }

            try
            {
                //  we have real bitmap...
                int w = mybmp.Width;
                int h = mybmp.Height;
                int MaxWidth = boundingBox.Width;
                int MaxHeight = boundingBox.Height;
                Rectangle cropRectangle = Rectangle.Empty;

                if (crop)
                {
                    //find which side is bigger
                    ar = ((float)w / (float)h);
                    mar = ((float)MaxWidth / (float)MaxHeight);

                    if (w > h) //na sirku
                    {
                        if (ar < mar)
                        {
                            nw = MaxWidth;
                            nh = (int)(((float)h / (float)w) * (float)MaxWidth);

                            int cropMiddleH = (nh - MaxHeight) / 2;
                            cropRectangle = new Rectangle(cropMiddleH, 0, MaxWidth, MaxHeight);

                        }
                        else
                        {

                            nh = MaxHeight;
                            nw = (int)(((float)w / (float)h) * (float)MaxHeight);

                            int cropMiddleW = (nw - MaxWidth) / 2;
                            cropRectangle = new Rectangle(cropMiddleW, 0, MaxWidth, MaxHeight);

                        }

                    }
                    else
                    {
                        //na vysku
                        if (ar < mar)
                        {
                            nw = MaxWidth;
                            nh = (int)(((float)h / (float)w) * (float)MaxWidth);

                            int cropMiddleH = (nh - MaxHeight) / 2;
                            cropRectangle = new Rectangle(0, cropMiddleH, MaxWidth, MaxHeight);
                        }
                        else
                        {

                            nh = MaxHeight;
                            nw = (int)(((float)w / (float)h) * (float)MaxHeight);
                            int cropMiddleW = (nw - MaxWidth) / 2;
                            cropRectangle = new Rectangle(cropMiddleW, 0, MaxWidth, MaxHeight);
                        }

                    }
                    //resize with min side in max size
                    Resize(new Size(nw, nh), false, quality, false);

                    //now crop bigger size to asked size
                    Crop(cropRectangle);

                    return;
                }


                //  if there is request for recalculation of an bitmap size with aspect ratio... do it
                if (keepAspectRatio)
                {
                    ar = ((float)w / (float)h);
                    mar = ((float)MaxWidth / (float)MaxHeight);
                    if ((ar < mar))
                    {
                        nh = MaxHeight;
                        nw = (int)(((float)w / (float)h) * (float)MaxHeight);
                    }
                    else
                    {
                        nw = MaxWidth;
                        nh = (int)(((float)h / (float)w) * (float)MaxWidth);
                    }
                }
                else
                {
                    //  we suppose that width and height are already in requested aspect ratio
                    nh = MaxHeight;
                    nw = MaxWidth;
                }
                //  create new bitmap and canvas
                bmp = new Bitmap(nw, nh, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bmp);
                //  set the requested quality...
                //  by choosing the interpolation and compositing quality
                switch (quality)
                {
                    case InterpolationsQuality.High:
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        break;
                    case InterpolationsQuality.Regular:
                        g.CompositingQuality = CompositingQuality.Default;
                        g.InterpolationMode = InterpolationMode.Default;
                        g.PixelOffsetMode = PixelOffsetMode.Default;
                        g.SmoothingMode = SmoothingMode.Default;
                        break;
                    case InterpolationsQuality.Low:
                        g.CompositingQuality = CompositingQuality.HighSpeed;
                        g.InterpolationMode = InterpolationMode.Low;
                        g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                        g.SmoothingMode = SmoothingMode.HighSpeed;
                        break;
                }
                //  resize the image...
                g.DrawImage(mybmp, 0, 0, nw, nh);
                //  free the resources
                g.Dispose();
            }
            catch (Exception)
            {
                //  in case of trouble, report the exception...
                //log.Error("Graphite:ResizeBitmap function exception", ex);
            }
            //  return the result...
            this.Image = bmp;
        }

        public void Canvas(Size requestedSize, Color background)
        {
            if (this.Image.Size.Width > requestedSize.Width && this.Image.Size.Height > requestedSize.Height)
                throw new ArgumentOutOfRangeException("Requested Width or Height is bigger than current image");

            Bitmap newBmp = new Bitmap(requestedSize.Width, requestedSize.Height, this.Image.PixelFormat);
            Graphics gr = Graphics.FromImage(newBmp);
            gr.FillRectangle(new SolidBrush(background), 0, 0, newBmp.Width, newBmp.Height);
            int x = (requestedSize.Width - this.Image.Size.Width) / 2;
            int y = (requestedSize.Height - this.Image.Size.Height) / 2;


            gr.DrawImage(this.Image, x, y, this.Image.Width, this.Image.Height);
            gr.Save();
            gr.Dispose();

            this.Image = newBmp;

        }

        /// <summary>
        /// shared function saving image as JPEG with specified quality level
        /// </summary>
        /// 
        public bool SaveAsJPEG(string path)
        {
            return SaveAsJPEG(path, DEFAULT_JPEG_QUALITY);
        }

        public bool SaveAsJPEG(string path, int quality)
        {
            bool res = false;
            if (this.Image == null)
            {
                //log.Error("Graphite:SaveAsJPEG function null bitmap");
                return res;
            }

            try
            {
                //  prepare codec parameters
                ImageCodecInfo myCodec = Utils.GetEncoderInfo("image/jpeg");
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
                myEncoderParameters.Param = new EncoderParameter[] { myEncoderParameter };
                //  Save bitmap
                this.Image.Save(path, myCodec, myEncoderParameters);
                //  release resources
                myEncoderParameter.Dispose();
                myEncoderParameters.Dispose();
                //  everything is ok now..
                res = true;
            }
            catch (Exception)
            {
                //log.Error("Graphite:SaveAsJPEG function exception", ex);
                throw;
            }
            //  return the result...
            return res;
        }

        /// <summary>
        /// the same as above, but saving to the stream instead of file
        /// </summary>
        public bool SaveAsJPEG(Stream stream)
        {
            return SaveAsJPEG(stream, DEFAULT_JPEG_QUALITY);
        }

        public bool SaveAsJPEG(Stream stream, int quality)
        {
            bool res = false;
            if (this.Image == null)
            {
                //log.Error("Graphite:SaveAsJPEG function null bitmap");
                return res;
            }

            try
            {
                //  prepare codec parameters
                ImageCodecInfo myCodec = Utils.GetEncoderInfo("image/jpeg");
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
                myEncoderParameters.Param = new EncoderParameter[] { myEncoderParameter };
                this.Image.Save(stream, myCodec, myEncoderParameters);
                //  release resources
                myEncoderParameter.Dispose();
                myEncoderParameters.Dispose();
                //  everything is ok now..
                res = true;
            }
            catch (Exception)
            {
                //log.Error("Graphite:SaveAsJPEG function exception", ex);
                throw;
            }
            //  return the result...
            return res;
        }

        public bool SaveAsPNG(Stream stream)
        {
            bool res = false;
            if (this.Image == null)
            {
                return res;
            }

            try
            {
                this.Image.Save(stream, ImageFormat.Png);

                res = true;
            }
            catch (Exception)
            {
                throw;
            }

            return res;
        }


        public bool SaveAsPNG(string path)
        {
            bool res = false;
            if (this.Image == null)
            {
                return res;
            }

            try
            {
                this.Image.Save(path, ImageFormat.Png);

                res = true;
            }
            catch (Exception)
            {
                throw;
            }

            return res;
        }

    }
}
