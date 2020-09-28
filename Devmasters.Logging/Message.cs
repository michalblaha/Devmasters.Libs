using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devmasters.Logging

/* Unmerged change from project 'Devmasters.Logging (net472)'
Before:
{

    
    public class LogMessage : Dictionary<string, object>
After:
{


    public class LogMessage : Dictionary<string, object>
*/
{


    public class LogMessage : Dictionary<string, object>
    {

        private const string CACHE_AllAssemblyVersions = "AllAssemblyVersions";
        object lockObj = new object();



        public LogMessage()
            : base()
        {
            this.SetLevel(PriorityLevel.Debug);
        }

        public void AddIfAbsent(string key, object value)
        {
            if (!ContainsKey(key))
            {
                AddSync(key, value);
            }
        }

        public void Set(string key, object value)
        {
            if (ContainsKey(key))
            {
                this[key] = value;
            }
            else
                AddSync(key, value);
        }

        public void Add(string key, string value)
        {
            base.Add(key, value);
        }

        public void AddFormat(string key, string valueFormat, params object[] valueArgs)
        {
            var value = string.Format(valueFormat, valueArgs);
            Add(key, value);
        }


        private void AddSync(string key, object val)
        {
            lock (lockObj)
            {
                this.Add(key, val);
            }
        }
        private void AddSync(string key, string val)
        {
            lock (lockObj)
            {
                this.Add(key, val);
            }
        }


        public PriorityLevel Level { get { return (PriorityLevel)getByKey("level"); } }
        public LogMessage SetLevel(PriorityLevel level)
        {
            this.Set("level", level);
            return this;
        }

        public string Message { get { return getStringByKey("message"); } }
        public LogMessage SetMessage(string txt)
        {
            if (!string.IsNullOrEmpty(txt))
                this.Set("message", txt);
            return this;
        }
        public LogMessage SetContext(object obj)
        {
            if (obj != null)
                this.Set("context", obj);
            return this;
        }
        public LogMessage SetStack(string formatedStack)
        {
            if (formatedStack != null)
                this.Set("stack", formatedStack);
            return this;
        }

        public Exception Exception { get { return (Exception)getByKey("exception"); } }
        public LogMessage SetException(Exception ex)
        {
            if (ex != null)
                this.Set("exception", ex);
            return this;
        }

        public LogMessage SetUsername(string username)
        {
            this.Set("username", username);
            return this;
        }

        public LogMessage SetVersion(Version version)
        {
            this.AddSync("version", version.ToString());
            return this;
        }

        public LogMessage SetVersionOfAllAssemblies(string[] allowedNamespaces = null)
        {
            if (allowedNamespaces == null)
                this.AddSync("assemblies", AllVersions());
            else
                this.AddSync("assemblies", AllVersions(allowedNamespaces));
            return this;
        }

        public LogMessage SetCustomKeyValue(string key, object value)
        {
            if (!string.IsNullOrEmpty(key) && value != null)
                this.AddSync(key, value);
            return this;
        }

        public LogMessage SetCustomKeyValues(Dictionary<string, object> values)
        {
            if (values != null)
                foreach (var keyvalue in values)
                    this.AddSync(keyvalue.Key, keyvalue.Value);
            return this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(512);
            lock (lockObj)
            {
                foreach (var kv in this.ToList())
                {
                    sb.Append(kv.Key + ": " + kv.Value + "; ");
                }
                sb.AppendLine();
                return sb.ToString();
            }
        }

        private string getStringByKey(string key)
        {
            object val = getByKey(key);
            if (val != null)
                return val.ToString();
            else
                return string.Empty;
        }

        private object getByKey(string key)
        {
            if (this.ContainsKey(key))
                return this[key];
            else
                return null;
        }


        public static LogMessage FromDictionary(IDictionary<object, object> source)
        {
            var msg = new LogMessage();
            var asDictionary = source.ToDictionary(k => k.Key != null ? k.Key.ToString() : string.Empty, v => v.Value);
            asDictionary.ToList().ForEach(x => msg.AddSync(x.Key, x.Value));
            return msg;
        }


        static string[] devmastersNamespaces = new string[] { "devmasters." };
        public static string AllVersions()
        {
            return AllVersions(new string[] { });
        }

        static string _ver = null;
        public static string AllVersions(string[] allowedNamespaces)
        {
            if (allowedNamespaces == null)
                allowedNamespaces = new string[] { };

            if (_ver == null)
            {
                string ver = "";
                System.Reflection.Assembly[] ass = AppDomain.CurrentDomain.GetAssemblies();
                foreach (System.Reflection.Assembly a in ass)
                {
                    string assemblyNameLow = a.ManifestModule.Name.ToLower();
                    if (allowedNamespaces.Count() == 0 || allowedNamespaces.Any(s => assemblyNameLow.StartsWith(s)))
                    {
                        ver += string.Format("{0}:{1}; ", a.ManifestModule.Name, a.GetName().Version.ToString());
                    }

                }
                _ver = ver;
            }
            return _ver;
        }




    }

}
