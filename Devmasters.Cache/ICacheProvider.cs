using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devmasters.Cache
{

    public class CacheRemoveEventArgs : EventArgs
    {
        public System.Runtime.Caching.OnChangedCallback Reason { get; set; }

    }

    public interface ICacheProvider<T> : IDisposable
    {
        void Remove(string key);

        void Insert(string key, T value, TimeSpan expiration);

        bool Exists(string key);
        T Get(string key);
        void Init();
    }
}
