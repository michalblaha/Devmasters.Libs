namespace Devmasters.Net.Proxies.Providers
{
    public abstract class BaseProxyProvider : IProxyProvider
    {
        public abstract IWebProxyWithStatus GetProxy();
        public abstract void Init();
        public abstract void Refresh();

        public void CheckStatus(IWebProxyWithStatus proxy)
        {
            TestProxyResult res = Helper.TestProxy(proxy);
            proxy.SetStatus(res);
        }

    }
}
