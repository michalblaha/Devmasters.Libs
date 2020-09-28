using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Devmasters.Imaging {

    [Serializable]
    public class ProcessingOptions {

        #region Set metadata

        private bool titleSet, descriptionSet, userCommentSet, authorSet, copyrightSet, timeShiftSet, timeSet;
        private string titleValue, descriptionValue, userCommentValue, authorValue, copyrightValue;
        private DateTime timeValue = DateTime.UtcNow;
        private TimeSpan timeShiftValue;

        [Category("Set metadata"), Description("Gets or sets if Title should be reset to specified value.")]
        [DefaultValue(false)]
        public bool TitleSet {
            get { return this.titleSet; }
            set { this.titleSet = value; }
        }

        [Category("Set metadata"), Description("Gets or sets if Description should be reset to specified value.")]
        [DefaultValue(false)]
        public bool DescriptionSet {
            get { return this.descriptionSet; }
            set { this.descriptionSet = value; }
        }

        [Category("Set metadata"), Description("Gets or sets if UserComment should be set to specified value.")]
        [DefaultValue(false)]
        public bool UserCommentSet {
            get { return this.userCommentSet; }
            set { this.userCommentSet = value; }
        }

        [Category("Set metadata"), Description("Gets or sets if Author should be set to specified value.")]
        [DefaultValue(false)]
        public bool AuthorSet {
            get { return this.authorSet; }
            set { this.authorSet = value; }
        }

        [Category("Set metadata"), Description("Gets or sets if Copyright should be set to specified value.")]
        [DefaultValue(false)]
        public bool CopyrightSet {
            get { return this.copyrightSet; }
            set { this.copyrightSet = value; }
        }

        [Category("Set metadata"), Description("Gets or sets value of Title Exif property.")]
        public string TitleValue {
            get { return this.titleValue; }
            set { this.titleValue = value; }
        }

        [Category("Set metadata"), Description("Gets or sets value of Description Exif property.")]
        public string DescriptionValue {
            get { return this.descriptionValue; }
            set { this.descriptionValue = value; }
        }

        [Category("Set metadata"), Description("Gets or sets value of UserComment Exif property.")]
        public string UserCommentValue {
            get { return this.userCommentValue; }
            set { this.userCommentValue = value; }
        }

        [Category("Set metadata"), Description("Gets or sets value of Author Exif property.")]
        public string AuthorValue {
            get { return this.authorValue; }
            set { this.authorValue = value; }
        }

        [Category("Set metadata"), Description("Gets or sets value of Copyright Exif property.")]
        public string CopyrightValue {
            get { return this.copyrightValue; }
            set { this.copyrightValue = value; }
        }

        [Category("Set metadata"), Description("Gets or sets relative time span.")]
        [DefaultValue("00:00:00")]
        public TimeSpan TimeShiftValue {
            get {
                return this.timeShiftValue;
            }
            set {
                this.timeShiftValue = value;
            }
        }

        [Category("Set metadata"), Description("Gets or sets if time should be shifted by specified value.")]
        [DefaultValue(false)]
        public bool TimeShiftSet {
            get {
                return this.timeShiftSet;
            }
            set {
                this.timeShiftSet = value;
            }
        }

        [Category("Set metadata"), Description("Gets or sets if time should be set to specified value.")]
        [DefaultValue(false)]
        public bool TimeSet {
            get {
                return this.timeSet;
            }
            set {
                this.timeSet = value;
            }
        }

        [Category("Set metadata"), Description("Gets or sets value of time-related Exif properties.")]
        [DefaultValue("1900-01-01")]
        public DateTime TimeValue {
            get {
                return this.timeValue;
            }
            set {
                this.timeValue = value;
            }
        }

        #endregion

        #region Remove metadata

        private bool removeThumbnail, removeMakerNote, removeOtherMetadata, removeDateTime;

        [DefaultValue(false)]
        public bool RemoveDateTime {
            get {
                return this.removeDateTime;
            }
            set {
                this.removeDateTime = value;
            }
        }

        [DefaultValue(false)]
        public bool RemoveThumbnail {
            get {
                return this.removeThumbnail;
            }
            set {
                this.removeThumbnail = value;
            }
        }

        [DefaultValue(false)]
        public bool RemoveMakerNote {
            get {
                return this.removeMakerNote;
            }
            set {
                this.removeMakerNote = value;
            }
        }

        [DefaultValue(false)]
        public bool RemoveOtherMetadata {
            get {
                return this.removeOtherMetadata;
            }
            set {
                this.removeOtherMetadata = value;
            }
        }

        #endregion

        #region Image processing

        private bool autoRotate = true, resize = false, recompress = false;
        private int recompressQuality = 60;
        private Size resizeValue = new Size(1024, 768);
        private BaseWatermark watermarkOptions = new TextWatermark();
        private ThumbnailOptions thumbnailsOptions = new ThumbnailOptions();
        private bool autoRename = false;

        [Category("Image processing"), Description("Gets or sets if image should be saved under new name, autogenerated by time taken.")]
        [DefaultValue(false)]
        public bool AutoRename {
            get { return autoRename; }
            set { autoRename = value; }
        }

        [Category("Image processing"), Description("Gets or sets if image should be rotated according to Exif Orientation property. Rotation is lossless when possible.")]
        [DefaultValue(true)]
        public bool AutoRotate {
            get { return this.autoRotate; }
            set { this.autoRotate = value; }
        }

        [Category("Image processing"), Description("Gets or sets if image should be resized.")]
        [DefaultValue(false)]
        public bool Resize {
            get { return this.resize; }
            set { this.resize = value; }
        }

        [Category("Image processing"), Description("Gets or sets if image should be JPEG recompressed.")]
        [DefaultValue(false)]
        public bool Recompress {
            get { return this.recompress; }
            set { this.recompress = value; }
        }

        [Category("Image processing"), Description("Gets or sets JPEG quality for recompression (0-100).")]
        [DefaultValue(60)]
        public int RecompressQuality {
            get { return this.recompressQuality; }
            set {
                if (value < 0 || value > 100) throw new ArgumentOutOfRangeException();
                this.recompressQuality = value;
            }
        }

        [Category("Image processing"), Description("")]
        [DefaultValue("1024, 768")]
        public System.Drawing.Size ResizeValue {
            get { return this.resizeValue; }
            set { this.resizeValue = value; }
        }

        [Category("Image processing"), Description("Gets or sets thumbnail creation settings.")]
        [Browsable(false)]
        public ThumbnailOptions ThumbnailsOptions {
            get { return this.thumbnailsOptions; }
            set { this.thumbnailsOptions = value; }
        }

        [Category("Image processing"), Description("Gets or sets watermark creation settings.")]
        [Browsable(false)]
        public BaseWatermark WatermarkOptions {
            get { return this.watermarkOptions; }
            set { this.watermarkOptions = value; }
        }

        #endregion

        public void SaveToFile(string fileName) {
            if (fileName == null) throw new ArgumentNullException("fileName");
            IFormatter bf = new BinaryFormatter();
            using (FileStream fs = File.Create(fileName))
            using (System.IO.Compression.GZipStream zs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress)) {
                bf.Serialize(zs, this);
                zs.Close();
                fs.Close();
            }
        }

        public static ProcessingOptions LoadFromFile(string fileName) {
            if (fileName == null) throw new ArgumentNullException("fileName");
            IFormatter bf = new BinaryFormatter();
            using (FileStream fs = File.OpenRead(fileName))
            using (System.IO.Compression.GZipStream zs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress)) {
                return (ProcessingOptions)bf.Deserialize(zs);
            }
        }

    }
}
