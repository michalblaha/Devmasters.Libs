namespace Devmasters.Net.Crawlers
{
    public class Google : CrawlerBase
    {
        public override string Name { get => "Google"; }
        public override string[] HostName => new string[] { "googlebot.com", "google.com" };
        public override string[] IP { get => null; }
        public override string[] UserAgent
        {
            get => new string[] {
                "APIs-Google (+https://developers.google.com/webmasters/APIs-Google.html)",
                "Mediapartners-Google",
                "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1 (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)",
                "AdsBot-Google (+http://www.google.com/adsbot.html)",
                "Googlebot-Image/1.0",
                "Googlebot-News",
                "Googlebot-Video/1.0",
                "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
                "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; Googlebot/2.1; +http://www.google.com/bot.html) Safari/537.36",
                "Googlebot/2.1 (+http://www.google.com/bot.html)",
                "Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.96 Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
                "Mediapartners-Google/2.1; +http://www.google.com/bot.html)",
                "AdsBot-Google-Mobile-Apps",
            };
        }
    }

}
