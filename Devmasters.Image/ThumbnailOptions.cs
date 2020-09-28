using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;

namespace Devmasters.Imaging {

    public enum ThumbnailFormat { Jpeg, Gif, Png }

    [Serializable]
    public class ThumbnailOptions {
        private bool enabled = false;
        private string fileNameTemplate = "tn_{0}";
        private ThumbnailFormat format = ThumbnailFormat.Jpeg;
        private Size size = new Size(120, 120);
        private Color backgroundColor = Color.Transparent;
        private bool autoRotate = true;

        [Description("Gets or sets if the image should be rotated based on EXIF orientation.")]
        [DefaultValue(true)]
        public bool AutoRotate {
            get { return autoRotate; }
            set { autoRotate = value; }
        }

        [Description("Gets or sets background color of thumbnail.")]
        public Color BackgroundColor {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        [Description("Gets or sets size of thumbnail.")]
        public Size Size {
            get { return size; }
            set {
                if (size.Width < 50 || size.Height < 50) throw new ArgumentOutOfRangeException("Thumbnail size must be at least 50x50px.");
                size = value;
            }
        }

        [Description("Gets or sets format of thumbnail image.")]
        [DefaultValue(ThumbnailFormat.Jpeg)]
        public ThumbnailFormat Format {
            get { return format; }
            set { format = value; }
        }

        [Browsable(false)]
        public System.Drawing.Imaging.ImageFormat ImageFormat {
            get {
                switch (this.Format) {
                    case ThumbnailFormat.Jpeg:
                        return System.Drawing.Imaging.ImageFormat.Jpeg;
                    case ThumbnailFormat.Gif:
                        return System.Drawing.Imaging.ImageFormat.Gif;
                    case ThumbnailFormat.Png:
                        return System.Drawing.Imaging.ImageFormat.Png;
                    default:
                        return null;
                }
            }
        }

        [Browsable(false)]
        public string FileExtension {
            get {
                switch (this.Format) {
                    case ThumbnailFormat.Jpeg:
                        return "jpg";
                    case ThumbnailFormat.Gif:
                        return "gif";
                    case ThumbnailFormat.Png:
                        return "png";
                    default:
                        return string.Empty;
                }
            }
        }

        [Description("Gets or sets thumbnail image file name format (without extension). Use \"{0}\" to specify original file name.")]
        [DefaultValue("tn_{0}")]
        public string FileNameTemplate {
            get { return fileNameTemplate; }
            set {
                if (value == null) throw new ArgumentNullException();
                value = value.Trim();
                if (value.IndexOf("{0}") == -1) throw new ArgumentException("Value must contain \"{0}\" placeholder.");
                if (value == "{0}") throw new ArgumentNullException("Name must be modified - cannot use \"{0}\" placeholder alone.");
                char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
                for (int i = 0; i < value.Length; i++) if (Array.IndexOf<char>(invalidChars, value[i]) != -1) throw new ArgumentException(string.Format("Character \"{0}\" is invalid in file name.", value[i]));
                
                fileNameTemplate = value;
            }
        }

        [Description("Gets or sets if thumbnail creation is enabled.")]
        [DefaultValue(false)]
        [Browsable(false)]
        public bool Enabled {
            get { return enabled; }
            set { enabled = value; }
        }

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

        public static ThumbnailOptions LoadFromFile(string fileName) {
            if (fileName == null) throw new ArgumentNullException("fileName");
            IFormatter bf = new BinaryFormatter();
            using (FileStream fs = File.OpenRead(fileName))
            using (System.IO.Compression.GZipStream zs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress)) {
                return (ThumbnailOptions)bf.Deserialize(zs);
            }
        }

    }

}
