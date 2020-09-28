using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Devmasters.Cache.LocalMemory
{
    [Serializable()]
    public class LocalMemoryCache<T> : BaseCache<T>
        where T : class
    {

        public LocalMemoryCache(TimeSpan expiration, System.Func<object, T> contentFunc)
            : this(expiration, null, contentFunc, null,null)
        { }
        public LocalMemoryCache(TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc)
            : this(expiration, cacheKey, contentFunc, null,null)
        { }
        public LocalMemoryCache(TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, object parameters, System.Runtime.Caching.CacheEntryUpdateCallback onUpdateCacheCallback)
            : base(new LocalMemory.LocalMemoryCacheProvider<T>(onUpdateCacheCallback),expiration, cacheKey, contentFunc, parameters)
        {
        }

    }
}
