namespace Devmasters.Net.Proxies.Providers
{

    public interface IProxyProvider
    {
        IWebProxyWithStatus GetProxy();
        void CheckStatus(IWebProxyWithStatus proxy);

        void Init();
        void Refresh();
    }
}
