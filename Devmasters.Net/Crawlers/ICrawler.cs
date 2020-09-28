namespace Devmasters.Net.Crawlers
{
    public interface ICrawler
    {
        string Name { get; }
        string[] IP { get; }
        string[] HostName { get; }
        string[] UserAgent { get; }

        bool IsItCrawler(string ip, string useragent);
    }
}
