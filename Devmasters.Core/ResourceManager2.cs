using System;

namespace Devmasters
{
    [SerializableAttribute]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ResourceManager2
    {

        private System.Globalization.CultureInfo culture = null;
        private System.Resources.ResourceManager rm = null;

        public ResourceManager2(Type resourcesType) : this(resourcesType, System.Threading.Thread.CurrentThread.CurrentUICulture) { }
        public ResourceManager2(string baseName, System.Reflection.Assembly assembly) : this(baseName, assembly, System.Threading.Thread.CurrentThread.CurrentUICulture) { }
        public ResourceManager2(string baseName, System.Reflection.Assembly assembly, Type usingResourceSet) : this(baseName, assembly, usingResourceSet, System.Threading.Thread.CurrentThread.CurrentUICulture) { }

        public ResourceManager2(Type resourcesType, System.Globalization.CultureInfo culture)
        {
            this.culture = culture;
            this.rm = new System.Resources.ResourceManager(resourcesType);
        }

        public ResourceManager2(string baseName, System.Reflection.Assembly assembly, System.Globalization.CultureInfo culture)
        {
            this.culture = culture;
            this.rm = new System.Resources.ResourceManager(baseName, assembly);
        }

        public ResourceManager2(string baseName, System.Reflection.Assembly assembly, Type usingResourceSet, System.Globalization.CultureInfo culture)
        {
            this.culture = culture;
            this.rm = new System.Resources.ResourceManager(baseName, assembly, usingResourceSet);
        }

        public System.Resources.ResourceManager ResourceManager
        {
            get { return rm; }
        }

        public string GetString(string key)
        {
            return rm.GetString(key, this.culture);
        }
        public string GetString(string key, System.Globalization.CultureInfo culture)
        {
            return rm.GetString(key, culture);
        }
        public object GetObject(string key)
        {
            return rm.GetObject(key, this.culture);
        }
        public object GetObject(string key, System.Globalization.CultureInfo culture)
        {
            return rm.GetObject(key, culture);
        }

    }
}
