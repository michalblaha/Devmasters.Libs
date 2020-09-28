using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Caching;

namespace Devmasters.Cache
{
    [Serializable()]
    public abstract class AutoUpdatebleCache<T> : BaseCache<T>
        where T : class
    {
        private System.Timers.Timer expirationTimer;
        private TimeSpan originalExpiration { get; set; }

        public AutoUpdatebleCache(ICacheProvider<T> cacheProvider, TimeSpan expiration, System.Func<object, T> contentFunc)
            : this(cacheProvider, expiration, null, contentFunc)
        { }
        public AutoUpdatebleCache(ICacheProvider<T> cacheProvider, TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc)
            : this(cacheProvider, expiration, cacheKey, contentFunc, null)
        { }
        public AutoUpdatebleCache(ICacheProvider<T> cacheProvider, TimeSpan expiration, string cacheKey, System.Func<object, T> contentFunc, object parameters)
            : base(cacheProvider, expiration, cacheKey, contentFunc, parameters)
        {
            this.originalExpiration = expiration;
            base.contentInfo.ExpiresInterval = expiration.Add(expiration); //double real expiration
        }


        object contentFncLock = new object();
        bool enteredFnc = false;
        protected override T GetDataFromContentFunction()
        {
            lock (contentFncLock)
            {
                T res = default(T);
                try
                {

                    enteredFnc = true;
                    if (expirationTimer != null)
                    {
                        if (expirationTimer.Enabled)
                        {
                            expirationTimer.Elapsed -= expirationTimer_Elapsed;
                            expirationTimer.Stop();
                        }
                    }
                    res = base.GetDataFromContentFunction();
                }
                catch (Exception e)
                {
                    Console.WriteLine("e");
                }
                finally
                {
                    expirationTimer = new System.Timers.Timer(this.originalExpiration.TotalMilliseconds);
                    expirationTimer.Elapsed += expirationTimer_Elapsed;
                    expirationTimer.Start();
                    enteredFnc = false;
                }
                return res;

            }
        }


        bool expirationTimer_Elapsed_entered = false;
        private void expirationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (expirationTimer_Elapsed_entered)
            {
                return;
            }
            expirationTimer_Elapsed_entered = true;

            if (enteredFnc)
            {
                return;
            }
            try
            {
                this.ForceRefreshCache(base.GetDataFromContentFunction());
            }
            catch (Exception)
            {
                Console.WriteLine("e");
            }
            finally
            {
                expirationTimer_Elapsed_entered = false;
            }

        }
    }
}
