using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Devmasters.Net.Proxies
{
    public static class Helper
    {

        public static void TestProxies(List<IWebProxyWithStatus> proxiesToCheck)
        {
            var results = new List<TestProxyResult>();

            Parallel.ForEach(proxiesToCheck,
#if DEBUG
               //new ParallelOptions() { MaxDegreeOfParallelism = 1 },// for DEBUG set 1
#else
#endif
               wp =>
               {
                   TestProxyResult res = TestProxy(wp);
                   wp.SetStatus(res);
                   System.Diagnostics.Debug.WriteLine(wp.GetProxy(new Uri("http://www.seznam.cz")).AbsoluteUri + " " + res.ElapsedTime.Milliseconds.ToString() + " " + res.Success);

               });

        }

        static Uri apiHost = new Uri("http://api.devmasters.cz/ip.ashx");
        static string host = apiHost.Host;
        public static TestProxyResult TestProxy(IWebProxyWithStatus wp, int timeoutInMs = 10000)
        {


            TestProxyResult result = new TestProxyResult();
            var sw = new Stopwatch();

            using (Devmasters.Net.HttpClient.URLContent url = new Devmasters.Net.HttpClient.URLContent("http://api.devmasters.cz/ip.ashx"))
            {
                try
                {
                    sw.Start();
                    url.Timeout = timeoutInMs;
                    url.Proxy = wp;

                    string content = url.GetContent().Text;

                    result.Success = (content == wp.GetProxy(apiHost).Host);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex;
                }
                finally
                {
                    sw.Stop();
                    result.ElapsedTime = sw.Elapsed;
                }
                return result;
            }
        }

        /*
        public static TestProxyResult TestProxy2(WebProxy wp)
        {

            TestProxyResult result = new TestProxyResult();
            var sw = new Stopwatch();
            try
            {
                sw.Start();
                var response = new RestClient
                {
                    BaseUrl = new Uri("https://webapi.theproxisright.com/"),
                    Proxy = wp
                }.Execute(new RestRequest
                {
                    Resource = "api/ip",
                    Method = Method.GET,
                    Timeout = 10000,
                    RequestFormat = DataFormat.Json
                });
                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }
                result.Success = (response.TextContent == wp.Address.Host);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex;
            }
            finally
            {
                sw.Stop();
                result.ElapsedTime = sw.Elapsed;
            }
            return result;

        }
        */

    }
}
