using Couchbase;
using Couchbase.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devmasters.Cache.Couchbase
{
    public class Connection
    {
        private static object _clientLock = new object();
        private static Dictionary<string, IBucket> _clients = new Dictionary<string, IBucket>();

        private static object locker = new object();

        public static IBucket GetCouchbaseClient(string[] serversUrl, string bucketName, string username, string password)
        {
            string clientKey = string.Join(",", serversUrl) 
                + "-" + bucketName 
                + "-" + Devmasters.Crypto.Hash.ComputeHashToHex(username) + Devmasters.Crypto.Hash.ComputeHashToHex(password);

            lock (_clientLock)
            {
                if (!_clients.ContainsKey(clientKey))
                {
                    Cluster cluster = new Cluster(new global::Couchbase.Configuration.Client.ClientConfiguration
                    {
                        Servers = serversUrl.Select(s => new Uri(s)).ToList()
                    });

                    var authenticator = new global::Couchbase.Authentication.PasswordAuthenticator(
                        username,
                        password);
                    cluster.Authenticate(authenticator);
                    var bucket = cluster.OpenBucket(bucketName);

                    _clients.Add(clientKey, bucket);
                }
                return _clients[clientKey];
            }

        }

    }

}
