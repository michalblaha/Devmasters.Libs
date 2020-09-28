using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Devmasters.Imaging {

    [Serializable]
    public abstract class BaseWatermark {
        protected ContentAlignment position = ContentAlignment.BottomCenter;
        protected int margin = 0;
        protected bool enabled = false;

        public abstract Image Render(Image image);

        [Category("Position"), Description("Gets or sets number of pixels between image border and watermark background.")]
        [DefaultValue(0)]
        public int Margin {
            get { return margin; }
            set { margin = value; }
        }

        [Category("Position"), Description("Gets or sets position of watermark in image.")]
        [DefaultValue(ContentAlignment.BottomCenter)]
        public ContentAlignment Position {
            get { return position; }
            set { position = value; }
        }

        [Description("Gets or sets if watermark should be added to image.")]
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

        public static BaseWatermark LoadFromFile(string fileName) {
            if (fileName == null) throw new ArgumentNullException("fileName");
            IFormatter bf = new BinaryFormatter();
            using (FileStream fs = File.OpenRead(fileName))
            using (System.IO.Compression.GZipStream zs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress)) {
                return (BaseWatermark)bf.Deserialize(zs);
            }
        }

    }
}
