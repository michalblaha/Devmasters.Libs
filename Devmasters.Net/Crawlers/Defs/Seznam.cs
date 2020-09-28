namespace Devmasters.Net.Crawlers
{
    public class Seznam : CrawlerBase
    {
        public override string Name { get => "Seznam"; }
        public override string[] HostName => null;
        public override string[] IP { get => new string[] { "77.75.74.0/24", "77.75.76.0/24", "77.75.77.0/24", "77.75.78.0/24", "77.75.79.0/24" }; }
        public override string[] UserAgent { get => new string[] { "Mozilla/5.0 (compatible; SeznamBot/3.2; +http://napoveda.seznam.cz/en/seznambot-intro/)" }; }
    }

}
