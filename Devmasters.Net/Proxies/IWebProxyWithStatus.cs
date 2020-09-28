using System;
using System.Net;

namespace Devmasters.Net.Proxies
{
    public interface IWebProxyWithStatus
        : IWebProxy
    {

        long NumberOfRequests { get; set; }
        Uri ProxyUri { get; set; }
        DateTime? LastChecked { get; set; }
        TimeSpan? LastElapsedTime { get; set; }
        Exception LastException { get; set; }
        bool Working { get; set; }
        void SetStatus(TestProxyResult result);
    }
}
