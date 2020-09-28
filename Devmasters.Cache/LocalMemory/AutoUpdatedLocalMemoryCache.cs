using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Caching;

namespace Devmasters.Cache.LocalMemory
{
    [Serializable()]
    public class AutoUpdatedLocalMemoryCache<T> : AutoUpdatebleCache<T>
        where T : class
    {

        public AutoUpdatedLocalMemoryCache(TimeSpan expiration, System.Func<object, T> contentFunc)
            : this(expiration, null, contentFunc, null)
        { }
        public AutoUpdatedLocalMemoryCache(TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc)
            : this(expiration, cacheKey, contentFunc, null)
        { }
        public AutoUpdatedLocalMemoryCache(TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, object parameters)
            : base(new LocalMemoryCacheProvider<T>(null), expiration, cacheKey, contentFunc, parameters)
        {
            //rewrite provider initialization
            //this.provider = new LocalMemory.LocalMemoryCacheProvider<T>(this.UpdateCacheAgain);
            //this.provider.Init();

        }

        public void UpdateCacheAgain(CacheEntryUpdateArguments cacheInfo )
        {
            try
            {
                cacheInfo.UpdatedCacheItem = new CacheItem(this.cacheKey, base.GetDataFromContentFunction());
                cacheInfo.UpdatedCacheItemPolicy = new CacheItemPolicy() { UpdateCallback = UpdateCacheAgain, AbsoluteExpiration = DateTime.UtcNow.Add(this.contentInfo.ExpiresInterval) };
            }
            catch 
            { 
                cacheInfo.UpdatedCacheItem = new CacheItem(this.cacheKey, this.Get());
                cacheInfo.UpdatedCacheItemPolicy = new CacheItemPolicy() { UpdateCallback = UpdateCacheAgain, AbsoluteExpiration = DateTime.UtcNow.Add(this.contentInfo.ExpiresInterval) };
            }
            //this.
            //expensiveObject = base.GetDataFromContentFunction();
            //dependency = null;
            //absoluteExpiration = DateTime.UtcNow.Add(this.contentInfo.ExpiresInterval);
            //slidingExpiration = System.Web.Caching.Cache.NoSlidingExpiration;
        }
    }
}
