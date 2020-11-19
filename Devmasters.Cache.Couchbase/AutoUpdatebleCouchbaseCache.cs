using Couchbase;
using System.Linq;
using Devmasters.Cache;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Devmasters.Cache.Couchbase
{
    [Serializable()]
    public class AutoUpdateCouchbaseCache<T> : AutoUpdatebleCache<T>
        where T : class
    {

        public AutoUpdateCouchbaseCache(TimeSpan expiration, System.Func<object, T> contentFunc, string[] serversUrl, string couchbaseBucketName, string username, string password, bool randomizeExpiration = true)
            : this(expiration, null, contentFunc, null,serversUrl,couchbaseBucketName,username,password,randomizeExpiration)
        { }
        public AutoUpdateCouchbaseCache(TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, string[] serversUrl, string couchbaseBucketName, string username, string password, bool randomizeExpiration = true)
            : this(expiration, cacheKey, contentFunc, null,serversUrl, couchbaseBucketName, username, password, randomizeExpiration)
        { }
        public AutoUpdateCouchbaseCache(TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, object parameters, string[] serversUrl, string couchbaseBucketName, string username, string password, bool randomizeExpiration = true)
            : base(new CouchbaseCacheProvider<T>(serversUrl, couchbaseBucketName, username, password, randomizeExpiration), expiration, cacheKey, contentFunc, parameters)
        {
        }

    }

}