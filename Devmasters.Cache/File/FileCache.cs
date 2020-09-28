using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Devmasters.Cache.File
{
    [Serializable()]
    public class FileCache<T> : BaseCache<T>
        where T : class
    {

        public FileCache(string path, TimeSpan expiration, System.Func<object, T> contentFunc)
            : this(path, expiration, null, contentFunc, null)
        { }
        public FileCache(string path, TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc)
            : this(path, expiration, cacheKey, contentFunc, null)
        { }
        public FileCache(string path, TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, object parameters)
            : base(new File.FileCacheProvider<T>(path),expiration, cacheKey, contentFunc, parameters)
        {
        }

    }
}
