using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Devmasters.Imaging
{

    [DefaultProperty("Title")]
    public partial class ExifPhotograph : IDisposable
    {
        // Private variables
        private bool disposed = false, changed = false, autoSave;
        private InMemoryImage image;
        private string fileName;
        private Encoding asciiValuesEncoding = Encoding.ASCII;

        // Constructors

        public ExifPhotograph(Image i, string fileName, bool autoSave)
        {
            this.image = new InMemoryImage(i);
            this.fileName = fileName;
            this.autoSave = autoSave;
        }

        public ExifPhotograph(Image i, string fileName, bool autoSave, Encoding encoding)
        {
            this.image = new InMemoryImage(i);
            this.fileName = fileName;
            this.autoSave = autoSave;
            this.asciiValuesEncoding = encoding;
        }
        public ExifPhotograph(string fileName)
            : this(fileName, false)
        { }

        public ExifPhotograph(string fileName, bool autoSave)
        {
            this.image = new InMemoryImage(fileName);
            this.fileName = fileName;
            this.autoSave = autoSave;
        }

        public ExifPhotograph(string fileName, bool autoSave, Encoding encoding)
        {
            this.image = new InMemoryImage(fileName);
            this.fileName = fileName;
            this.autoSave = autoSave;
            this.asciiValuesEncoding = encoding;
        }

        // Public methods

        public void RenderWatermark(BaseWatermark watermark)
        {
            if (watermark == null) throw new ArgumentNullException("watermark");
            if (!watermark.Enabled) return;
            this.image = new InMemoryImage(watermark.Render(this.image.Image));
            this.changed = true;
        }

        public void RemoveAllMetadata()
        {
            foreach (int propertyId in this.image.Image.PropertyIdList)
            {
                if (propertyId != (int)ExifTagName.JPEGInterFormat &&
                    propertyId != (int)ExifTagName.JPEGInterLength &&
                    propertyId != (int)ExifTagName.ChrominanceTable &&
                    propertyId != (int)ExifTagName.LuminanceTable)
                {
                    this.image.Image.RemovePropertyItem(propertyId);
                }
            }
            this.changed = true;
            this.Save();
        }

        public void RemoveSelectedMetadata(bool removeThumbnail, bool removeMakerNote, bool removeDateTime, bool removeOther)
        {
            if (!removeThumbnail && !removeMakerNote && !removeDateTime && !removeOther) return;                // no work
            if (removeThumbnail && removeMakerNote && removeDateTime && removeOther) this.RemoveAllMetadata();  // remove everything

            foreach (int propertyId in this.image.Image.PropertyIdList)
            {
                switch (propertyId)
                {
                    case (int)ExifTagName.ExifMakerNote:
                        if (removeMakerNote) this.image.Image.RemovePropertyItem(propertyId);
                        break;
                    case (int)ExifTagName.DateTime:
                    case (int)ExifTagName.ExifDTDigitized:
                    case (int)ExifTagName.ExifDTDigSS:
                    case (int)ExifTagName.ExifDTOrig:
                    case (int)ExifTagName.ExifDTOrigSS:
                        if (removeDateTime) this.image.Image.RemovePropertyItem(propertyId);
                        break;
                    case (int)ExifTagName.ThumbnailFormat:
                    case (int)ExifTagName.ThumbnailWidth:
                    case (int)ExifTagName.ThumbnailHeight:
                    case (int)ExifTagName.ThumbnailColorDepth:
                    case (int)ExifTagName.ThumbnailPlanes:
                    case (int)ExifTagName.ThumbnailRawBytes:
                    case (int)ExifTagName.ThumbnailSize:
                    case (int)ExifTagName.ThumbnailCompressedSize:
                    case (int)ExifTagName.ThumbnailData:
                    case (int)ExifTagName.ThumbnailImageWidth:
                    case (int)ExifTagName.ThumbnailImageHeight:
                    case (int)ExifTagName.ThumbnailBitsPerSample:
                    case (int)ExifTagName.ThumbnailCompression:
                    case (int)ExifTagName.ThumbnailPhotometricInterp:
                    case (int)ExifTagName.ThumbnailImageDescription:
                    case (int)ExifTagName.ThumbnailEquipMake:
                    case (int)ExifTagName.ThumbnailEquipModel:
                    case (int)ExifTagName.ThumbnailStripOffsets:
                    case (int)ExifTagName.ThumbnailOrientation:
                    case (int)ExifTagName.ThumbnailSamplesPerPixel:
                    case (int)ExifTagName.ThumbnailRowsPerStrip:
                    case (int)ExifTagName.ThumbnailStripBytesCount:
                    case (int)ExifTagName.ThumbnailResolutionX:
                    case (int)ExifTagName.ThumbnailResolutionY:
                    case (int)ExifTagName.ThumbnailPlanarConfig:
                    case (int)ExifTagName.ThumbnailResolutionUnit:
                    case (int)ExifTagName.ThumbnailTransferFunction:
                    case (int)ExifTagName.ThumbnailSoftwareUsed:
                    case (int)ExifTagName.ThumbnailDateTime:
                    case (int)ExifTagName.ThumbnailArtist:
                    case (int)ExifTagName.ThumbnailWhitePoint:
                    case (int)ExifTagName.ThumbnailPrimaryChromaticities:
                    case (int)ExifTagName.ThumbnailYCbCrCoefficients:
                    case (int)ExifTagName.ThumbnailYCbCrSubsampling:
                    case (int)ExifTagName.ThumbnailYCbCrPositioning:
                    case (int)ExifTagName.ThumbnailRefBlackWhite:
                    case (int)ExifTagName.ThumbnailCopyRight:
                        if (removeThumbnail) this.image.Image.RemovePropertyItem(propertyId);
                        break;
                    case (int)ExifTagName.JPEGInterFormat:
                    case (int)ExifTagName.JPEGInterLength:
                    case (int)ExifTagName.ChrominanceTable:
                    case (int)ExifTagName.LuminanceTable:
                        // Not removable properties
                        break;
                    default:
                        if (removeOther) this.image.Image.RemovePropertyItem(propertyId);
                        break;
                }
            }
            this.changed = true;
        }

        public void FitTo(int width, int height)
        {
            FitTo(new Size(width, height));
        }

        public void FitTo(Size size)
        {
            // Validate
            if (this.disposed) throw new ObjectDisposedException("ExifMetadata");
            if (size.Width < 50) throw new ArgumentOutOfRangeException("size");
            if (size.Height < 50) throw new ArgumentOutOfRangeException("size");
            if (size.Width >= this.image.Image.Width && size.Height >= this.image.Image.Height) return; // no work here

            // Compute new size
            size = Utils.GetFitSize(image.Image.Size, size);

            // Resize image.Image
            Bitmap resized = new Bitmap(this.image.Image, size);
            Utils.CopyProperties(this.image.Image, resized);
            this.image = new InMemoryImage(resized);
            this.changed = true;
        }

        public ExifData GetExifData()
        {
            ExifData d = new ExifData();
            d.Aperture = this.Aperture;
            d.Author = this.Author;
            d.Copyright = this.Copyright;
            d.DateTimeDigitized = this.DateTimeDigitized;
            d.DateTimeLastModified = this.DateTimeLastModified;
            d.DateTimeOriginal = this.DateTimeOriginal;
            d.Description = this.Description;
            d.ExifVersion = this.ExifVersion;
            d.ExposureMeteringMode = this.ExposureMeteringMode;
            d.ExposureProgram = this.ExposureProgram;
            d.ExposureTime = this.ExposureTime;
            d.FlashFired = this.FlashFired;
            d.FlashpixVersion = this.FlashpixVersion;
            d.FocalLength = this.FocalLength;
            d.FocalLengthIn35mm = this.FocalLengthIn35mm;
            d.GPSLatitude = this.GPSLatitude;
            d.GPSLongitude = this.GPSLongitude;
            d.GPSVersionID = this.GPSVersionID;
            d.GPSTime = this.GSPTime;
            d.IsoSensitivity = this.IsoSensitivity;
            d.LightSource = this.LightSource;
            d.Maker = this.Maker;
            d.Model = this.Model;
            d.Orientation = this.Orientation;
            d.Resolution = this.Resolution;
            d.ResolutionUnit = this.ResolutionUnit;
            d.Size = this.Size;
            d.Software = this.Software;
            d.SubjectDistance = this.SubjectDistance;
            d.Title = this.Title;
            d.UserComment = this.UserComment;
            d.XPAuthor = this.XPAuthor;
            d.XPComment = this.XPComment;
            d.XPKeywords = this.XPKeywords;
            d.XPSubject = this.XPSubject;
            d.XPTitle = this.XPTitle;

            return d;
        }

        public bool NeedsRotationFlip
        {
            get
            {
                return (this.RecommendedRotateFlipType != RotateFlipType.RotateNoneFlipNone || this.Orientation == ExifOrientation.NeedsRotation);
            }
        }

        public void Save()
        {
            this.Save(RotateFlipType.RotateNoneFlipNone, -1);
        }

        public void Save(RotateFlipType rotation)
        {
            Save(rotation, -1);
        }

        public void Save(RotateFlipType rotation, int jpegQuality)
        {
            // Validate
            if (this.disposed) throw new ObjectDisposedException("ExifMetadata");
            if (jpegQuality < -1 || jpegQuality > 100) throw new ArgumentOutOfRangeException("jpegQuality");

            // Create list of operations
            List<EncoderParameter> paramList = new List<EncoderParameter>();
            if (jpegQuality != -1) paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality));
            if (rotation == RotateFlipType.Rotate90FlipNone)
            {
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformRotate90));
                this.Orientation = ExifOrientation.TopLeft;
            }
            else if (rotation == RotateFlipType.Rotate180FlipNone)
            {
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformRotate180));
                this.Orientation = ExifOrientation.TopLeft;
            }
            else if (rotation == RotateFlipType.Rotate270FlipNone)
            {
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformRotate270));
                this.Orientation = ExifOrientation.TopLeft;
            }
            else if (rotation == RotateFlipType.RotateNoneFlipX)
            {
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformFlipVertical));
                this.Orientation = ExifOrientation.TopLeft;
            }
            else if (rotation == RotateFlipType.Rotate90FlipX)
            {
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformRotate90));
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformFlipVertical));
                this.Orientation = ExifOrientation.TopLeft;
            }
            else if (rotation == RotateFlipType.Rotate180FlipX)
            {
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformRotate180));
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformFlipVertical));
                this.Orientation = ExifOrientation.TopLeft;
            }
            else if (rotation == RotateFlipType.Rotate270FlipX)
            {
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformRotate270));
                paramList.Add(new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)EncoderValue.TransformFlipVertical));
                this.Orientation = ExifOrientation.TopLeft;
            }
            else
                this.Orientation = ExifOrientation.TopLeft;

            if (paramList.Count == 0)
            {
                // No operations defined
                if (this.changed) this.image.Image.Save(this.FileName);
            }
            else
            {
                // Save with params
                EncoderParameters ep = new EncoderParameters(paramList.Count);
                ep.Param = paramList.ToArray();
                this.image.Image.Save(this.FileName, this.GetCodecInfo(), ep);
            }
            this.changed = false;
        }

        public void SaveThumbnail(ThumbnailOptions tn)
        {
            if (tn == null) throw new ArgumentNullException("tn");
            if (!tn.Enabled) return; // No work here

            // Get final file name
            string thumbnailFileName = Path.Combine(Path.GetDirectoryName(this.FileName), string.Format(tn.FileNameTemplate, Path.GetFileNameWithoutExtension(this.FileName)) + "." + tn.FileExtension);

            // Get working size
            Size tnSize = tn.Size;
            if (tn.AutoRotate && this.Orientation == ExifOrientation.NeedsRotation) tnSize = new Size(tn.Size.Height, tn.Size.Width);
            tnSize = Utils.GetFitSize(this.image.Image.Size, tnSize);

            // Create raw thumbnail and rotate it if needed
            using (Image rawTn = new Bitmap(this.image.Image, tnSize))
            {
                if (tn.AutoRotate) rawTn.RotateFlip(this.RecommendedRotateFlipType);

                // Create final thumbnail
                using (Image finalTn = new Bitmap(tn.Size.Width, tn.Size.Height))
                using (Graphics g = Graphics.FromImage(finalTn))
                {
                    g.FillRectangle(new SolidBrush(tn.BackgroundColor), new Rectangle(new Point(0, 0), tn.Size));
                    g.DrawImage(rawTn, new Point((tn.Size.Width - rawTn.Width) / 2, (tn.Size.Height - rawTn.Height) / 2));
                    finalTn.Save(thumbnailFileName, tn.ImageFormat);
                }
            }
            GC.Collect();
        }

        public void Process(ProcessingOptions po)
        {
            if (po == null) throw new ArgumentNullException("po");

            // Remove privacy related metadata
            this.RemoveSelectedMetadata(po.RemoveThumbnail, po.RemoveMakerNote, po.RemoveDateTime, po.RemoveOtherMetadata);

            // Set common properties
            if (po.AuthorSet) this.Author = po.AuthorValue;
            if (po.CopyrightSet) this.Copyright = po.CopyrightValue;
            if (po.DescriptionSet) this.Description = po.DescriptionValue;
            if (po.TitleSet) this.Title = po.TitleValue;
            if (po.UserCommentSet) this.UserComment = po.UserCommentValue;

            // Set time
            if (po.TimeSet)
            {
                this.DateTimeDigitized = po.TimeValue;
                this.DateTimeLastModified = po.TimeValue;
                this.SetPropertyDateTime(Devmasters.Imaging.ExifTagName.ExifDTOrig, po.TimeValue);
            }

            // Apply time shift
            if (po.TimeShiftSet && po.TimeShiftValue != TimeSpan.Zero)
            {
                this.DateTimeDigitized = TimeShift(po, this.DateTimeDigitized);
                this.DateTimeLastModified = TimeShift(po, this.DateTimeLastModified);
                this.DateTimeOriginal = TimeShift(po, this.DateTimeOriginal);
            }

            // Resize image.Image
            if (po.Resize) this.FitTo(po.ResizeValue);

            // Generate thumbnail
            this.SaveThumbnail(po.ThumbnailsOptions);

            // Apply watermark
            this.RenderWatermark(po.WatermarkOptions);

            // Save image.Image
            int jpegQuality = po.Recompress ? (int)po.RecompressQuality : -1;
            RotateFlipType rotation = po.AutoRotate ? this.RecommendedRotateFlipType : RotateFlipType.RotateNoneFlipNone;
            this.Save(rotation, jpegQuality);

            // Auto rename if needed
            if (po.AutoRename)
            {
                // Get new file name
                string fileNameFormat = Path.Combine(Path.GetDirectoryName(this.fileName), this.DecisiveTime.ToString(@"yyyyMMdd\-HHmmss") + "-{0}.jpg");
                string newFileName = string.Empty;
                int t = 0;
                do
                {
                    newFileName = string.Format(fileNameFormat, t.ToString().PadLeft(4, '0'));
                    t++;
                } while (File.Exists(newFileName));

                // Rename file
                File.Move(this.FileName, newFileName);
                this.FileName = newFileName;
            }

        }


        public bool ContainsExifData
        {
            get
            {
                return (IsPropertyDefined(ExifTagName.ExifVer));
            }
        }

        // Public properties

        [Browsable(false)]
        public bool AutoSave
        {
            get { return autoSave; }
            set { autoSave = value; }
        }

        [Browsable(false)]
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        [Browsable(false)]
        public RotateFlipType RecommendedRotateFlipType
        {
            get
            {
                switch (this.Orientation)
                {
                    case Devmasters.Imaging.ExifOrientation.TopLeft:
                        return RotateFlipType.RotateNoneFlipNone;
                    case Devmasters.Imaging.ExifOrientation.TopRight:
                        return RotateFlipType.RotateNoneFlipX;
                    case Devmasters.Imaging.ExifOrientation.BottomRight:
                        return RotateFlipType.Rotate180FlipNone;
                    case Devmasters.Imaging.ExifOrientation.BottomLeft:
                        return RotateFlipType.Rotate180FlipX;
                    case Devmasters.Imaging.ExifOrientation.LeftTop:
                        return RotateFlipType.Rotate90FlipX;
                    case Devmasters.Imaging.ExifOrientation.RightTop:
                        return RotateFlipType.Rotate90FlipNone;
                    case Devmasters.Imaging.ExifOrientation.RightBottom:
                        return RotateFlipType.Rotate270FlipX;
                    case Devmasters.Imaging.ExifOrientation.LeftBottom:
                        return RotateFlipType.Rotate270FlipNone;
                    default:
                        return RotateFlipType.RotateNoneFlipNone;
                }
            }
        }

        [Browsable(false)]
        public bool Changed
        {
            get { return this.changed; }
        }

        [Browsable(false)]
        public Encoding AsciiValuesEncoding
        {
            get { return this.asciiValuesEncoding; }
            set { this.asciiValuesEncoding = value; }
        }

        [Browsable(false)]
        public DateTime DecisiveTime
        {
            get
            {
                if (this.DateTimeOriginal > DateTime.MinValue) return this.DateTimeOriginal;
                if (this.DateTimeDigitized > DateTime.MinValue) return this.DateTimeDigitized;
                try
                {
                    return File.GetLastWriteTime(this.FileName);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        #region Windows metadata access properties

        [Category("[Windows metadata]"), Description("Image title as used by Windows XP and above.")]
        public string XPTitle
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.ImageDescription)) return this.Description;
                if (!IsPropertyDefined(ExifTagName.XPTitle)) return string.Empty;
                return this.GetPropertyStringUnicode(ExifTagName.XPTitle, string.Empty);
            }
            set
            {
                this.Description = value;
                this.SetPropertyStringUnicode(ExifTagName.XPTitle, value);
            }
        }

        [Category("[Windows metadata]"), Description("Image author  as used by Windows XP and above.")]
        public string XPAuthor
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.Artist)) return this.Author;
                if (!IsPropertyDefined(ExifTagName.XPAuthor)) return string.Empty;
                return this.GetPropertyStringUnicode(ExifTagName.XPAuthor, string.Empty);
            }
            set
            {
                this.Author = value;
                this.SetPropertyStringUnicode(ExifTagName.XPAuthor, value);
            }
        }

        [Category("[Windows metadata]"), Description("Image comment as used by Windows XP and above.")]
        public string XPComment
        {
            get { return this.GetPropertyStringUnicode(ExifTagName.XPComment, string.Empty); }
            set { this.SetPropertyStringUnicode(ExifTagName.XPComment, value); }
        }

        [Category("[Windows metadata]"), Description("Image keywords as used by Windows XP and above.")]
        public string XPKeywords
        {
            get { return this.GetPropertyStringUnicode(ExifTagName.XPKeywords, string.Empty); }
            set { this.SetPropertyStringUnicode(ExifTagName.XPKeywords, value); }
        }

        [Category("[Windows metadata]"), Description("Image subject as used by Windows XP and above.")]
        public string XPSubject
        {
            get { return this.GetPropertyStringUnicode(ExifTagName.XPSubject, string.Empty); }
            set { this.SetPropertyStringUnicode(ExifTagName.XPSubject, value); }
        }

        #endregion

        #region Exif metadata access properties

        // Equipment information

        [Category("Equipment"), Description("Manufacturer of camera.")]
        public string Maker
        {
            get { return GetPropertyStringASCII(ExifTagName.EquipMake, ""); }
            set { SetPropertyStringASCII(ExifTagName.EquipMake, value); }
        }

        [Category("Equipment"), Description("Model of camera.")]
        public string Model
        {
            get { return GetPropertyStringASCII(ExifTagName.EquipModel, ""); }
            set { SetPropertyStringASCII(ExifTagName.EquipModel, value); }
        }

        [Category("Equipment"), Description("Software/firmware and version.")]
        public string Software
        {
            get { return GetPropertyStringASCII(ExifTagName.Firmware, ""); }
            set { SetPropertyStringASCII(ExifTagName.Firmware, value); }
        }

        // Notes



        // Image parameters

        [Category("Image info"), Description("Author name.")]
        public string Author
        {
            get { return GetPropertyStringASCII(ExifTagName.Artist, ""); }
            set { SetPropertyStringASCII(ExifTagName.Artist, value); }
        }

        [Category("Image info"), Description("Copyright information.")]
        public string Copyright
        {
            get { return GetPropertyStringASCII(ExifTagName.Copyright, ""); }
            set { SetPropertyStringASCII(ExifTagName.Copyright, value); }
        }

        [Category("Image info"), Description("Title of image.Image.")]
        public string Title
        {
            get { return GetPropertyStringASCII(ExifTagName.ImageTitle, ""); }
            set { SetPropertyStringASCII(ExifTagName.ImageTitle, value); }
        }

        [Category("Image info"), Description("Description of image.Image.")]
        public string Description
        {
            get { return GetPropertyStringASCII(ExifTagName.ImageDescription, ""); }
            set { SetPropertyStringASCII(ExifTagName.ImageDescription, value); }
        }

        [Category("Image info"), Description("User comment on image.Image.")]
        public string UserComment
        {
            get
            {
                return GetPropertyStringExif(ExifTagName.ExifUserComment, "");
            }
            set
            {
                SetPropertyStringExif(ExifTagName.ExifUserComment, value);
            }
        }

        [Category("Image info"), Description("Date and time when image.Image was digitized.")]
        public DateTime DateTimeDigitized
        {
            get { return GetPropertyDateTime(ExifTagName.ExifDTDigitized, DateTime.MinValue); }
            set { SetPropertyDateTime(ExifTagName.ExifDTDigitized, value); }
        }

        [Category("Image info"), Description("Date and time when image.Image was last modified.")]
        public DateTime DateTimeLastModified
        {
            get { return GetPropertyDateTime(ExifTagName.DateTime, DateTime.MinValue); }
            set { SetPropertyDateTime(ExifTagName.DateTime, value); }
        }

        [Category("Image info"), Description("Image quality rating. This is Devmasters custom property.")]
        [DefaultValue(Rating.NotRated)]
        public Rating Rating
        {
            get
            {
                if (this.IsPropertyDefined(ExifTagName.X_DevmastersRating) &&
                    this.image.Image.GetPropertyItem((int)ExifTagName.X_DevmastersRating).Type == (short)Devmasters.Imaging.ExifDataType.String &&
                    Regex.IsMatch(this.GetPropertyStringASCII(ExifTagName.X_DevmastersRating, ""), @"^Devmasters\.Rating\.\d$"))
                {
                    return (Rating)int.Parse(this.GetPropertyStringASCII(ExifTagName.X_DevmastersRating, "").Substring(16));
                }
                else
                {
                    return Rating.NotRated;
                }
            }
            set
            {
                if (this.IsPropertyDefined(ExifTagName.X_DevmastersRating)) this.image.Image.RemovePropertyItem((int)ExifTagName.X_DevmastersRating);
                this.SetPropertyStringASCII(ExifTagName.X_DevmastersRating, "Devmasters.Rating." + (int)value);
            }
        }

        [Category("Image info"), Description("The orientation of the camera relative to the scene, when the image.Image was captured. The relation of the '0th row' and '0th column' to visual position is shown.")]
        public ExifOrientation Orientation
        {
            get { return (ExifOrientation)GetPropertyShort(ExifTagName.Orientation, (short)ExifOrientation.TopLeft); }
            set { SetPropertyShort(ExifTagName.Orientation, (short)value); }
        }

        [Category("Image info"), Description("Image size in pixels.")]
        public Size Size
        {
            get { return this.image.Image.Size; }
        }

        [Category("Image info"), Description("EXIF format version.")]
        public Version ExifVersion
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.ExifVer))
                {
                    byte[] v = GetProperty(ExifTagName.ExifVer, null);
                    if (v == null || v.Length != 4) return new Version();
                    string vs = System.Text.Encoding.ASCII.GetString(v, 0, 2) + "." + System.Text.Encoding.ASCII.GetString(v, 2, 2);
                    return new Version(vs);
                }
                else
                {
                    return new Version();
                }
            }
        }

        [Category("Image info"), Description("Flashpix format version.")]
        public Version FlashpixVersion
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.ExifFPXVer))
                {
                    byte[] v = GetProperty(ExifTagName.ExifFPXVer, null);
                    if (v == null || v.Length != 4) return new Version();
                    string vs = System.Text.Encoding.ASCII.GetString(v, 0, 2) + "." + System.Text.Encoding.ASCII.GetString(v, 2, 2);
                    return new Version(vs);
                }
                else
                {
                    return new Version();
                }
            }
        }

        [Category("Image info"), Description("Embedded image.Image thumbnail.")]
        public Image Thumbnail
        {
            get
            {
                if (!IsPropertyDefined(ExifTagName.ThumbnailData)) return null;
                using (MemoryStream ms = new MemoryStream(this.GetProperty(ExifTagName.ThumbnailData, null)))
                {
                    return Image.FromStream(ms);
                }
            }
        }

        [Category("Image info"), Description("Image resolution (see ResolutionUnit for unit).")]
        public PointF Resolution
        {
            get
            {
                PointF p = new PointF(72, 72);
                if (this.IsPropertyDefined(ExifTagName.XResolution)) p.X = this.GetPropertyRational(ExifTagName.XResolution).ToFloat();
                if (this.IsPropertyDefined(ExifTagName.YResolution))
                {
                    p.Y = this.GetPropertyRational(ExifTagName.YResolution).ToFloat();
                }
                else
                {
                    p.Y = p.X;
                }
                return p;
            }
        }

        [Category("Image info"), Description("Unit for image.Image resolution value.")]
        [DefaultValue(ExifResolutionUnit.Inches)]
        public ExifResolutionUnit ResolutionUnit
        {
            get { return (ExifResolutionUnit)this.GetPropertyShort(ExifTagName.ResolutionUnit, (short)ExifResolutionUnit.Inches); }
        }

        // Shooting conditions

        [Category("Shooting conditions"), Description("Exposure time in fractions of second (1/x).")]
        public int ExposureTime
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.ExifExposureTime))
                {
                    return GetPropertyRational(ExifTagName.ExifExposureTime).Denominator;
                }
                else if (IsPropertyDefined(ExifTagName.ExifShutterSpeed))
                {
                    return (int)Math.Pow(2, GetPropertyRational(ExifTagName.ExifShutterSpeed).ToDouble());
                }
                else
                {
                    return 0;
                }
            }
        }

        [Category("Shooting conditions"), Description("Aperture value as F-number.")]
        public decimal Aperture
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.ExifFNumber))
                {
                    return GetPropertyRational(ExifTagName.ExifFNumber).ToDecimal();
                }
                else if (IsPropertyDefined(ExifTagName.ExifAperture))
                {
                    return (decimal)Math.Pow(Math.Sqrt(2), GetPropertyRational(ExifTagName.ExifAperture).ToDouble());
                }
                else
                {
                    return 0;
                }
            }
        }

        [Category("Shooting conditions"), Description("Exposure program.")]
        public ExifExposureProgram ExposureProgram
        {
            get { return (ExifExposureProgram)GetPropertyShort(ExifTagName.ExifExposureProg, (short)ExifExposureProgram.Unknown); }
        }

        [Category("Shooting conditions"), Description("Exposure metering mode.")]
        public ExifExposureMeteringMode ExposureMeteringMode
        {
            get { return (ExifExposureMeteringMode)GetPropertyShort(ExifTagName.ExifMeteringMode, (short)ExifExposureMeteringMode.Unknown); }
        }

        [Category("Shooting conditions"), Description("CCD sensitivity equivalent to Ag-Hr film speedrate.")]
        public int IsoSensitivity
        {
            get { return GetPropertyShort(ExifTagName.ExifISOSpeed, 0); }
        }

        [Category("Shooting conditions"), Description("Subject distance in meters.")]
        public decimal SubjectDistance
        {
            get { return GetPropertyRational(ExifTagName.ExifSubjectDist).ToDecimal(); }
        }

        [Category("Shooting conditions"), Description("Focal length of lenses in mm.")]
        public decimal FocalLength
        {
            get { return GetPropertyRational(ExifTagName.ExifFocalLength).ToDecimal(); }
        }

        [Category("Shooting conditions"), Description("Focal length in 35 mm film.")]
        public short FocalLengthIn35mm
        {
            get { return GetPropertyShort(ExifTagName.ExifFocalLengthIn35mmFilm, 0); }
        }

        [Category("Shooting conditions"), Description("Source of main light.")]
        public ExifLightSource LightSource
        {
            get { return (ExifLightSource)GetPropertyShort(ExifTagName.ExifLightSource, (short)ExifLightSource.Unknown); }
        }

        [Category("Shooting conditions"), Description("Flash mode.")]
        public bool FlashFired
        {
            get { return GetPropertyShort(ExifTagName.ExifFlash, 0) != 0; }
        }

        [Category("Shooting conditions"), Description("Date and time when image.Image was taken. This value should not be modified by user program.")]
        public DateTime DateTimeOriginal
        {
            get { return GetPropertyDateTime(ExifTagName.ExifDTOrig, DateTime.MinValue); }
            private set { SetPropertyDateTime(ExifTagName.ExifDTOrig, value); }
        }

        public Version GPSVersionID
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.GpsVer))
                {
                    byte[] v = GetProperty(ExifTagName.GpsVer, null);
                    if (v == null || v.Length != 4) return new Version();
                    return new Version((int)v[0], (int)v[1], (int)v[2], (int)v[3]);
                }
                else
                {
                    return new Version();
                }
            }
        }

        public decimal GPSLatitude
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.GpsLatitudeRef))
                {
                    int iref = 0;
                    string latref = GetPropertyStringASCII(ExifTagName.GpsLatitudeRef, "n").ToLower();
                    switch (latref)
                    {
                        case "n":
                            iref = 1;
                            break;
                        case "s":
                            iref = -1;
                            break;

                        default:
                            iref = 0;
                            break;
                    }
                    return ReadGPSCoordinates(ExifTagName.GpsLatitude) * iref;
                }
                else
                    return decimal.Zero;
            }
        }

        public decimal GPSLongitude
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.GpsLongitudeRef))
                {
                    int iref = 0;
                    string latref = GetPropertyStringASCII(ExifTagName.GpsLongitudeRef, "e").ToLower();
                    switch (latref)
                    {
                        case "e":
                            iref = 1;
                            break;
                        case "w":
                            iref = -1;
                            break;

                        default:
                            iref = 0;
                            break;
                    }
                    return ReadGPSCoordinates(ExifTagName.GpsLongitude) * iref;
                }
                else
                    return decimal.Zero;
            }
        }

        public DateTime GSPTime
        {
            get
            {
                if (IsPropertyDefined(ExifTagName.GpsGpsTime))
                {
                    return GetPropertyDateTime(ExifTagName.GpsGpsTime, DateTime.MinValue);
                }
                return DateTime.MinValue;
            }
        }

        private decimal ReadGPSCoordinates(ExifTagName property)
        {
            if (IsPropertyDefined(property))
            {
                byte[] data = GetProperty(property, null);
                byte[] intnum = new byte[4];
                byte[] intdenom = new byte[4];
                Array.ConstrainedCopy(data, 0, intnum, 0, 4);
                Array.ConstrainedCopy(data, 4, intdenom, 0, 4);
                ExifRational deg = new ExifRational(convertToInt32(intnum), convertToInt32(intdenom));

                Array.ConstrainedCopy(data, 8, intnum, 0, 4);
                Array.ConstrainedCopy(data, 12, intdenom, 0, 4);
                ExifRational min = new ExifRational(convertToInt32(intnum), convertToInt32(intdenom));

                Array.ConstrainedCopy(data, 16, intnum, 0, 4);
                Array.ConstrainedCopy(data, 20, intdenom, 0, 4);
                ExifRational sec = new ExifRational(convertToInt32(intnum), convertToInt32(intdenom));

                decimal coord = (decimal)(deg.ToInt32() + (decimal)(min.ToFloat() / 60) + (decimal)(sec.ToFloat() / 3600));
                return coord;
            }
            else
                return decimal.Zero;
        }



        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this.disposed) return;
            if (this.image.Image != null) this.image.Image.Dispose();
            this.disposed = true;
        }

        #endregion

        private ImageCodecInfo GetCodecInfo()
        {
            // Get filename extension
            string ext = System.IO.Path.GetExtension(this.FileName).ToLower();

            // Find image.Image codec
            foreach (ImageCodecInfo ci in ImageCodecInfo.GetImageEncoders())
            {
                string[] exts = ci.FilenameExtension.ToLower().Replace("*", "").Split(';');
                if (Array.IndexOf<string>(exts, ext) != -1) return ci;
            }

            // No codec found
            return null;
        }

        private static bool ByteArrayStartsWith(byte[] b, params byte[] start)
        {
            if (b == null) throw new ArgumentNullException("b");
            if (start == null) throw new ArgumentNullException("start");
            if (b.Length < start.Length) return false;
            for (int i = 0; i < start.Length; i++) if (b[i] != start[i]) return false;
            return true;
        }

        private static EncoderParameters GetParametersFromRotateFlip(RotateFlipType op, System.Drawing.Imaging.Encoder enc)
        {
            EncoderParameter[] paramList;

            if (op == RotateFlipType.Rotate90FlipNone)
            {
                paramList = new EncoderParameter[] { new EncoderParameter(enc, (long)EncoderValue.TransformRotate90) };
            }
            else if (op == RotateFlipType.Rotate180FlipNone)
            {
                paramList = new EncoderParameter[] { new EncoderParameter(enc, (long)EncoderValue.TransformRotate180) };
            }
            else if (op == RotateFlipType.Rotate270FlipNone)
            {
                paramList = new EncoderParameter[] { new EncoderParameter(enc, (long)EncoderValue.TransformRotate270) };
            }
            else if (op == RotateFlipType.RotateNoneFlipX)
            {
                paramList = new EncoderParameter[] { new EncoderParameter(enc, (long)EncoderValue.TransformFlipVertical) };
            }
            else if (op == RotateFlipType.Rotate90FlipX)
            {
                paramList = new EncoderParameter[] { new EncoderParameter(enc, (long)EncoderValue.TransformRotate90), new EncoderParameter(enc, (long)EncoderValue.TransformFlipVertical) };
            }
            else if (op == RotateFlipType.Rotate180FlipX)
            {
                paramList = new EncoderParameter[] { new EncoderParameter(enc, (long)EncoderValue.TransformRotate180), new EncoderParameter(enc, (long)EncoderValue.TransformFlipVertical) };
            }
            else if (op == RotateFlipType.Rotate270FlipX)
            {
                paramList = new EncoderParameter[] { new EncoderParameter(enc, (long)EncoderValue.TransformRotate270), new EncoderParameter(enc, (long)EncoderValue.TransformFlipVertical) };
            }
            else
            {
                return null;
            }

            EncoderParameters ep = new EncoderParameters(paramList.Length);
            ep.Param = paramList;
            return ep;
        }

        private static DateTime TimeShift(ProcessingOptions po, DateTime original)
        {
            if (original == DateTime.MinValue || original == DateTime.MaxValue) return DateTime.MinValue;
            return original.Add(po.TimeShiftValue);
        }

    }
}