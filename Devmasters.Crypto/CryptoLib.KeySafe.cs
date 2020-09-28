using System;

using Devmasters;

namespace Devmasters.Crypto
{

    [Serializable()]
    public class KeySafe
    {
        public string Key = string.Empty;
        public string Vector = string.Empty;
        public string Algorithm = string.Empty;

        public string ToXML()
        {
            return XMLSerializator.ToXml(this, typeof(KeySafe));

        }

        public static KeySafe FromXML(string xml)
        {
            try
            {
                KeySafe k = XMLSerializator.FromXml(xml, typeof(KeySafe)) as KeySafe;
                return k;
            }
            catch (Exception e)
            {
                Logging.Logger.Root.Error("", e);
                return null;
            }
        }

        public KeySafe() { }

        public KeySafe(string key, string vector, string algorithm)
        {
            this.Key = key;
            this.Vector = vector;
            this.Algorithm = algorithm;
        }
    }
}
