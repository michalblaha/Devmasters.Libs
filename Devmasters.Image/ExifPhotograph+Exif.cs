using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Devmasters.Imaging
{
	public partial class ExifPhotograph
	{
		// Charset signatures for Exif UserComment tag
		private static readonly byte[] CharCodeSigASCII = { 0x41, 0x53, 0x43, 0x49, 0x49, 0x00, 0x00, 0x00 };
		private static readonly byte[] CharCodeSigJIS = { 0x4A, 0x49, 0x53, 0x00, 0x00, 0x00, 0x00, 0x00 };
		private static readonly byte[] CharCodeSigUnicode = { 0x55, 0x4E, 0x49, 0x43, 0x4F, 0x44, 0x45, 0x00 };
		private static readonly byte[] CharCodeSigUndefined = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

		// Property existence check

		public bool IsPropertyDefined(ExifTagName propertyId)
		{
			return IsPropertyDefined((int)propertyId);
		}

		public bool IsPropertyDefined(int propertyId)
		{
			if (this.disposed) throw new ObjectDisposedException("ExifMetadata");
			if (this.image != null && this.image.Image != null && this.image.Image.PropertyIdList != null)
				return Array.IndexOf<int>(this.image.Image.PropertyIdList, propertyId) != -1;
			else
				return false;
		}

		// High-level property readers

		public int GetPropertyInt(ExifTagName propertyId, int defaultValue)
		{
			return GetPropertyInt((int)propertyId, defaultValue);
		}

		public int GetPropertyInt(int propertyId, int defaultValue)
		{
			if (this.IsPropertyDefined(propertyId))
			{
				return ParseIntValue(this.image.Image.GetPropertyItem(propertyId).Value);
			}
			else
			{
				return defaultValue;
			}
		}

		public short GetPropertyShort(ExifTagName propertyId, short defaultValue)
		{
			return GetPropertyShort((int)propertyId, defaultValue);
		}

		public short GetPropertyShort(int propertyId, short defaultValue)
		{
			if (this.IsPropertyDefined(propertyId))
			{
				try
				{
					return ParseShortValue(this.image.Image.GetPropertyItem(propertyId).Value);

				}
				catch
				{
					return defaultValue;
				}
			}
			else
			{
				return defaultValue;
			}
		}

		public string GetPropertyStringASCII(ExifTagName propertyId, string defaultValue)
		{
			return GetPropertyStringASCII((int)propertyId, defaultValue);
		}

		public string GetPropertyStringASCII(int propertyId, string defaultValue)
		{
			if (this.IsPropertyDefined(propertyId))
			{
				return ParseStringValue(this.image.Image.GetPropertyItem(propertyId).Value);
			}
			else
			{
				return defaultValue;
			}
		}

		public string GetPropertyStringExif(ExifTagName propertyId, string defaultValue)
		{
			return GetPropertyStringExif((int)propertyId, defaultValue);
		}

		public string GetPropertyStringExif(int propertyId, string defaultValue)
		{
			if (this.IsPropertyDefined(propertyId))
			{
				PropertyItem pi = this.image.Image.GetPropertyItem(propertyId);
				if (pi.Type == (short)ExifDataType.String)
				{
					return ParseStringValue(pi.Value);
				}
				else
				{
					return ParseExifStringValue(this.image.Image.GetPropertyItem((int)propertyId).Value);
				}
			}
			else
			{
				return defaultValue;
			}
		}

		public string GetPropertyStringUnicode(ExifTagName propertyId, string defaultValue)
		{
			return GetPropertyStringUnicode((int)propertyId, defaultValue);
		}

		public string GetPropertyStringUnicode(int propertyId, string defaultValue)
		{
			if (!IsPropertyDefined(propertyId)) return defaultValue;
			return Encoding.Unicode.GetString(this.image.Image.GetPropertyItem(propertyId).Value);
		}

		public byte[] GetProperty(ExifTagName propertyId, byte[] defaultValue)
		{
			return GetProperty((int)propertyId, defaultValue);
		}

		public byte[] GetProperty(int propertyId, byte[] defaultValue)
		{
			if (this.disposed) throw new ObjectDisposedException("ExifMetadata");
			if (this.IsPropertyDefined(propertyId))
			{
				return this.image.Image.GetPropertyItem(propertyId).Value;
			}
			else
			{
				return defaultValue;
			}
		}

		public string GetProperty(int propertyID)
		{
			if (this.disposed) throw new ObjectDisposedException("ExifMetadata");
			if (this.IsPropertyDefined(propertyID))
			{
				PropertyItem prop = this.image.Image.GetPropertyItem(propertyID);
				if (prop.Type == 0x1)
				{
					//1 = BYTE An 8-bit unsigned integer.,
					return prop.Value[0].ToString();
				}
				else if (prop.Type == 0x2)
				{
					//2 = ASCII An 8-bit byte containing one 7-bit ASCII code. The final byte is terminated with NULL.,
					return GetPropertyStringASCII(propertyID,"");
				}
				else if (prop.Type == 0x3)
				{
					//3 = SHORT A 16-bit (2 -byte) unsigned integer,
					return GetPropertyShort(propertyID, 0).ToString();

				}
				else if (prop.Type == 0x4)
				{
					//4 = LONG A 32-bit (4 -byte) unsigned integer,
					return GetPropertyInt(propertyID, 0).ToString();
				}
				else if (prop.Type == 0x5)
				{
					//5 = RATIONAL Two LONGs. The first LONG is the numerator and the second LONG expresses the//denominator.,
					return GetPropertyRational(propertyID).ToString(".");
					
				}
				else if (prop.Type == 0x7)
				{
					//7 = UNDEFINED An 8-bit byte that can take any value depending on the field definition,
					return prop.Value[0].ToString();
				}
				else if (prop.Type == 0x9)
				{
					//9 = SLONG A 32-bit (4 -byte) signed integer (2's complement notation),
					return convertToInt32(prop.Value).ToString();
				}
				else if (prop.Type == 0xA)
				{
					//10 = SRATIONAL Two SLONGs. The first SLONG is the numerator and the second SLONG is the
					//denominator.

					// rational
					byte[] n = new byte[prop.Len / 2];
					byte[] d = new byte[prop.Len / 2];
					Array.Copy(prop.Value, 0, n, 0, prop.Len / 2);
					Array.Copy(prop.Value, prop.Len / 2, d, 0, prop.Len / 2);
					int a = convertToInt32(n);
					int b = convertToInt32(d);
					ExifRational r = new ExifRational(a, b);
					r.ToString(".");
				}
			}
			return string.Empty;

		}

		private int convertToInt32(byte[] arr)
		{
			if (arr.Length != 4)
				return 0;
			else
				return arr[3] << 24 | arr[2] << 16 | arr[1] << 8 | arr[0];
		}

		public ExifRational GetPropertyRational(ExifTagName propertyId)
		{
			return GetPropertyRational((int)propertyId);
		}

		public ExifRational GetPropertyRational(int propertyId)
		{
			if (this.IsPropertyDefined(propertyId))
			{
				return ParseRationalValue(this.image.Image.GetPropertyItem(propertyId).Value);
			}
			else
			{
				ExifRational rat;
				rat.Numerator = 0;
				rat.Denominator = 1;
				return rat;
			}
		}

		public DateTime GetPropertyDateTime(ExifTagName propertyId, DateTime defaultValue)
		{
			return GetPropertyDateTime((int)propertyId, defaultValue);
		}

		public DateTime GetPropertyDateTime(int propertyId, DateTime defaultValue)
		{
			string s = GetPropertyStringASCII(propertyId, "");
			if (string.IsNullOrEmpty(s)) return DateTime.MinValue;
			DateTime dt;
			if (!DateTime.TryParseExact(s, @"yyyy\:MM\:dd HH\:mm\:ss", null, System.Globalization.DateTimeStyles.None, out dt)) return DateTime.MinValue;
			return dt;
		}

		// High-level property writers

		public void SetPropertyStringASCII(ExifTagName propertyId, string value)
		{
			SetPropertyStringASCII((int)propertyId, value);
		}

		public void SetPropertyStringASCII(int propertyId, string value)
		{
			if (this.asciiValuesEncoding.GetType() == typeof(System.Text.ASCIIEncoding))
			{
				// Remove diacritic marks for ASCII encoding
				value = value.Normalize(NormalizationForm.FormD);
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < value.Length; i++)
				{
					if (CharUnicodeInfo.GetUnicodeCategory(value[i]) != UnicodeCategory.NonSpacingMark) sb.Append(value[i]);
				}
				value = sb.ToString();
			}

			byte[] data = this.asciiValuesEncoding.GetBytes(value + '\0');
			SetProperty(propertyId, data, ExifDataType.String);
		}

		public void SetPropertyStringExif(ExifTagName propertyId, string value)
		{
			SetPropertyStringExif((int)propertyId, value);
		}

		public void SetPropertyStringExif(int propertyId, string value)
		{
			// Generate Unicode prefix
			byte[] data = new byte[System.Text.Encoding.Unicode.GetByteCount(value) + 8];
			Array.Copy(CharCodeSigUnicode, data, 8);

			// Generate rest of data
			System.Text.Encoding.Unicode.GetBytes(value, 0, value.Length, data, 8);
			SetProperty(propertyId, data, ExifDataType.ByteArray);
		}

		public void SetPropertyStringUnicode(ExifTagName propertyId, string value)
		{
			SetPropertyStringUnicode((int)propertyId, value);
		}

		public void SetPropertyStringUnicode(int propertyId, string value)
		{
			SetProperty(propertyId, Encoding.Unicode.GetBytes(value), ExifDataType.ByteArray);
		}

		public void SetPropertyDateTime(ExifTagName propertyId, DateTime value)
		{
			SetPropertyDateTime((int)propertyId, value);
		}

		public void SetPropertyDateTime(int propertyId, DateTime value)
		{
			if (value == DateTime.MinValue || value == DateTime.MaxValue)
			{
				SetPropertyStringASCII(propertyId, "    :  :     :  :  "); // date unspecified
			}
			else
			{
				SetPropertyStringASCII(propertyId, value.ToString(@"yyyy\:MM\:dd HH\:mm\:ss"));
			}
		}

		public void SetPropertyShort(ExifTagName propertyId, short value)
		{
			SetPropertyShort((int)propertyId, value);
		}

		public void SetPropertyShort(int propertyId, short value)
		{
			byte[] data = new byte[2];
			data[0] = ((byte)(value & 0xFF));
			data[1] = ((byte)((value & 0xFF00) >> 8));
			SetProperty(propertyId, data, ExifDataType.UnsignedShort);
		}

		public void SetPropertyInt(ExifTagName propertyId, int value)
		{
			SetPropertyInt((int)propertyId, value);
		}

		public void SetPropertyInt(int propertyId, int value)
		{
			byte[] data = new byte[4];
			for (int i = 0; i <= 3; i++)
			{
				data[i] = ((byte)(value & 0xFF));
				value >>= 8;
			}
			SetProperty(propertyId, data, ExifDataType.UnsignedLong);
		}

		public void SetProperty(ExifTagName propertyId, byte[] data, ExifDataType exifType)
		{
			SetProperty((int)propertyId, data, exifType);
		}

		public void SetProperty(int propertyId, byte[] data, ExifDataType exifType)
		{
			if (this.disposed) throw new ObjectDisposedException("ExifMetadata");

			System.Drawing.Imaging.PropertyItem prop;
			if (IsPropertyDefined(propertyId))
			{
				prop = this.image.Image.GetPropertyItem((int)propertyId);
			}
			else
			{
				prop = (System.Drawing.Imaging.PropertyItem)Activator.CreateInstance(typeof(System.Drawing.Imaging.PropertyItem), true);
				prop.Id = (int)propertyId;
			}
			prop.Value = data;
			prop.Type = (short)exifType;
			prop.Len = data.Length;
			this.image.Image.SetPropertyItem(prop);
			this.changed = true;
			if (this.autoSave) this.Save();
		}

		// Low-level property parsers

		private int ParseIntValue(byte[] data)
		{
			if (data == null) throw new ArgumentNullException("data");
			if (data.Length != 4) throw new ArgumentException("Data length error (4 bytes expected)", "data");
			return data[3] << 24 | data[2] << 16 | data[1] << 8 | data[0];
		}

		private short ParseShortValue(byte[] data)
		{
			if (data == null) throw new ArgumentNullException("data");
			if (data.Length != 2) throw new ArgumentException("Data length error (2 bytes expected)", "data");
			return (short)(data[1] << 8 | data[0]);
		}

		private string ParseStringValue(byte[] data)
		{
			return ParseStringValue(data, this.asciiValuesEncoding);
		}

		private string ParseStringValue(byte[] data, Encoding encoding)
		{
			if (data == null) throw new ArgumentNullException("data");
			string s = encoding.GetString(data);
			if (s.EndsWith("\0")) s = s.Substring(0, s.Length - 1);
			return s;
		}

		private string ParseExifStringValue(byte[] data)
		{
			if (data == null)
				return string.Empty; //throw new ArgumentNullException("data");
			if (data.Length < 8)
				return string.Empty; //throw new ArgumentException("Insufficient length of data.", "data");
			if (data.Length == 8) return string.Empty;          // Only signature is present

			// Analyze bytes for character encoding
			System.Text.Encoding enc;
			if (ByteArrayStartsWith(data, CharCodeSigASCII)) enc = System.Text.Encoding.ASCII;
			else if (ByteArrayStartsWith(data, CharCodeSigJIS)) enc = System.Text.Encoding.GetEncoding("shift_jis");
			else if (ByteArrayStartsWith(data, CharCodeSigUnicode)) enc = System.Text.Encoding.Unicode;
			else if (ByteArrayStartsWith(data, CharCodeSigUndefined)) enc = System.Text.Encoding.Default;
			else throw new ArgumentException("Unknown charset signature.", "data");

			string strData =  enc.GetString(data, 8, data.Length - 8);     // Return data in given encoding
            strData = strData.Replace("\0","");
            return strData;
		}

		private ExifRational ParseRationalValue(byte[] data)
		{
			byte[] nom = new byte[4], den = new byte[4];
			Array.Copy(data, 0, nom, 0, 4);
			Array.Copy(data, 4, den, 0, 4);

			ExifRational rat = new ExifRational();
			rat.Denominator = this.ParseIntValue(den);
			rat.Numerator = this.ParseIntValue(nom);
			return rat;
		}

	}
}