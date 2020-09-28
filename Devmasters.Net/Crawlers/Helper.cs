using System.Linq;

namespace Devmasters.Net.Crawlers
{
    public static class Helper
    {
        public static ICrawler[] AllCrawlers = new ICrawler[] {
            new Seznam(), new Google(), new Twitter(), new Facebook()
        };

        public static bool IsAnyCrawler(string ip, string userAgent)
        {
            return AllCrawlers.Any(cr => cr.IsItCrawler(ip, userAgent));
        }
    }
}
