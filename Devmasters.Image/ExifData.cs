using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Devmasters.Imaging
{
	[Serializable]
	public class ExifData
	{
		public enum ExifDataStringFormat
		{
			Detailed = 1,
			ShortForWeb = 2,
		}

		#region Windows metadata access properties

		[Category("[Windows metadata]"), Description("Image title as used by Windows XP and above.")]
		public string XPTitle
		{
			get;
			set;
		}

		[Category("[Windows metadata]"), Description("Image author  as used by Windows XP and above.")]
		public string XPAuthor
		{
			get;
			set;
		}

		[Category("[Windows metadata]"), Description("Image comment as used by Windows XP and above.")]
		public string XPComment
		{
			get;
			set;
		}

		[Category("[Windows metadata]"), Description("Image keywords as used by Windows XP and above.")]
		public string XPKeywords
		{
			get;
			set;
		}

		[Category("[Windows metadata]"), Description("Image subject as used by Windows XP and above.")]
		public string XPSubject
		{
			get;
			set;
		}

		#endregion

		#region Exif metadata access properties

		// Equipment information

		[Category("Equipment"), Description("Manufacturer of camera.")]
		public string Maker
		{
			get;
			set;
		}

		[Category("Equipment"), Description("Model of camera.")]
		public string Model
		{
			get;
			set;
		}

		[Category("Equipment"), Description("Software/firmware and version.")]
		public string Software
		{
			get;
			set;
		}

		// Notes



		// Image parameters

		[Category("Image info"), Description("Author name.")]
		public string Author
		{
			get;
			set;
		}

		[Category("Image info"), Description("Copyright information.")]
		public string Copyright
		{
			get;
			set;
		}

		[Category("Image info"), Description("Title of image.Image.")]
		public string Title
		{
			get;
			set;
		}

		[Category("Image info"), Description("Description of image.Image.")]
		public string Description
		{
			get;
			set;
		}

		[Category("Image info"), Description("User comment on image.Image.")]
		public string UserComment
		{
			get;
			set;
		}

		[Category("Image info"), Description("Date and time when image.Image was digitized.")]
		public DateTime DateTimeDigitized
		{
			get;
			set;
		}

		[Category("Image info"), Description("Date and time when image.Image was last modified.")]
		public DateTime DateTimeLastModified
		{
			get;
			set;
		}

		[Category("Image info"), Description("The orientation of the camera relative to the scene, when the image.Image was captured. The relation of the '0th row' and '0th column' to visual position is shown.")]
		public ExifOrientation Orientation
		{
			get;
			set;
		}

		[Category("Image info"), Description("Image size in pixels.")]
		public Size Size
		{
			get;
			set;
		}

		[Category("Image info"), Description("EXIF format version.")]
		public Version ExifVersion
		{
			get;
			set;
		}

		[Category("Image info"), Description("Flashpix format version.")]
		public Version FlashpixVersion
		{
			get;
			set;
		}

		[Category("Image info"), Description("Image resolution (see ResolutionUnit for unit).")]
		public PointF Resolution
		{
			get;
			set;
		}

		[Category("Image info"), Description("Unit for image.Image resolution value.")]
		[DefaultValue(ExifResolutionUnit.Inches)]
		public ExifResolutionUnit ResolutionUnit
		{
			get;
			set;
		}

		// Shooting conditions

		[Category("Shooting conditions"), Description("Exposure time in fractions of second (1/x).")]
		public int ExposureTime
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Aperture value as F-number.")]
		public decimal Aperture
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Exposure program.")]
		public ExifExposureProgram ExposureProgram
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Exposure metering mode.")]
		public ExifExposureMeteringMode ExposureMeteringMode
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("CCD sensitivity equivalent to Ag-Hr film speedrate.")]
		public int IsoSensitivity
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Subject distance in meters.")]
		public decimal SubjectDistance
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Focal length of lenses in mm.")]
		public decimal FocalLength
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Focal length of lenses in mm.")]
		public decimal FocalLengthIn35mm
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Source of main light.")]
		public ExifLightSource LightSource
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Flash mode.")]
		public bool FlashFired
		{
			get;
			set;
		}

		[Category("Shooting conditions"), Description("Date and time when image.Image was taken. This value should not be modified by user program.")]
		public DateTime DateTimeOriginal
		{
			get;
			set;
		}

		public Version GPSVersionID
		{ get; set; }

		public decimal GPSLatitude
		{ get; set; }

		public decimal GPSLongitude
		{ get; set; }

		public DateTime GPSTime
		{ get; set; }


		public override string ToString()
		{
			return base.ToString();
		}

		public bool AreBasicDataAvailable
		{
			get
			{
				return (this.ExposureTime != 0 && this.Aperture != 0 && this.FocalLength != 0 && this.IsoSensitivity != 0);
			}
		}

		public string ToString(ExifDataStringFormat format)
		{
			switch (format)
			{
				case ExifDataStringFormat.Detailed:
					return string.Format("1/{0} at f/{1}, {2}mm, ISO {3}", this.ExposureTime.ToString(), this.Aperture.ToString(), this.FocalLength.ToString(), this.IsoSensitivity.ToString());

				case ExifDataStringFormat.ShortForWeb:
					string s = "";

					s = string.Format("1/{0} at f/{1}, {2}mm, ISO {3}",
						   this.ExposureTime.ToString(), this.Aperture.ToString(),
						   this.FocalLength.ToString(), this.IsoSensitivity.ToString()
						   );
					if (this.FlashFired)
						s += ", with flash";
					return s;
				default:
					return this.ToString();
			}
		}

		#endregion

	}
}
