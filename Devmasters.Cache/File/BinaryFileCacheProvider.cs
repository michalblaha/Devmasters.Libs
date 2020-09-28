using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace Devmasters.Cache.File
{


    public class BinaryFileCacheProvider : ICacheProvider<byte[]>
    {
        public static Devmasters.Logging.Logger Logger = new Logging.Logger("Devmasters.Cache.V20.File");



        object syncObj = new object();

        string extContent = ".filecache";
        FilenameDistributedFilePath dfn = null;
        TimeSpan expiration = TimeSpan.Zero;

        public BinaryFileCacheProvider(string path, TimeSpan expiration, int hashLegth = 2)
        {
            this.expiration = expiration;
            if (string.IsNullOrEmpty(path))
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //else
            //    path = System.IO.Path.GetDirectoryName(path);

            dfn = new FilenameDistributedFilePath(hashLegth, path);
            dfn.InitPaths();
        }

        public BinaryFileCacheProvider()
           : this(null, TimeSpan.FromHours(6))
        {
        }

        #region ICacheProvider<T> Members

        public string EncodeFilename(string key)
        {
            if (key.Length > 120)
                return Devmasters.Crypto.Hash.ComputeHashToBase64(key) + extContent;

            StringBuilder sb = new StringBuilder();
            foreach (var c in key)
            {
                if (System.IO.Path.GetInvalidFileNameChars().Contains(c))
                    sb.Append("_");
                else
                    sb.Append(c);
            }
            string fn = sb.ToString();
            if (fn.Length > 120)
                fn = Devmasters.Crypto.Hash.ComputeHashToBase64(fn);
            return fn + extContent;
        }

        public void Init()
        {
            dfn.InitPaths();
        }

        public void Remove(string key)
        {
            string fn = GetFullPath(key);
            Logger.Debug("Removing cache file from  " + fn);
            var f1 = DeleteFile(fn);
            Logger.Debug("Removed cache file from " + fn);
        }

        private bool DeleteFile(string fullPath)
        {

            try
            {
                System.IO.File.Delete(fullPath);
                Logger.Debug("V20.BinaryFillCacheProvider file deleted: " + fullPath);
                return true;

            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(100);
                try
                {
                    System.IO.File.Delete(fullPath);
                    return true;

                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(200);

                    try
                    {
                        System.IO.File.Delete(fullPath);
                        return true;
                    }
                    catch (Exception)
                    {
                        Logger.Error("V20.BinaryFillCacheProvider file cannot be deleted: " + fullPath);
                        return false;
                    }
                }

            }
        }

        protected string nullString = "/*null*/";

        public void Insert(string key, byte[] value, TimeSpan expiration)
        {
            try
            {
                var expireData = new ExpirationData(expiration);

                lock (syncObj)
                {
                    //Remove(key);
                    Logger.Debug("V20.BinaryFillCacheProvider saving into " + GetFullPath(key));
                    System.IO.File.WriteAllBytes(GetFullPath(key), value);
                }
            }
            catch (Exception e)
            {
                Logger.Error("V20.BinaryFillCacheProvider error saving into " + GetFullPath(key), e);
            }

        }



        public bool Exists(string key)
        {
            var fn = GetFullPath(key);
            if (!System.IO.File.Exists(fn))
            {
                return false;
            }

            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(fn);


                if (fi.CreationTimeUtc.Add(this.expiration) < DateTime.UtcNow)
                {
                    this.DeleteFile(fn);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Debug("V20.BinaryFillCacheProvider Exists error for key " + key, e);
                //Remove(key);
                return false;
            }

        }

        private string GetFullPath(string key)
        {
            return dfn.GetFullPath(key, EncodeFilename(key));
        }


        public byte[] Get(string key)
        {
            try
            {
                var fn = GetFullPath(key);
                if (System.IO.File.Exists(fn))
                {
                    return System.IO.File.ReadAllBytes(GetFullPath(key));
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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

        #endregion


    }
}
