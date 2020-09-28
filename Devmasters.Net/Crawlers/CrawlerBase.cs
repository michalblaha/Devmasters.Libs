using System;
using System.Linq;
using System.Net;

namespace Devmasters.Net.Crawlers
{

    public abstract class CrawlerBase : ICrawler
    {
        public abstract string Name { get; }
        public abstract string[] IP { get; }
        public abstract string[] HostName { get; }
        public abstract string[] UserAgent { get; }

        private IPNetwork[] _iPnets = null;
        private string[] _userAgents = null;
        private string[] _hostNames = null;

        public CrawlerBase()
        {
            if (IP != null)
                _iPnets = IP.Select(i => IPNetwork.Parse(i)).ToArray();
            if (UserAgent != null)
                _userAgents = UserAgent.Select(s => s.ToLower()).ToArray();
            if (HostName != null)
                _hostNames = HostName.Select(s => s.ToLower()).ToArray();
        }

        public bool IsItCrawler(string ip, string useragent)
        {
            if ((IP == null && HostName == null)
                || UserAgent == null)
                return false;

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(useragent))
                return false;

            IPAddress ipa = IPAddress.Parse(ip);

            useragent = useragent.ToLower();

            var detected = true;
            if (this._iPnets != null)
                detected = detected &&
                    _iPnets.Any(i => i.Contains(ipa));
            else if (_hostNames != null)
            {
                try
                {
                    string hostname = Dns.GetHostEntry(ip)?.HostName?.ToLower() ?? "";

                    if (string.IsNullOrEmpty(hostname))
                        detected = false;
                    else
                        detected = detected &&
                            _hostNames.Any(d => hostname.EndsWith(d.ToLower()));

                }
                catch (Exception)
                {
                    detected = false;
                }


            }

            if (detected)
                detected = detected &&
                    _userAgents.Any(ua => useragent.StartsWith(ua));

            return detected;
        }
    }
}
