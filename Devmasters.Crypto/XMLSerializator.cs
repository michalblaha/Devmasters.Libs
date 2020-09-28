namespace Devmasters
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml.Serialization;

    [Serializable]
    public abstract class XMLSerializator
    {
        public static object FromXml(string xml, Type type)
        {
            object objectValue = null;
            if (xml == null)
                return null;

            if (xml.Length == 0)
            {
                return null;
            }
            StringReader textReader = new StringReader(xml);
            if (xml.Trim().Length == 0)
            {
                return null;
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(type);
                objectValue = RuntimeHelpers.GetObjectValue(serializer.Deserialize(textReader));
            }
            catch (Exception exception1)
            {
                Exception innerException = exception1;
                Logging.Logger.Root.Error("type:" + type.ToString() + "\nXml:" + xml, exception1);
                throw new ApplicationException("Deserialization problem", innerException);
            }
            return objectValue;
        }

        public static string GetClassXMLRootAttribute(Type type)
        {
            XmlRootAttribute customAttribute = (XmlRootAttribute)Attribute.GetCustomAttribute(type, typeof(XmlRootAttribute));
            if (customAttribute == null)
            {
                return "";
            }
            return customAttribute.ElementName;
        }

        public virtual string ToXml()
        {
            return this.ToXml(this.GetType());
        }

        protected virtual string ToXml(Type type)
        {
            return ToXml(this, type);
        }

        public static string ToXml(object Obj, Type type)
        {
            try
            {
                if (Obj == null)
                    return string.Empty;

                XmlSerializer serializer = new XmlSerializer(type);
                StringBuilder sb = new StringBuilder(0x400);
                StringWriter writer = new StringWriter(sb);
                serializer.Serialize((TextWriter)writer, RuntimeHelpers.GetObjectValue(Obj));
                writer.Close();
                sb.Replace("\0", "");
                return sb.ToString();

            }
            catch (Exception)
            {
                //Logger.Error("type:" + type.ToString(), ex);
                throw;
            }
        }
    }
}

