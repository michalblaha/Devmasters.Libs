namespace Devmasters.Net.HttpClient
{
    public enum BrowserUserAgent
    {
        IE6,
        IE9,
        IE11,
        IE_Old,
        IE_New,
        ChromeOld,
        ChromeNew,
        Chrome41,
        Chrome36,
        ChromeOS,
        FF_New,
        FF_Old,
        FF36,
        FF29,
        Opera,
        Opera_Mobile,
        Safari,
        AndroidWebkit,
        BlackBerry,

        GoogleBot,
        SeznamBot,
        YahooSeeker,
        YandexBot,
        Baiduspider


    }


    public static class Helper
    {

        //Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; FSL 7.0.6.01001)


        private static string getUserAgent(BrowserUserAgent browser)
        {

            switch (browser)
            {

                case BrowserUserAgent.IE9:
                    return "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; AS; rv:11.0) like Gecko";
                case BrowserUserAgent.IE6:
                case BrowserUserAgent.IE_Old:
                    return "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; AS; rv:11.0) like Gecko";

                case BrowserUserAgent.IE11:
                case BrowserUserAgent.IE_New:
                    return "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; AS; rv:11.0) like Gecko";

                case BrowserUserAgent.Chrome36:
                case BrowserUserAgent.ChromeOld:
                    return "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

                case BrowserUserAgent.ChromeOS:
                    return "Mozilla/5.0 (X11; CrOS i686 2268.111.0) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.57 Safari/536.11";


                case BrowserUserAgent.FF36:
                case BrowserUserAgent.FF_New:
                    return "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0";

                case BrowserUserAgent.FF29:
                case BrowserUserAgent.FF_Old:
                    return "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:25.0) Gecko/20100101 Firefox/29.0";

                case BrowserUserAgent.Opera:
                    return "Opera/9.80 (Windows NT 6.1; U; es-ES) Presto/2.9.181 Version/12.00";

                case BrowserUserAgent.Opera_Mobile:
                    return "Opera/12.02 (Android 4.1; Linux; Opera Mobi/ADR-1111101157; U; en-US) Presto/2.9.201 Version/12.02";

                case BrowserUserAgent.Safari:
                    return "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_2) AppleWebKit/600.4.10 (KHTML, like Gecko) Version/8.0.4 Safari/600.4.10";



                case BrowserUserAgent.GoogleBot:
                    return "Googlebot/2.1 (+http://www.google.com/bot.html)";
                case BrowserUserAgent.SeznamBot:
                    return "SeznamBot/2.0 (+http://fulltext.seznam.cz/)";
                case BrowserUserAgent.YahooSeeker:
                    return "YahooSeeker/1.2 (compatible; Mozilla 4.0; MSIE 5.5; yahooseeker at yahoo-inc dot com ; http://help.yahoo.com/help/us/shop/merchant/)";
                case BrowserUserAgent.YandexBot:
                    return "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)";
                case BrowserUserAgent.Baiduspider:
                    return "Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)";


                case BrowserUserAgent.Chrome41:
                case BrowserUserAgent.ChromeNew:
                default:
                    return "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

            }

        }


        /// <summary>
        /// Get default UserAgent for request
        /// </summary>
        /// <returns></returns>
        public static string GetUserAgent()
        {
            return getUserAgent(BrowserUserAgent.ChromeNew);
        }

        /// <summary>
        /// Get some of prepared UserAgents
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
        public static string GetUserAgent(BrowserUserAgent browser)
        {
            return getUserAgent(browser);
        }


    }
}
