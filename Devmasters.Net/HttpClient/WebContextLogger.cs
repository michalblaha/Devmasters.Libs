using System;
using System.Net;
using System.
/* Unmerged change from project 'Devmasters.Net (net472)'
Before:
using System.Text;
using System.Web;
using System.Net;
using Devmasters;
using Microsoft.AspNetCore.Http;
After:
using System.Net;
using System.Text;
using System.Web;

using Devmasters;

using Microsoft.AspNetCore.Http;
*/
Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace Devmasters.Net
{
    public class WebContextLogger
    {
        public static string Log404WebError(HttpRequest req)
        {
            try
            {
                return GetWebContextInfoJust404(req);

            }
            catch (Exception exc)
            {
                //Logger.Error(exc);
                return "";
            }

        }

        public static string LogFatalWebError(Exception ex, HttpContext context, bool includeRawRequest, string addInfo)
        {
            try
            {
                return GetWebContextInfo(ex, context, includeRawRequest, addInfo);

            }
            catch (Exception exc)
            {
                //Logger.Error(exc);
                return "";
            }

        }


        private static string GetWebContextInfoJust404(HttpRequest request)
        {
            DateTime errorTime = DateTime.UtcNow;
            StringBuilder sb = new StringBuilder(0x1388);
            sb.Append("Date: " + errorTime.ToString("r") + "\r\n");
            sb.Append("Local Date: " + errorTime.ToLocalTime().ToString("F") + "\r\n");
            sb.Append("Host: " + System.Environment.MachineName + "\r\n");
            sb.Append(string.Format("Url: {0} {1}", request.Method, request.GetEncodedUrl()) + "\r\n");
            string ip = request.HttpContext.Connection.RemoteIpAddress.ToString();
            string hostname = "";
            try
            {
                hostname = Dns.GetHostEntry(ip).HostName;
            }
            catch
            {
            }

            sb.Append(string.Format("Remote Host: {0} {1}", ip, hostname) + "\r\n");


            if (request != null && request.Headers != null)
                if (request.Headers["Referer"] != StringValues.Empty)
                    sb.Append("Referer: " + request.Headers["Referer"].ToString());
            if (request != null && request.GetTypedHeaders().Referer != null)
                if (request.GetTypedHeaders().Referer != null)
                    sb.Append("Referer Server Variable: " + request.GetTypedHeaders().Referer.AbsoluteUri);

            return sb.ToString();
        }


        private static string GetWebContextInfo(Exception ex, HttpContext context, bool includeRawRequest, string addInfo)
        {
            DateTime errorTime = DateTime.UtcNow;
            StringBuilder sb = new StringBuilder(0x1388);
            sb.Append("Date: " + errorTime.ToString("r") + "\r\n");
            sb.Append("Local Date: " + errorTime.ToLocalTime().ToString("F") + "\r\n");
            sb.Append("Host: " + System.Environment.MachineName + "\r\n");
            if (context != null && context.Request != null)
            {
                sb.Append(string.Format("Url: {0} {1}", context.Request.Method, context.Request.GetEncodedUrl()) + "\r\n");
                string hostname = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                try
                {
                    hostname = Dns.GetHostEntry(hostname).HostName;
                }
                catch
                {
                }

                sb.Append(string.Format("Remote Host: {0} {1}", context.Request.HttpContext.Connection.RemoteIpAddress.ToString(), hostname) + "\r\n");
            }
            if (context != null)
            {
                if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
                    sb.Append(string.Format("Logged User: {0}{1}", context.User.Identity.Name, "\r\n"));
                else
                    sb.Append(string.Format("Logged User: {0}{1}", "Not Logged", "\r\n"));
            }
            sb.Append("\r\n\r\n\r\n");

            if (!string.IsNullOrEmpty(addInfo))
            {
                sb.Append("Additional Info:\r\n");
                sb.Append(addInfo); //PrettyPrintUrlData(context.Application[Devmasters.OTR.Messaging.URLData.CONTENT_ITEM_NAME])
            }

            sb.Append("\r\n\r\n\r\n");
            try
            {
                sb.Append("Exception:\r\n");
                sb.Append(PrettyPrintException(ex));

            }
            catch
            {
            }
            sb.Append("\r\n\r\n\r\n");
            if (context != null && context.Request != null)
            {

                try
                {
                    sb.Append("HTTP Headers:\r\n");
                    foreach (string key in context.Request.Headers.Keys)
                    {
                        if (key.ToLower() != "cookie")
                        {
                            sb.Append("   " + key + ": " + context.Request.Headers[key] + "\r\n");
                        }
                    }

                }
                catch
                {
                }
                sb.Append("\r\n\r\n\r\n");
            }
            if (context != null && context.Request != null)
            {
                try
                {
                    sb.Append("POST data:\r\n");
                    foreach (string key in context.Request.Form.Keys)
                    {
                        sb.Append("   " + key + ": " + context.Request.Form[key] + "\r\n");
                    }

                }
                catch
                {
                }
                sb.Append("\r\n\r\n\r\n");
            }


            if (context != null && context.Request != null)
            {
                try
                {
                    sb.Append("HTTP Cookies:\r\n");
                    foreach (string key in context.Request.Cookies.Keys)
                    {
                        sb.Append("   " + key + ": " + context.Request.Cookies[key] + "\r\n");
                    }

                }
                catch
                {
                }
                sb.Append("\r\n\r\n\r\n");
            }

            if (includeRawRequest && context.Request != null)
            {
                sb.AppendLine("---------------------------\r\n");
                sb.AppendLine("RAW REQUEST\r\n\r\n");
                using (var rr = new System.IO.StreamReader(context.Request.Body))
                {
                    sb.AppendLine(rr.ReadToEnd());
                }
            }

            return sb.ToString();
        }

        public static string FormatNameValueCollection(System.Collections.Specialized.NameValueCollection coll)
        {
            if (coll == null || coll.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(512);
            foreach (string key in coll.AllKeys)
            {
                if (key.ToLower() != "cookie")
                {
                    sb.Append(key + "=" + coll[key] + "\r\n");
                }
            }
            return sb.ToString();
        }



        private static string PrettyPrintException(Exception ex)
        {
            StringBuilder sb = new StringBuilder(1024);
            Exception currentEx = ex;
            int exceptionDepth = 0;
            while (currentEx != null)
            {

                sb.Append("  Ex Type: " + currentEx.GetType().ToString() + "\r\n");
                if (currentEx.Message != null)
                    sb.Append("  LogMessage: " + currentEx.Message + "\r\n");

                if (currentEx.TargetSite != null)
                    sb.Append("  In Method: " + currentEx.TargetSite.ToString() + "\r\n");

                if (currentEx.StackTrace != null)
                    sb.Append("  Stack Trace: \r\n" + currentEx.StackTrace + "\r\n");

                currentEx = currentEx.InnerException;
                if (currentEx != null)
                {
                    sb.Append("\r\n\r\n");
                    exceptionDepth++;
                }
            }
            return sb.ToString();
        }


        public static bool DeleteFile(string filename)
        {
            try
            {
                if (System.IO.File.Exists(filename))
                    System.IO.File.Delete(filename);
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(20);
                try
                {
                    System.IO.File.Delete(filename);
                }
                catch (Exception e)
                {
                    //Devmasters.Logger.Info("File " + filename + " cannot be deleted", e);
                    return false;
                }

            }
            return true;

        }

    }

}
