using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devmasters.Cache.File
{
    public class AutoUpdatedFileCache<T> : AutoUpdatebleCache<T>
                where T : class
    {
        public AutoUpdatedFileCache(string path, TimeSpan expiration, Func<object, T> contentFunc)
            : this(path,expiration, null, contentFunc, null)
        {
        }

        public AutoUpdatedFileCache(string path, TimeSpan expiration, string cacheKey, Func<object, T> contentFunc)
            : this(path, expiration, cacheKey, contentFunc, null)
        {
        }

        public AutoUpdatedFileCache(string path, TimeSpan expiration, string cacheKey, Func<object, T> contentFunc, object parameters)
            : base(new File.FileCacheProvider<T>(path), expiration, cacheKey, contentFunc, parameters)
        {
        }
    }
}
