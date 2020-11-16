using Devmasters.Cache;
using System;
using System.Collections.Generic;

namespace Devmasters.Cache.Elastic
{

    [Serializable()]
    public class ElasticCache<T> : BaseCache<T>
        where T : class
    {

        public ElasticCache(string[] elasticServers, string indicieName, TimeSpan expiration, System.Func<object, T> contentFunc,
            int numberOfReplicas = 2, int numberOfShards = 3, string providerId = "")
            : this(elasticServers, indicieName, expiration,null,contentFunc, numberOfReplicas, numberOfShards, providerId)
        { }
        public ElasticCache(string[] elasticServers, string indicieName, TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc,
            int numberOfReplicas = 2, int numberOfShards = 3, string providerId = "")
            : this(elasticServers, indicieName, 
                  expiration, cacheKey,contentFunc,null, numberOfReplicas, numberOfShards, providerId)
        { }
        public ElasticCache(string[] elasticServers, string indicieName, TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, object parameters,
            int numberOfReplicas = 2, int numberOfShards = 3, string providerId = "")
            : base(new ElasticCacheProvider<T>(elasticServers, indicieName, numberOfReplicas, numberOfShards, providerId), 
                  expiration, cacheKey, contentFunc, parameters)
        {
        }

    }

}