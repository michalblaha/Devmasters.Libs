using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace Devmasters.Cache.File
{

    [Serializable]
    public class ExpirationData
    {
        public static TimeSpan UnlimitedExpiration = TimeSpan.FromDays(1000);

        public ExpirationData() { }
        public ExpirationData(TimeSpan expiration)
        {
            if (expiration == TimeSpan.Zero)
                this.ExpirationInTicks = UnlimitedExpiration.Ticks;
            else
                this.ExpirationInTicks = expiration.Ticks;
        }

        public virtual long CreatedInTicks { get; set; } = DateTime.UtcNow.Ticks;

        public virtual long ExpirationInTicks { get; set; } = 0;

        public DateTime Created() { return new DateTime(this.CreatedInTicks, DateTimeKind.Utc); }
        public DateTime ExpiresAt() { return Created().AddTicks(this.ExpirationInTicks); }
    }

    public class FileCacheProvider<T> : ICacheProvider<T>
        where T : class
    {
        public static Devmasters.Logging.Logger Logger = new Logging.Logger("Devmasters.Cache.V20.File");


        object syncObj = new object();

        private T internalValue = default(T);
        private DateTime internalValueCreated = DateTime.MinValue;

        string extContent = ".filecache.content";
        string extData = ".filecache.data";
        string path = null;
        public FileCacheProvider(string path)
        {
            if (string.IsNullOrEmpty(path))
                this.path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            else
                this.path = System.IO.Path.GetDirectoryName(path);

        }

        public FileCacheProvider()
           : this(null)
        {
        }

        #region ICacheProvider<T> Members


        public void Init()
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }

        public void Remove(string key)
        {
            Logger.Debug("Removing cache key " + key + " for type " + typeof(T).ToString());
            internalValue = default(T);
            internalValueCreated = DateTime.MinValue;
            var f1 = DeleteFile(GetFullPath(key, true));
            var f2 = DeleteFile(GetFullPath(key, false));
            Logger.Debug("Removed cache key " + key + " for type " + typeof(T).ToString() + " with success: f1:" + f1.ToString() + " & f2: " + f2.ToString());
        }

        private bool DeleteFile(string fullPath)
        {

            try
            {
                System.IO.File.Delete(fullPath);
                Logger.Debug("V20.FillCacheProvider file deleted: " + fullPath);
                return true;

            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(100);
                try
                {
                    System.IO.File.Delete(fullPath);
                    return true;

                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(200);

                    try
                    {
                        System.IO.File.Delete(fullPath);
                        return true;
                    }
                    catch (Exception)
                    {
                        Logger.Error("V20.FillCacheProvider file cannot be deleted: " + fullPath);
                        return false;
                    }
                }

            }
        }

        protected string nullString = "/*null*/";
        protected virtual byte[] Serialize<TObj>(TObj obj)
        {
            try
            {
                if (obj == null)
                    return Encoding.UTF8.GetBytes(nullString);
                return Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));

            }
            catch (Exception e)
            {
                Logger.Error("error serialization ", e);
                return null;
            }
        }
        protected virtual TObj Deserialize<TObj>(byte[] bobj)
        {
            string sobj = Encoding.UTF8.GetString(bobj);
            try
            {
                if (sobj == nullString)
                    return default(TObj);

                return Newtonsoft.Json.JsonConvert.DeserializeObject<TObj>(sobj);

            }
            catch (Exception e)
            {
                Logger.Error("V20.FillCacheProvider deserialize error. " + typeof(TObj).ToString() + " : " + e);
                return default(TObj);
            }
        }

        public void Insert(string key, T value, TimeSpan expiration)
        {

            try
            {

                var expireData = new ExpirationData(expiration);

                lock (syncObj)
                {
                    //Remove(key);
                    Logger.Debug("V20.FillCacheProvider saving into " + GetFullPath(key, true));
                    System.IO.File.WriteAllBytes(GetFullPath(key, true), Serialize<T>(value));
                    Logger.Debug("V20.FillCacheProvider saving into " + GetFullPath(key, false));
                    System.IO.File.WriteAllBytes(GetFullPath(key, false), Serialize<ExpirationData>(expireData));

                    internalValue = value;
                    internalValueCreated = expireData.Created();
                }
            }
            catch (Exception e)
            {
                Logger.Error("V20.FillCacheProvider error saving into " + GetFullPath(key, true), e);
            }

        }



        public bool Exists(string key)
        {
            if (!System.IO.File.Exists(GetFullPath(key, false)))
            {
                System.Threading.Thread.Sleep(200);
                if (!System.IO.File.Exists(GetFullPath(key, false)))
                {
                    Logger.Debug($"Exist func: no {GetFullPath(key, false)}");
                    return false;
                }
            }
            if (!System.IO.File.Exists(GetFullPath(key, true)))
            {
                Logger.Debug($"Exist func: no {GetFullPath(key, true)}");
                return false;
            }



            try
            {
                ExpirationData expireData = Deserialize<ExpirationData>(System.IO.File.ReadAllBytes(GetFullPath(key, false)));
                if (expireData == default(ExpirationData))
                {
                    Logger.Debug($"Exist func: no expiration data: " + Newtonsoft.Json.JsonConvert.SerializeObject(expireData));
                    return false;
                }

                if (expireData.ExpirationInTicks == ExpirationData.UnlimitedExpiration.Ticks)
                    return true;

                bool stillValid = expireData.ExpiresAt() > DateTime.UtcNow;
                if (!stillValid)
                {
                    Logger.Debug($"Exist func: expired data: " + Newtonsoft.Json.JsonConvert.SerializeObject(expireData));
                    return false;
                }
                if (expireData.Created() != internalValueCreated)
                    internalValue = default(T);
                return stillValid;
            }
            catch (Exception e)
            {
                Logger.Debug("V20.FillCacheProvider Exists error for key " + key, e);
                //Remove(key);
                return false;
            }

        }

        private string GetFullPath(string key, bool content)
        {
            if (content)
                return System.IO.Path.Combine(this.path, key + extContent);

            else
                return System.IO.Path.Combine(this.path, key + extData);

        }


        public T Get(string key)
        {
            if (internalValue == default(T))
            {
                internalValue = Deserialize<T>(System.IO.File.ReadAllBytes(GetFullPath(key, true)));
                ExpirationData expireData = Deserialize<ExpirationData>(System.IO.File.ReadAllBytes(GetFullPath(key, false)));
                internalValueCreated = expireData.Created();
            }
            return internalValue;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }


                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LocalMemoryCacheProvider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #endregion


    }
}
