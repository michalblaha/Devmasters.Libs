using Devmasters.Cache;
using System;
using System.Collections.Generic;

namespace Devmasters.Cache.Couchbase
{

    [Serializable()]
    public class CouchbaseCache<T> : BaseCache<T>
        where T : class
    {

        public CouchbaseCache(TimeSpan expiration, System.Func<object, T> contentFunc, string[] serversUrl, string couchbaseBucketName, string username, string password, bool randomizeExpiration = true)
            : this(expiration, null, contentFunc, null, serversUrl, couchbaseBucketName, username, password, randomizeExpiration)
        { }
        public CouchbaseCache(TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, string[] serversUrl, string couchbaseBucketName, string username, string password, bool randomizeExpiration = true)
            : this(expiration, cacheKey, contentFunc, null, serversUrl, couchbaseBucketName, username, password, randomizeExpiration)
        { }
        public CouchbaseCache(TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, object parameters, string[] serversUrl, string couchbaseBucketName, string username, string password, bool randomizeExpiration = true)
            : base(new CouchbaseCacheProvider<T>(serversUrl, couchbaseBucketName, username, password, randomizeExpiration), expiration, cacheKey, contentFunc, parameters)
        {
        }

    }

}