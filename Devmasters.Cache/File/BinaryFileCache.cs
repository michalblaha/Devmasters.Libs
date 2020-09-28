using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Devmasters.Cache.File
{
    [Serializable()]
    public class BinaryFileCache : BaseCache<byte[]>
    {

        public BinaryFileCache(string path, TimeSpan expiration, System.Func<object, byte[]> contentFunc)
            : this(path, expiration, null, contentFunc, null)
        { }
        public BinaryFileCache(string path, TimeSpan expiration, string cacheKey, System.Func<object, byte[]> contentFunc)
            : this(path, expiration, cacheKey, contentFunc, null)
        { }
        public BinaryFileCache(string path, TimeSpan expiration, string cacheKey, System.Func<object, byte[]> contentFunc, object parameters)
            : base(new BinaryFileCacheProvider(path, expiration),expiration, cacheKey, contentFunc, parameters)
        {
        }

    }
}
