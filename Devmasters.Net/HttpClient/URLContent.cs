
/* Unmerged change from project 'Devmasters.Net (net472)'
Before:
using Devmasters;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Net.Sockets;
After:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
*/
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Devmasters;

using Devmasters;
using Devmasters.DT;
using Devmasters.Net.Proxies;

namespace Devmasters.Net.HttpClient
{

    /// <summary>
    /// Allowed types of HTTP Request
    /// </summary>
    public enum MethodEnum
    {
        GET,
        POST,
        PUT,
        DELETE,
        HEAD
    }

    /// <summary>
    /// Downloads content of URL page (usefull only for text content, not for binary content)
    /// </summary>
    public class URLContent : IDisposable
    {

        private static Devmasters.Logging.Logger WebLogger = new Devmasters.Logging.Logger("Devmasters.Net.HttpClient");


        private NetworkCredential _credentials = null;

        private MethodEnum _method = MethodEnum.GET;

        ParametersContainer _requestParams = new ParametersContainer();
        ParametersContainer _responseParams = null;

        bool _orig_Expect100Continue = System.Net.ServicePointManager.Expect100Continue;
        bool _orig_UseNagleAlgorithm = System.Net.ServicePointManager.UseNagleAlgorithm;


        /// <summary>
        /// Initializes a new instance of the class, for URL with parameters in URL defined in queryString value
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="queryString"></param>
        public URLContent(string URL, System.Collections.Specialized.NameValueCollection queryString)
            : this()
        {
            System.Text.StringBuilder tmpURL = new StringBuilder(URL);
            if (!URL.Contains("?"))
            {
                tmpURL.Append("?");
            }
            if (queryString != null)
            {
                foreach (string key in queryString)
                {
                    tmpURL.Append(key);
                    tmpURL.Append("=");
                    tmpURL.Append(System.Net.WebUtility.UrlEncode(queryString[key]));
                    tmpURL.Append("&");
                }
            }
            this.Url = tmpURL.ToString();
        }


        /// <summary>
        /// Initializes a new instance of the class, for URL
        /// </summary>
        /// <param name="URL"></param>
        public URLContent(string Url)
            : this(Url, (UrlContentContext)null)
        {
        }

        public URLContent(string Url, UrlContentContext previousURLcontext)
            : this()
        {
            if (previousURLcontext != null)
            {
                this.RequestParams.Cookies.Add(previousURLcontext.Cookies);
                this.Referer = previousURLcontext.Url;
            }
            this.Url = Url;
        }



        private URLContent()
        {
            //init default values
            this.UserAgent = BrowserUserAgent.ChromeNew;
            this.Timeout = 15000; //15s
            this.Proxy = null;
            this.ProcessTime = TimeSpan.Zero;
            this.IgnoreHttpErrors = false;
        }

        public bool IgnoreHttpErrors { get; set; }
        public bool DontLogAnything { get; set; } = false;

        public BrowserUserAgent UserAgent { get; set; }

        public int Timeout { get; set; }

        public string Url { get; protected set; }
        public string ResponseUrl { get; protected set; }

        public string Referer { get; set; }

        public int Tries { get; set; } = 1;
        public int TimeInMsBetweenTries { get; set; } = 2000;


        /// <summary>
        /// Get or set ParametersContainer for Request
        /// </summary>
        public ParametersContainer RequestParams
        {
            get
            {
                return _requestParams;
            }
        }

        /// <summary>
        /// Get ParametersContainer for HTTP Response.
        /// Its always null before you call GetContent method
        /// </summary>
        public ParametersContainer ResponseParams
        {
            get { return _responseParams; }
        }

        /// <summary>
        /// Get or set ParametersContainer for Request
        /// </summary>
        public IWebProxyWithStatus Proxy { get; set; }


        /// <summary>
        /// Get ContentType of page
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// How long was content downloaded and processed. Before calling GetContent method its always 0 ticks.
        /// </summary>
        public TimeSpan ProcessTime { get; protected set; }

        /// <summary>
        /// Get or set GET or POST method for request
        /// </summary>
        public MethodEnum Method
        {
            get { return _method; }
            set
            {
                _method = value;
                if (_method == MethodEnum.POST)
                    EnablePostConfig();
                else
                    DisablePostConfig();
            }
        }

        /// <summary>
        /// Set all parameters for POST request
        /// </summary>
        private void EnablePostConfig()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            System.Net.ServicePointManager.UseNagleAlgorithm = false;
            this.ContentType = "application/x-www-form-urlencoded";

        }

        /// <summary>
        /// Set all parameters for GET method (to values in time of initialization of class or before POST request)
        /// </summary>
        private void DisablePostConfig()
        {
            System.Net.ServicePointManager.Expect100Continue = _orig_Expect100Continue;
            System.Net.ServicePointManager.UseNagleAlgorithm = _orig_UseNagleAlgorithm;
            this.ContentType = "";

        }

        /// <summary>
        /// Set basic credentials for request
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void SetCredentials(string username, string password)
        {
            _credentials = new NetworkCredential(username, password);

        }

        /// <summary>
        /// Set basic credentials for request, included domain
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public void SetCredentials(string username, string password, string domain)
        {
            _credentials = new NetworkCredential(username, password, domain);
        }


        /// <summary>
        /// Get content of page in defined charset 
        /// </summary>
        /// <param name="charset"></param>
        /// <returns></returns>
        public virtual TextContentResult GetContent(Encoding encoding)
        {
            StopWatchEx stopwatch = new StopWatchEx();
            stopwatch.Start();

            string content = FillContent(this.Url, encoding);

            stopwatch.Stop();
            this.ProcessTime = stopwatch.Elapsed;

            CookieCollection cookies = new CookieCollection();
            if (this.ResponseParams != null && this.ResponseParams.Cookies != null)
                cookies.Add(this.ResponseParams.Cookies);
            else
                cookies = null;

            WebHeaderCollection headers = new WebHeaderCollection();
            if (this.ResponseParams != null && this.ResponseParams.Headers != null)
                headers.Add(this.ResponseParams.Headers);
            else
                headers = null;
            return new TextContentResult()
            {
                Text = content,
                Context = new UrlContentContext()
                {
                    Cookies = cookies,
                    Headers = headers,
                    Url = this.Url,
                    Referer = this.Referer
                }
            };

        }

        public virtual BinaryContentResult GetBinary()
        {
            StopWatchEx stopwatch = new StopWatchEx();
            stopwatch.Start();
            byte[] data = null;
            try
            {
                data = GetRawContent(this.Url);
            }
            catch (UrlContentException ex)
            {
                if (this.IgnoreHttpErrors && ex.DownloadedContent != null)
                {
                    return new BinaryContentResult()
                    {
                        Binary = ex.DownloadedContent,
                        Context = new UrlContentContext()
                        {
                            Url = this.Url,
                        }
                    };
                }
                else
                    throw;
            }
            stopwatch.Stop();
            this.ProcessTime = stopwatch.Elapsed;

            CookieCollection cookies = new CookieCollection();
            if (this.ResponseParams != null && this.ResponseParams.Cookies != null)
                cookies.Add(this.ResponseParams.Cookies);
            else
                cookies = null;

            WebHeaderCollection headers = new WebHeaderCollection();
            if (this.ResponseParams != null && this.ResponseParams.Headers != null)
                headers.Add(this.ResponseParams.Headers);
            else
                headers = null;
            return new BinaryContentResult()
            {
                Binary = data,
                Context = new UrlContentContext()
                {
                    Cookies = cookies,
                    Headers = headers,
                    Url = this.Url,
                    Referer = this.Referer
                }
            };
        }

        /// <summary>
        /// Get content of page, try to find out charset automatically
        /// </summary>
        /// <returns></returns>
        public virtual TextContentResult GetContent()
        {
            return GetContent(null);
        }

        #region "private"

        /// <summary>
        /// Find ouf charset of page from HTTP Headers or first 4 kB of request
        /// </summary>
        /// <param name="result"></param>
        /// <param name="receiveStream"></param>
        /// <returns></returns>
        private Encoding GetCharset(ref byte[] rawdata)
        {
            int size = 4 * 1024;
            string Content;
            string sCharsetRegex = "(?ixm-s)(charset=(?<charset>[A-Za-z\\-0-9_]*)|encoding=\\W*(?<charset>[A-Za-z\\-0-9_]*))";
            Regex FoundRegex;
            MatchCollection found;
            if (rawdata == null || rawdata.Length == 0)
                return System.Text.Encoding.ASCII;
            try
            {

                string sHeaderValue;
                string sCharset;
                if (this.ResponseParams != null)
                {
                    foreach (string sHeaderKey in this.ResponseParams.Headers)
                    {
                        if (sHeaderKey.ToLower().Contains("content-type"))
                        {
                            sHeaderValue = this.ResponseParams.Headers[sHeaderKey];
                            FoundRegex = new Regex(sCharsetRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                            found = FoundRegex.Matches(sHeaderValue);
                            foreach (Match aMatch in found)
                            {

                                sCharset = aMatch.Groups["charset"].Value.Trim();
                                if (!string.IsNullOrEmpty(sCharset))
                                {
                                    try
                                    {
                                        return System.Text.Encoding.GetEncoding(sCharset);
                                    }
                                    catch
                                    {
                                        logger_Debug("No valid charset identified in headers");
                                    }
                                }
                            }
                        }
                    }
                }

                if (rawdata.Length < size)
                    size = rawdata.Length;

                StringBuilder ContentSB = new StringBuilder(size);
                for (int i = 0; i < size; i++)
                {
                    ContentSB.Append(Core.Chr(rawdata[i]));
                }
                Content = ContentSB.ToString();

                Content = Core.Left(Content, size);
                FoundRegex = new Regex(sCharsetRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                found = FoundRegex.Matches(Content);
                foreach (Match aMatch in found)
                {
                    sCharset = aMatch.Groups["charset"].Value.Trim();
                    if (sCharset != "")
                    {
                        try
                        {
                            return System.Text.Encoding.GetEncoding(sCharset);
                        }
                        catch
                        {
                            logger_Debug("No valid charset identified in HTML");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger_Debug("Some exception occured in time of searching charset", ex);
                return System.Text.Encoding.ASCII;
            }
            finally
            { }

            return System.Text.Encoding.ASCII;
        }

        private string FillContent(string URL, Encoding encoding)
        {
            byte[] rawdata = null;
            try
            {
                rawdata = GetRawContent(URL);

            }
            catch (UrlContentException ex)
            {
                if (this.IgnoreHttpErrors && ex.DownloadedContent != null)
                {
                    rawdata = ex.DownloadedContent;
                }
                else
                    throw;
            }

            if (encoding == null)
                encoding = GetCharset(ref rawdata);


            string html = encoding.GetString(rawdata);
            return html;

        }

        private Byte[] GetRawContent(string URL)
        {
            int numTry = 0;
            do
            {
                numTry++;
                try
                {
                    return doHttpRequest(URL);

                }
                catch (Exception e)
                {

                    if (numTry >= this.Tries)
                        throw;
                }

            } while (numTry <= this.Tries);
            return null;
        }
        private Byte[] doHttpRequest(string URL)
        {
            string downloadedcontent = string.Empty;
            HttpWebResponse _httpResponse = null;
            HttpWebRequest _httprequest = null;
            try
            {
                _httprequest = (HttpWebRequest)HttpWebRequest.Create(this.Url);

                //logger_Debug("Request stream parameters setting");
                //set request parameters before calling HTTP request
                if (_requestParams != null)
                {
                    CookieContainer cc = new CookieContainer();
                    cc.Add(_requestParams.Cookies);
                    _httprequest.CookieContainer = cc;
                    foreach (string key in _requestParams.Headers.AllKeys)
                    {
                        _httprequest.Headers.Add(key, _requestParams.Headers[key]);
                    }
                    //_httprequest.Headers = _requestParams.Headers;
                }
                _httprequest.UserAgent = Helper.GetUserAgent(this.UserAgent);
                _httprequest.Timeout = this.Timeout;
                _httprequest.Proxy = this.Proxy ?? WebRequest.DefaultWebProxy;
                _httprequest.Method = _method.ToString();
                _httprequest.Credentials = _credentials;
                if (!string.IsNullOrEmpty(this.Referer))
                    _httprequest.Referer = this.Referer;

                if (!string.IsNullOrEmpty(_requestParams?.Accept))
                    _httprequest.Accept = _requestParams.Accept;
                if (!string.IsNullOrEmpty(_requestParams?.Referer))
                    _httprequest.Referer = _requestParams.Referer;

                if (_requestParams != null)
                    _httprequest.AllowAutoRedirect = _requestParams.AllowAutoRedirect;

                if (_credentials != null && !string.IsNullOrEmpty(_credentials.UserName) && !string.IsNullOrEmpty(_credentials.Password))
                {
                    string ticket = string.Format("{0}:{1}", _credentials.UserName, _credentials.Password);
                    _httprequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(ticket)));
                }

                _httprequest.ContentType = this.ContentType;

                //if POST, add post data into request stream
                if (_method == MethodEnum.POST)
                {
                    Stream streamReq = _httprequest.GetRequestStream();
                    string postData = this.RequestParams.FormInPostDataFormat();
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    streamReq.Write(byteArray, 0, byteArray.Length);
                    streamReq.Close();
                }


                //logger_Debug("Request started, loading data");

                //Open request, load data
                try
                {
                    _httpResponse = (HttpWebResponse)_httprequest.GetResponse();

                }
                catch (WebException e_h)
                {
                    _httpResponse = (HttpWebResponse)e_h.Response; 
                    if (_httpResponse.StatusCode != HttpStatusCode.Redirect) 
                        throw (e_h); 
                }

                _responseParams = new ParametersContainer(_httpResponse.Headers, _httpResponse.Cookies);
                this.ContentType = _httpResponse.ContentType;

                byte[] rawdata = ReadAllBytesFromHttpStream(_httpResponse.GetResponseStream());
                this.ResponseUrl = _httpResponse.ResponseUri?.AbsoluteUri ?? this.Url;
                return rawdata;


            }
            catch (WebException ex)
            {
                if (this.Proxy != null)
                    this.Proxy.SetStatus(new TestProxyResult() { Error = ex, Success = false });

                byte[] rawdata = null;
                if (ex.Response != null)
                {
                    this.ResponseUrl = ex.Response.ResponseUri?.AbsoluteUri ?? this.Url;
                    MemoryStream exmemStream = new MemoryStream();
                    if (ex.Response.GetResponseStream() != null && ex.Response.GetResponseStream().CanRead)
                    {
                        rawdata = ReadAllBytesFromHttpStream(ex.Response.GetResponseStream());
                        //var encoding = GetCharset(ref rawdata);
                        //string exdownloadedcontent = encoding.GetString(rawdata);


                    }
                }

                logger_Error(this.ToString() + "\n\n", ex);
                //throw new WebException(exdownloadedcontent, ex);
                throw new UrlContentException(this.ToString(), ex) { DownloadedContent = rawdata };
            }
            catch (Exception exc)
            {
                if (this.Proxy != null)
                    this.Proxy.SetStatus(new TestProxyResult() { Error = exc, Success = false });

                logger_Error(this.ToString(), exc);
                throw new UrlContentException("", exc);
            }
            finally
            {

                _httprequest = null;
                if (!(_httpResponse == null))
                {
                    _httpResponse.Close();
                }
                //logger_Debug("Request stream closed");
                if (_method == MethodEnum.POST)
                {
                    //clear special config for POSTing data
                    DisablePostConfig();
                }
            }

        }


        private byte[] ReadAllBytesFromHttpStream(Stream receiveStream)
        {

            //read to binary array & convert to text with corrent charset
            using (MemoryStream binaryHttpData = new MemoryStream())
            {
                using (BinaryReader binaryReaderFromHttp = new BinaryReader(receiveStream))
                {

                    StreamReader fromBinaryDataToTextReader = null;
                    byte[] read = new byte[2048];
                    int count = binaryReaderFromHttp.Read(read, 0, read.Length);
                    while (count > 0)
                    {
                        binaryHttpData.Write(read, 0, count);
                        count = binaryReaderFromHttp.Read(read, 0, read.Length);
                    }

                    if (binaryReaderFromHttp != null)
                        binaryReaderFromHttp.Close();
                    if (fromBinaryDataToTextReader != null)
                        fromBinaryDataToTextReader.Close();
                    if (binaryHttpData != null)
                        binaryHttpData.Close();

                    read = null;
                    return binaryHttpData.ToArray();
                }
            }
        }

        public override string ToString()
        {
            string template = "URL: {0}\nPost:{1}";
            if (this.RequestParams != null)
                return string.Format(template, this.Url, CollectionToString(this.RequestParams.Form));
            else
                return string.Format(template, this.Url, "");


            //return base.ToString();
        }

        private string CollectionToString(System.Collections.Specialized.NameValueCollection coll)
        {
            StringBuilder sb = new StringBuilder();
            if (coll != null)
            {
                foreach (string key in coll)
                {
                    sb.AppendLine(key + " - " + coll[key]);
                }
            }
            return sb.ToString();
        }

        private void logger_Error(string msg, Exception ex = null)
        {
            try
            {
                if (!this.DontLogAnything)
                    WebLogger.Error(msg, ex);
            }
            catch
            {
            }
        }
        private void logger_Debug(string msg, Exception ex = null)
        {
            try
            {
                if (!this.DontLogAnything)
                    WebLogger.Debug(msg, ex);
            }
            catch
            {
            }
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion
    }
}

