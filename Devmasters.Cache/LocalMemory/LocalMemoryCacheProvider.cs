using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace Devmasters.Cache.LocalMemory
{

    public class LocalMemoryCacheProvider<T> : ICacheProvider<T>
        where T : class
    {
        private System.Runtime.Caching.MemoryCache cache = MemoryCache.Default;
        object syncObj = new object();

        System.Runtime.Caching.CacheEntryUpdateCallback OnUpdateCacheCallback = null;

        public LocalMemoryCacheProvider(System.Runtime.Caching.CacheEntryUpdateCallback onUpdateCacheCallback)
        {
            this.OnUpdateCacheCallback = onUpdateCacheCallback;
        }
        public LocalMemoryCacheProvider()
            : this(null)
        {
        }

        #region ICacheProvider<T> Members


        public void Init()
        {
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        public void Insert(string key, T value, TimeSpan expiration)
        {
            if (expiration == TimeSpan.Zero)
                Insert(key, value, DateTime.UtcNow.AddYears(2));
            else
                Insert(key, value, DateTime.UtcNow.Add(expiration));
        }

        private void Insert(string key, T value, DateTime datetime)
        {

            if (value != null)
            {
                if (this.OnUpdateCacheCallback != null)
                {
                    var cp = new CacheItemPolicy();
                    cp.AbsoluteExpiration = datetime;
                    cp.UpdateCallback = this.OnUpdateCacheCallback;
                    cache.Set(new CacheItem(key, value), cp);
                }
                else
                    cache.Set(new CacheItem(key, value), new CacheItemPolicy() { AbsoluteExpiration = datetime });
            }
            else
                BaseCache<T>.Logger.Warning(new Logging.LogMessage()
                    .SetMessage("LocalMemoryCacheProvider> null value")
                    .SetLevel(Logging.PriorityLevel.Warning)
                    .SetCustomKeyValue("objectType",typeof(T).ToString())
                    .SetCustomKeyValue("cache key", key)
                    );
        }

        public bool Exists(string key)
        {
            return (cache.Get(key) != null);
        }

        public T Get(string key)
        {
            return cache.Get(key) as T;
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
