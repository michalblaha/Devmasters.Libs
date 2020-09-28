using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Devmasters.Imaging
{
	public class ImageWatermark : IDisposable
	{

		//position of the watermark...
		public enum WaterMarkPosition
		{
			Center = 1,
			RightBottom = 2,
			RightUpper = 3,
			LeftUpper = 4,
			LeftBottom = 5,
			CenterUpper = 6,
		}

		string watermarkFilename = string.Empty;
		Bitmap watermark;



		public ImageWatermark(string watermarkFilename)
		{
			this.watermarkFilename = watermarkFilename;
			watermark = new Bitmap(this.watermarkFilename);
		}

		public ImageWatermark(Bitmap watermarkImage)
		{
			this.watermarkFilename = string.Empty;
			watermark = new InMemoryImage(watermarkImage).Image;
		}

		private Point GetWatermarkCoordinates(InMemoryImage sourceImage, WaterMarkPosition position)
		{
			float safeMargin = 0.05f;
			float xOffsetPercent = 0f, yOffsetPercent = 0f;
			float xW, yW = 0; //position of left upper corner of watermark
			switch (position)
			{
				case WaterMarkPosition.Center:
					xOffsetPercent = 50;
					yOffsetPercent = 50;
					break;
				case WaterMarkPosition.RightBottom:
					xOffsetPercent = 95;
					yOffsetPercent = 95;
					break;
				case WaterMarkPosition.RightUpper:
					xOffsetPercent = 95;
					yOffsetPercent = 5;
					break;
				case WaterMarkPosition.LeftBottom:
					xOffsetPercent = 5;
					yOffsetPercent = 95;
					break;
				case WaterMarkPosition.LeftUpper:
					xOffsetPercent = 5;
					yOffsetPercent = 5;
					break;
				case WaterMarkPosition.CenterUpper:
					xOffsetPercent = 50;
					yOffsetPercent = 5;
					break;
			}
			xOffsetPercent = xOffsetPercent / 100;
			yOffsetPercent = yOffsetPercent / 100;
			//
			xW = (sourceImage.Image.Size.Height * (xOffsetPercent)) - watermark.Size.Height * xOffsetPercent;
			yW = (sourceImage.Image.Size.Width * (yOffsetPercent)) - watermark.Size.Width * yOffsetPercent;

			//set safe margins
			xW = Math.Max(xW, sourceImage.Image.Size.Height * safeMargin);
			xW = Math.Min(xW, sourceImage.Image.Size.Height * (1 - safeMargin) - watermark.Size.Height);

			yW = Math.Max(yW, sourceImage.Image.Size.Width * safeMargin);
			yW = Math.Min(yW, sourceImage.Image.Size.Width * (1 - safeMargin) - watermark.Size.Width);

			return new Point(Convert.ToInt32(yW), Convert.ToInt32(xW));

		}

		public InMemoryImage Render(InMemoryImage sourceImage, WaterMarkPosition position)
		{
			//from http://www.codeproject.com/KB/GDI-plus/watermark.aspx

			Graphics gSource = Graphics.FromImage(sourceImage.Image);

			ImageAttributes imageAttributes = new ImageAttributes();
			ColorMap colorMap = new ColorMap();

			// The first step in manipulating the watermark image is to replace the 
			// background color (green) with one that is transparent (Alpha=0, R=0, G=0, B=0). 

			colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
			colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
			ColorMap[] remapTable = { colorMap };

			imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

			//The second color manipulation is used to change the opacity of the watermark. 
			//This is done by applying a 5x5 matrix that contains the coordinates for the RGBA space. 
			//By setting the 3rd row and 3rd column to 0.3f we achieve a level of opacity.  
			//The result is a watermark which slightly shows the underlying image.

			float[][] colorMatrixElements = { 
				new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
				new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
				new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
				new float[] {0.0f,  0.0f,  0.0f,  0.5f, 0.0f},
				new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
			};

			ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);

			imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			int xPosOfWm = Math.Min(sourceImage.Image.Width / 25, 10);
			int yPosOfWm = Math.Min(sourceImage.Image.Height / 25, 10);

			Point watPosition = GetWatermarkCoordinates(sourceImage, position);

			gSource.DrawImage(watermark,
				 new Rectangle(watPosition.X, watPosition.Y, watermark.Width,watermark.Height),
				 0,
				 0,
				 watermark.Width,
				 watermark.Height,
				 GraphicsUnit.Pixel,
				 imageAttributes);


			gSource.Dispose();

			return sourceImage;

		}

		#region IDisposable Members

		public void Dispose()
		{
			if (this.watermark != null)
				this.watermark.Dispose();
		}

		#endregion
	}
}
