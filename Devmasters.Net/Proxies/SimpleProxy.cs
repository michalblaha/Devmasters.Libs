using System;

namespace Devmasters.Net.Proxies
{
    public class SimpleProxy : IWebProxyWithStatus
    {
        public SimpleProxy(string address, int port)
            : this(new System.Net.WebProxy(address, port))
        {
        }

        public System.Net.IWebProxy WebProxy { get; protected set; }
        public SimpleProxy(System.Net.WebProxy proxy)
        {
            this.WebProxy = proxy;
            this.Working = false;
            this.LastChecked = null;
            this.ProxyUri = proxy.Address;
        }


        long _numberOfRequests = 0;
        public virtual long NumberOfRequests
        {
            get
            {
                return System.Threading.Interlocked.Read(ref _numberOfRequests);
            }
            set
            {

                _numberOfRequests = value;
            }
        }

        public virtual Uri ProxyUri { get; set; }

        public virtual DateTime? LastChecked { get; set; }
        public virtual TimeSpan? LastElapsedTime { get; set; }
        public virtual Exception LastException { get; set; }

        public virtual bool Working { get; set; }

        public virtual void SetStatus(TestProxyResult result)
        {
            this.Working = result.Success;
            this.LastChecked = DateTime.Now;
            this.LastElapsedTime = result.ElapsedTime;
            this.LastException = result.Error;
        }


        public virtual System.Net.ICredentials Credentials
        {
            get
            {
                return this.WebProxy.Credentials;
            }
            set
            {
                this.WebProxy.Credentials = value;
            }
        }

        public virtual Uri GetProxy(Uri destination)
        {
            System.Threading.Interlocked.Increment(ref _numberOfRequests);
            return this.WebProxy.GetProxy(destination ?? new Uri("http://www.seznam.cz"));
        }

        public virtual bool IsBypassed(Uri host)
        {
            return this.WebProxy.IsBypassed(host);
        }
    }

}
