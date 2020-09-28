using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Devmasters.Imaging {
    public static class Utils {

        public static Size GetFitSize(Size original, Size fitTo) {
            float ratio = Math.Min((float)fitTo.Width / (float)original.Width, (float)fitTo.Height / (float)original.Height);
            fitTo.Width = (int)(original.Width * ratio);
            fitTo.Height = (int)(original.Height * ratio);
            return fitTo;
        }

        public static void CopyProperties(Image source, Image target) {
            foreach (int pid in source.PropertyIdList) {
                if (Array.IndexOf<int>(target.PropertyIdList, pid) == -1) {
                    target.SetPropertyItem(source.GetPropertyItem(pid));
                }
            }
        }

        public static ImageCodecInfo GetEncoderInfo(string MimeType)
        {
            int i;
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (i = 0; (i <= (encoders.Length - 1)); i++)
            {
                if ((encoders[i].MimeType == MimeType))
                    return encoders[i];
            }
            return null;
        }

        public static System.Drawing.Size GetImageSize(string filename)
        {
            System.Drawing.Size functionReturnValue = new System.Drawing.Size();

            // First we try to get the image size via our own jpeg parser
            // if that fails we we fall back to using .net stuff
            //
            try
            {
                functionReturnValue = GetJpegSize(filename);
            }
            catch
            {
                System.Drawing.Image image;

                image = System.Drawing.Image.FromFile(filename);
                functionReturnValue = image.Size;

                image.Dispose();
                image = null;
            }

            if (functionReturnValue.Width == 0)
            {
                throw new Exception("ImageSize Width = 0");
            }

            return functionReturnValue;
        }

        private static int GetBigEndianShort(BinaryReader reader)
        {

            byte highByte;
            byte lowByte;

            highByte = reader.ReadByte();
            lowByte = reader.ReadByte();

            return ((highByte * 256) + lowByte);

        }



        private static System.Drawing.Size GetJpegSize(string filename)
        {
            System.Drawing.Size functionReturnValue = new System.Drawing.Size();
            Stream stream;
            BinaryReader reader;

            stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            reader = new BinaryReader(stream);

            // Check header
            //
            if (reader.ReadByte() != 255 || reader.ReadByte() != 216)
            {
                reader.Close();
                reader = null;

                stream.Close();
                stream = null;

                throw new Exception("Header Invalid");
            }

            // Loop over the sections
            //
            bool foundSizeInfo = false;
            bool doneProcessing = false;

            while (!doneProcessing)
            {
                byte markerByte;
                byte markerNumber;
                int sectionLength;

                do
                {
                    markerByte = reader.ReadByte();
                }
                while ((markerByte != 255));

                markerNumber = reader.ReadByte();
                switch (markerNumber)
                {
                    case 218:
                    case 217:
                        // Start of Stream (image data follows)
                        // EOI (no image in this file)
                        doneProcessing = true;
                        break;

                    case 192:
                    case 193:
                    case 194:
                    case 195:
                    case 197:
                    case 198:
                    case 199:
                    case 201:
                    case 202:
                    case 203:
                    case 205:
                    case 206:
                    case 207:
                        // C4, C8, CC are notvalid
                        //
                        reader.ReadUInt16();
                        reader.ReadByte();
                        functionReturnValue.Height = GetBigEndianShort(reader);
                        functionReturnValue.Width = GetBigEndianShort(reader);
                        foundSizeInfo = true;
                        doneProcessing = true;
                        break;

                    case 224: // TODO: to 236
                        sectionLength = GetBigEndianShort(reader) - 2;
                        reader.BaseStream.Seek(sectionLength, SeekOrigin.Current);
                        break;

                    default:
                        sectionLength = GetBigEndianShort(reader) - 2;
                        reader.BaseStream.Seek(sectionLength, SeekOrigin.Current);
                        break;

                }

            }

            reader.Close();
            reader = null;

            stream.Close();
            stream = null;

            if (!foundSizeInfo)
            {
                throw new Exception("Size Not found");
            }
            return functionReturnValue;

        }

    }
}
