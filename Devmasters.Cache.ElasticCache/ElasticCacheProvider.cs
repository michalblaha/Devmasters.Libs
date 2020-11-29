using System.Linq;
using Devmasters.Cache;
using System;
using System.Configuration;
using Nest;
using Elasticsearch.Net;
using System.Text;

namespace Devmasters.Cache.Elastic
{
    public class ElasticCacheProvider<T> : ICacheProvider<T>
      where T : class
    {

        public static Devmasters.Logging.Logger ESTraceLogger = new Devmasters.Logging.Logger("Devmasters.Cache.Elastic.Trace");
        public static bool ESTraceLoggerExists = log4net.LogManager.Exists("Devmasters.Cache.Elastic.Trace")?.Logger?.IsEnabledFor(log4net.Core.Level.Debug) == true;

        ElasticClient client = null;
        System.Timers.Timer cleaning = new System.Timers.Timer(1000 * 60 * 60 * 1); //1 hours
        public ElasticCacheProvider(string[] elasticServers, string dbName,
            int numberOfReplicas = 2, int numberOfShards = 3, string providerId = "")
        {
            this.ElasticServers = elasticServers;
            this.DBName = System.Text.RegularExpressions.Regex.Replace(dbName.ToLower(), "[^a-z]", "", System.Text.RegularExpressions.RegexOptions.None);
            this.NumberOfReplicas = numberOfReplicas;
            this.NumberOfShards = numberOfShards;
            this.ProviderId = providerId;

        }

        public void Init()
        {
            ESTraceLogger.Debug($"ElasticCacheProvider Init: Starting");
            if (client == null)
            {
                var pool = new Elasticsearch.Net.StaticConnectionPool(
                    this.ElasticServers
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Select(u => new Uri(u))
                );
                var settings = new ConnectionSettings(pool)
                    .DefaultIndex(this.DBName)
                    .DisableAutomaticProxyDetection(false)
                    .RequestTimeout(TimeSpan.FromMilliseconds(60000))
                    .SniffLifeSpan(null)
                    .OnRequestCompleted(call =>
                    {
                        // log out the request and the request body, if one exists for the type of request
                        if (call.RequestBodyInBytes != null)
                        {
                            ESTraceLogger.Debug($"{call.HttpMethod}\t{call.Uri}\t" +
                                $"{Encoding.UTF8.GetString(call.RequestBodyInBytes)}");
                        }
                        else
                        {
                            ESTraceLogger.Debug($"{call.HttpMethod}\t{call.Uri}\t");
                        }

                    })
                    ;
                if (System.Diagnostics.Debugger.IsAttached || ESTraceLoggerExists || Devmasters.Config.GetWebConfigValue("ESDebugDataEnabled") == "true")
                    settings = settings.DisableDirectStreaming();

                client = new ElasticClient(settings);
                IndexSettings set = new IndexSettings();
                set.NumberOfReplicas = this.NumberOfReplicas;
                set.NumberOfShards = this.NumberOfShards;
                IndexState idxSt = new IndexState();
                idxSt.Settings = set;

                ESTraceLogger.Debug($"ElasticCacheProvider Init: Check Elastic DB {client.ConnectionSettings.DefaultIndex}");
                var resExists = client.Indices.Exists(client.ConnectionSettings.DefaultIndex);
                //if (resExists.IsValid == false)
                //{
                //    ESTraceLogger.Fatal($"ElasticCacheProvider: Cannot check db existance {resExists.ServerError?.ToString()}", resExists.OriginalException);
                //    throw new ApplicationException("Cannot check Elastic DB");
                //}
                if (resExists.Exists == false)
                {
                    ESTraceLogger.Debug($"ElasticCacheProvider Init: DB {client.ConnectionSettings.DefaultIndex} doesn't exist. Creating new one");

                    var res = client.Indices.Create(client.ConnectionSettings.DefaultIndex,
                        i => i.InitializeUsing(idxSt)
                                .Map<Bag<T>>(map => map.AutoMap().DateDetection(false))
                    );
                    if (res.IsValid == false)
                    {
                        ESTraceLogger.Fatal($"ElasticCacheProvider: Created db error {res.ServerError?.ToString()}", res.OriginalException);
                        throw new ApplicationException("Cannot created Elastic DB");
                    }
                }
                ESTraceLogger.Debug($"ElasticCacheProvider Init: DB {client.ConnectionSettings.DefaultIndex} is live.");
                cleaning.Elapsed += Cleaning_Elapsed;
                cleaning.Start();
            }
        }

        private void Cleaning_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int iteration = 0;

        startAgain:
            ESTraceLogger.Debug($"ElasticCacheProvider Cleaning_Elapsed: Starting (iteration {iteration})");
            //find expired keys
            var res = client
                .Search<Bag<T>>(s => s
                    .Query(q => q
                        .DateRange(dr => dr.LessThan(DateTime.Now).Field(f => f.ExpirationTime))
                    )
                    .Source(false)
                    .Size(9000)
                );
            if (res.IsValid)
            {
                ESTraceLogger.Info($"ElasticCacheProvider Cleaning_Elapsed: Successfull query. Found {res.Total} records to delete");

                Devmasters.Batch.Manager.DoActionForAll<string>(res.Hits.Select(m => m.Id),
                    id =>
                    {
                        ESTraceLogger.Debug($"ElasticCacheProvider Cleaning_Elapsed: removing record {id}");
                        Remove(id);
                        return new Batch.ActionOutputData() { };
                    }, true);
            }
            ESTraceLogger.Debug($"ElasticCacheProvider Cleaning_Elapsed: End (iteration {iteration})");
            if (res.Total > 9100 && iteration < 10)
            {
                iteration++;
                goto startAgain;
            }


        }

        string typeName = null;
        private string fixKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (typeName == null)
            {
                typeName = typeof(T).Name;
                if (typeName.EndsWith("`1"))
                    typeName = typeName.Substring(0, typeName.Length - 2);
            }
            key = typeName + "." + key;
            if (key.Length > 500)
                return Devmasters.Crypto.Hash.ComputeHashToBase64(key);
            else
                return key;
        }
        public void Remove(string key)
        {
            var res = client.Delete<Bag<T>>(fixKey(key));
            if (res.IsValid == false)
                ESTraceLogger.Error($"ElasticCacheProvider: remove record {key} error {res.ServerError?.ToString()}", res.OriginalException);
        }

        public void Insert(string key, T value, TimeSpan expiration)
        {
            if (value != null)
            {
                Bag<T> bag = new Bag<T>()
                {
                    ExpirationTime = (expiration == TimeSpan.Zero) ? DateTime.UtcNow.AddDays(365) : DateTime.UtcNow.Add(expiration),
                    Id = key,
                    Value = value,
                    Updated = DateTime.UtcNow,
                    ProviderId = this.ProviderId
                };
                var res = client.Index<Bag<T>>(bag, v => v.Id(fixKey(key)));
                if (res.IsValid == false)
                    ESTraceLogger.Error($"ElasticCacheProvider: insert record {key} error {res.ServerError?.ToString()}", res.OriginalException);

            }
            else
                BaseCache<T>.Logger.Warning(new Devmasters.Logging.LogMessage()
                    .SetMessage("ElasticCacheProvider> null value")
                    .SetLevel(Devmasters.Logging.PriorityLevel.Warning)
                    .SetCustomKeyValue("objectType", typeof(T).ToString())
                    .SetCustomKeyValue("cache key", key)
                    );

        }


        public bool Exists(string key)
        {
            var res = client.DocumentExists<Bag<T>>(fixKey(key));
            if (res.IsValid == false)
            {
                ESTraceLogger.Error($"ElasticCacheProvider: Exists record {key} error {res.ServerError?.ToString()}", res.OriginalException);
                return false;
            }
            else
            {
                var resG = client.Get<Bag<T>>(fixKey(key), s => s.SourceExcludes(se => se.RawValue));
                if (resG.IsValid && resG.Found)
                {
                    if (resG.Source == null)
                        return false;
                    else
                        return resG.Source.ExpirationTime >= DateTime.UtcNow;
                }
                else
                    return false;
            }
        }

        private Bag<T> GetInternal(string key)
        {
            var res = client.Get<Bag<T>>(fixKey(key));
            if (res.IsValid == false)
            {
                ESTraceLogger.Error($"ElasticCacheProvider: Get record {key} error {res.ServerError?.ToString()}", res.OriginalException);
                return null;
            }
            if (res.Found)
            {
                return res.Source;
            }
            else
                return null;
        }

        public T Get(string key)
        {
            var bag = GetInternal(key);
            if (bag == null)
                return null;
            else
                return bag.Value;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public string[] ElasticServers { get; private set; }
        public string DBName { get; private set; }
        public int NumberOfReplicas { get; private set; }
        public int NumberOfShards { get; private set; }
        public string ProviderId { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cleaning.Stop();
                    cleaning.Elapsed -= Cleaning_Elapsed;
                    cleaning.Dispose();
                }


                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LocalMemoryCacheProvider() {
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