using System;

namespace Devmasters.Net.Proxies
{
    public class TestProxyResult
    {

        public bool Success { get; set; }
        public Exception Error { get; set; }
        public TimeSpan ElapsedTime { get; set; }

    }
}
