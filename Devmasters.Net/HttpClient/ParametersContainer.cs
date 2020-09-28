using System;
using System.
/* Unmerged change from project 'Devmasters.Net (net472)'
Before:
using System.Text;
using System.Net;
After:
using System.Net;
using System.Text;
*/
Text;

namespace Devmasters.Net.HttpClient
{
    /// <summary>
    /// Contains HTTP Headers, Cookies and Post parameters
    /// </summary>
    public class ParametersContainer
    {
        System.Net.WebHeaderCollection _headers = null;
        System.Net.CookieCollection _cookies = null;
        System.Collections.Specialized.NameValueCollection _form = null;

        public string Referer { get; set; }
        public string Accept { get; set; }
        public bool AllowAutoRedirect { get; set; } = true;

        string rawContent = string.Empty;

        public string RawContent
        {
            get
            {
                return rawContent;
            }
            set
            {
                rawContent = value;
            }
        }
        /// <summary>
        /// Set of POST parameters and values
        /// </summary>
        public System.Collections.Specialized.NameValueCollection Form
        {
            get { return _form; }
        }

        public System.Net.WebHeaderCollection Headers
        {
            get { return _headers; }
        }

        public System.Net.CookieCollection Cookies
        {
            get { return _cookies; }
        }


        /// <summary>
        /// Return POST parameters in form used in HTTP request
        /// </summary>
        /// <returns></returns>
        public string FormInPostDataFormat()
        {
            if (_form == null)
                return "";

            if (RawContent.Length > 0)
                return RawContent;

            if (_form.Count == 0)
                return "";

            System.Text.StringBuilder data = new StringBuilder(1024);

            foreach (string key in _form.AllKeys)
            {
                data.Append(System.Net.WebUtility.UrlEncode(key));
                data.Append("=");
                data.Append(System.Net.WebUtility.UrlEncode(_form[key]));
                data.Append("&");
            }
            string sout = data.ToString().Remove(data.Length - 1, 1);
            return sout;
        }

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        public ParametersContainer()
        {
            _headers = new System.Net.WebHeaderCollection();
            _cookies = new System.Net.CookieCollection();
            _form = new System.Collections.Specialized.NameValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the class with Headers and cookies collections
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="cookies"></param>
        public ParametersContainer(System.Net.WebHeaderCollection headers, System.Net.CookieCollection cookies)
        {
            _headers = headers;
            _cookies = cookies;
            _form = new System.Collections.Specialized.NameValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the class with Headers, cookies and Forms collections
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="cookies"></param>
        /// <param name="form"></param>
        public ParametersContainer(System.Net.WebHeaderCollection headers, System.Net.CookieCollection cookies, System.Collections.Specialized.NameValueCollection form)
        {
            _headers = headers;
            _cookies = cookies;
            _form = form;
        }

        /// <summary>
        /// Filter Headers collection and removes headers which aren't in array allowedHeaders
        /// </summary>
        /// <param name="allowedHeaders"></param>
        public void FilterHeaders(string[] allowedHeaders)
        {
            foreach (string key in _headers.AllKeys)
            {
                //if not in allowed headers, remove it  
                if (Array.IndexOf(allowedHeaders, key) == -1)
                    _headers.Remove(key);
            }

        }

    }
}
