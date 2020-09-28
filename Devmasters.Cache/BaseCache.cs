using System;

namespace Devmasters.Cache
{
    public abstract class BaseCache<TResult> : IDisposable
        where TResult : class
    {
        public static Devmasters.Logging.Logger Logger = new Logging.Logger("Devmasters.Cache");

        protected struct ContentProducerInfo<TResult>
        {
            public System.Func<object, TResult> Function;
            public object Parameters;
            public TimeSpan ExpiresInterval;
            public DateTime Created;
            public DateTime DataLoaded;
            public void Reset()
            {
                this.Function = null;
                this.Parameters = null;
                this.ExpiresInterval = TimeSpan.Zero;
                this.Created = DateTime.MinValue;
                this.DataLoaded = DateTime.MinValue;

            }
        }

        protected ICacheProvider<TResult> provider = null;
        protected ContentProducerInfo<TResult> contentInfo = default(ContentProducerInfo<TResult>);
        protected string cacheKey = string.Empty;

        protected object loadCacheContentFromProviderLockObj = new object();
        protected BaseCache(ICacheProvider<TResult> provider, TimeSpan expires, System.Func<object, TResult> contentFunc)
            : this(provider, expires, null, contentFunc, null)
        {
        }

        public DateTime Created { get { return this.contentInfo.Created; } }
        public DateTime DataLoaded { get { return this.contentInfo.DataLoaded; } }

        protected BaseCache(ICacheProvider<TResult> cacheProvider, TimeSpan expires, string cacheKey, System.Func<object, TResult> contentFunc, object parameters)
        {
            this.contentInfo.Function = contentFunc;
            this.contentInfo.Parameters = parameters;
            this.contentInfo.Created = DateTime.UtcNow;
            this.contentInfo.ExpiresInterval = expires;

            if (string.IsNullOrEmpty(cacheKey))
                cacheKey = Guid.NewGuid().ToString();
            this.cacheKey = cacheKey;

            this.provider = cacheProvider;
            Logger.Debug("initiating type:" + typeof(TResult).ToString() + " key:" + cacheKey);
            this.provider.Init();
        }


        protected virtual TResult GetDataFromContentFunction()
        {
            TResult data = default(TResult);
            if (this.contentInfo.Function == null)
            {
                Logger.Info("Starting GetDataFromContentFunction : No contentFunction for type " + typeof(TResult).ToString());
                return data;
            }
            try
            {
                Logger.Debug("Starting GetDataFromContentFunction for type " + typeof(TResult).ToString());
                data = this.contentInfo.Function(this.contentInfo.Parameters);
                this.contentInfo.DataLoaded = DateTime.Now;
                Logger.Debug("Finished GetDataFromContentFunction for type " + typeof(TResult).ToString());
                return data;
            }
            catch (Exception e)
            {
                Logger.Error("Get Content from ContentFunction call error", e);
                return data;
            }
        }

        public virtual TResult Get()
        {
            if (disposedValue)
                throw new ObjectDisposedException("BaseCache");

            lock (loadCacheContentFromProviderLockObj)
            {
                //if there is no content function, read data from cache
                if (this.contentInfo.Function == null)
                {

                    var data= this.provider.Get(this.cacheKey);
                    if (data == null)
                    {
                        Logger.Debug($"Get(): No contentFunction, no data in cache for key:{cacheKey} type:{typeof(TResult).ToString()}" );

                        return default(TResult);
                    }
                    return data;
                }
                if (this.provider.Exists(this.cacheKey))
                {
                    //Logger.Debug($"Get(): data exists in cache for key:{cacheKey} type:{typeof(TResult).ToString()}" );
                    return this.provider.Get(this.cacheKey);
                }
                else
                {
                    //check again if another thread inbetween filled the cache
                    System.Threading.Thread.Sleep(4);
                    if (this.provider.Exists(this.cacheKey))
                        return this.provider.Get(this.cacheKey);
                    else
                    {
                        Logger.Debug($"Get(): expired data in cache for key:{cacheKey} type:{typeof(TResult).ToString()}");
                        TResult data = GetDataFromContentFunction();
                        this.provider.Insert(this.cacheKey, data, this.contentInfo.ExpiresInterval);
                        return data;
                    }
                }
            }
        }
        public virtual bool ForceRefreshCache(TResult newContent = null)
        {
            if (disposedValue)
                throw new ObjectDisposedException("BaseCache");

            lock (loadCacheContentFromProviderLockObj)
            {
                TResult data = default(TResult);
                data = newContent;
                this.provider.Insert(this.cacheKey, data, this.contentInfo.ExpiresInterval);
                return true;
            }
        }

        public virtual bool ForceRefreshCache()
        {
            if (disposedValue)
                throw new ObjectDisposedException("BaseCache");

            lock (loadCacheContentFromProviderLockObj)
            {
                TResult data = default(TResult);
                data = GetDataFromContentFunction();
                this.provider.Insert(this.cacheKey, data, this.contentInfo.ExpiresInterval);
                return true;
            }
        }

        //public virtual bool Put(TResult content )
        //{
        //    if (disposedValue)
        //        throw new ObjectDisposedException("BaseCache");

        //    lock (loadCacheContentFromProviderLockObj)
        //    {
        //        //check again if another thread inbetween filled the cache
        //        TResult data = content;
        //        if (data != null || data != default(TResult))
        //        {
        //            this.provider.Insert(this.cacheKey, data, this.contentInfo.ExpiresInterval);
        //            return true;
        //        }
        //        else
        //            throw new ArgumentNullException("content");
        //    }
        //}

        public virtual void Invalidate()
        {
            if (disposedValue)
                throw new ObjectDisposedException("BaseCache");

            this.provider.Remove(this.cacheKey);
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    this.contentInfo.Reset();
                    this.provider.Dispose();
                    this.provider = null;
                    this.cacheKey = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseCache() {
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


    }
}
