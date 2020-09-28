using System.Net;

namespace Devmasters.Net.HttpClient
{
    public class UrlContentContext
    {
        public UrlContentContext()
        {
            Cookies = new CookieCollection();
            Headers = new WebHeaderCollection();
            Referer = string.Empty;
        }
        public CookieCollection Cookies { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public string Referer { get; set; }
        public string Url { get; set; }
    }
}
