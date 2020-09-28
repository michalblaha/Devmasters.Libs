using System;

/* Unmerged change from project 'Devmasters.Logging (net472)'
Before:
using System.Linq;
After:
using System.Collections;
*/
using System.Collections.Generic;

/* Unmerged change from project 'Devmasters.Logging (net472)'
Before:
using System.Text;
using System.Diagnostics;
After:
using System.Diagnostics;
using System.IO;
*/
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

/* Unmerged change from project 'Devmasters.Logging (net472)'
Before:
using System.Collections;
using System.Web;
using System.Net;
using System.Net.Mail;
After:
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web;
*/
using System.Text;


namespace Devmasters.Logging
{

    public enum PriorityLevel
    {
        Fatal = 0,
        Error = 1,
        Warning = 2,
        Information = 3,
        Debug = 4,
    }

    /// <summary>
    /// Log events of application with Log4net library
    /// </summary>
    public class Logger
    {
        private const string CONFIG_FILENAME = "Logger.log4net";

        public static Logger Root = new Logger("Devmasters");
        public static Logger PageTimes = new Logger("PageTimes");
        public static Logger DevmastersCore = new Logger("Devmasters.Core");

        public enum CallingMethodDetail
        {
            None,
            //LastMethod,
            FullStack
        }

        static Logger()
        {
            //string rootDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location ) + @"\";

            string rootDir = AppDomain.CurrentDomain.BaseDirectory;
            string configFilename = rootDir;
            if (!configFilename.EndsWith(@"\"))
                configFilename += @"\";

            if (!string.IsNullOrEmpty(GetWebConfigValue("Logger.Config.Filename")))
            {
                configFilename = configFilename + GetWebConfigValue("Logger.Config.Filename");
            }
            else
                configFilename = configFilename + CONFIG_FILENAME;

            if (System.IO.File.Exists(configFilename))
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(log4net.LogManager.GetRepository(Assembly.GetCallingAssembly()), new System.IO.FileInfo(configFilename));
            }
            else
            {
                log4net.Config.BasicConfigurator.Configure(log4net.LogManager.GetRepository(Assembly.GetCallingAssembly()));
                //throw new ApplicationException(configFilename + " was not found!");
            }
        }

                private static string GetWebConfigValue(string value)
        {
            string @out = System.Configuration.ConfigurationManager.AppSettings[value];
            if (@out == null)
            {
                @out = string.Empty;
            }
            return @out;
        }

        protected log4net.ILog logger = null;
        public CallingMethodDetail AddCallingMethod { get; set; }

        public Logger(string nameOfLogger)
        {
            logger = log4net.LogManager.GetLogger(Assembly.GetCallingAssembly(), nameOfLogger);
            AddCallingMethod = CallingMethodDetail.None;
        }


        /// <summary>
        /// Returns name of method, from which is this method called.
        /// </summary>
        /// <returns></returns>
        public static string GetCallingMethod(bool showFullStack)
        {
            int startFrame = 1;
            StackFrame stackframe = null;
            StackFrame prevStackframe = null;
            StringBuilder sb = new StringBuilder(1024);
            do
            {
                if (stackframe != null)
                    prevStackframe = stackframe;
                stackframe = new StackFrame(startFrame, true);
                if (showFullStack)
                    sb.Append(FormatStackFrame(stackframe));
                startFrame++;
            } while (stackframe.GetMethod() != null); //&& stackframe.GetMethod().ReflectedType.FullName == "Devmasters.Logger");
            if (IsStackframeNull(stackframe) && IsStackframeNull(prevStackframe) == false)
                stackframe = prevStackframe;
            if (IsStackframeNull(stackframe) == false)
            {
                sb.Append(FormatStackFrame(stackframe));
                if (showFullStack)
                    do
                    {
                        stackframe = new StackFrame(startFrame, true);
                        sb.Append(FormatStackFrame(stackframe));
                        startFrame++;
                    } while (stackframe.GetMethod() != null);
                return sb.ToString();
            }
            else
                return "Unknown method";
        }

        private static bool IsStackframeNull(StackFrame sf)
        {
            if (sf == null)
                return true;
            else if (sf.ToString() == "null\r\n")
                return true;
            else
                return false;
        }

        private static string FormatStackFrame(StackFrame stackframe)
        {
            if (stackframe == null || stackframe.GetMethod() == null)
                return string.Empty;
            else
                return (
                    string.Format("{0}.{1} (line {2}, col {3} in {4})\n",
                        stackframe.GetMethod().ReflectedType.FullName,
                        stackframe.GetMethod().Name,
                        stackframe.GetFileLineNumber().ToString(),
                        stackframe.GetFileColumnNumber(),
                        stackframe.GetFileName()
                        )
                    );
            //return stackframe.ToString();

        }


        private LogMessage addStack(LogMessage msg)
        {
            switch (this.AddCallingMethod)
            {
                case CallingMethodDetail.FullStack:
                    return msg.SetStack(GetCallingMethod(true));
                //case CallingMethodDetail.LastMethod:
                //    return msg.SetStack(GetCallingMethod(false));
                case CallingMethodDetail.None:
                default:
                    return msg;
            }

        }




        /*
         2015-04-03 14:16:30,194 [104] FATAL : Worker UnhandledException
System.Net.WebException: The operation has timed out
   at System.Net.HttpWebRequest.EndGetResponse(IAsyncResult asyncResult)
   at log4net.ElasticSearch.WebElasticClient.FinishGetResponse(IAsyncResult result) in d:\uriel\Programing\C#\log4stash\src\log4net.ElasticSearch\ElasticClient.cs:line 71
   at System.Net.LazyAsyncResult.Complete(IntPtr userToken)
   at System.Net.ContextAwareResult.CompleteCallback(Object state)
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
   at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
   at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Net.ContextAwareResult.Complete(IntPtr userToken)
   at System.Net.LazyAsyncResult.ProtectedInvokeCallback(Object result, IntPtr userToken)
   at System.Net.HttpWebRequest.Abort(Exception exception, Int32 abortState)
   at System.Net.HttpWebRequest.AbortWrapper(Object context)
   at System.Threading.QueueUserWorkItemCallback.System.Threading.IThreadPoolWorkItem.ExecuteWorkItem()
   at System.Threading.ThreadPoolWorkQueue.Dispatch()
   at System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()

         
         */

        string[] attributesToSkip = new string[] { "message", "level", "exception" };
        object objLock = new object();
        public void executeLog(LogMessage msg)
        {
            try
            {

                msg = addStack(msg);
                lock (objLock)
                {
                    List<IDisposable> pushes = new List<IDisposable>();
                    foreach (var kv in msg.ToList())
                    {
                        if (!attributesToSkip.Contains(kv.Key))
                            pushes.Add(log4net.ThreadContext.Stacks[kv.Key].Push(kv.Value.ToString()));
                    }

                    switch (msg.Level)
                    {
                        case PriorityLevel.Fatal:
                            logger.Fatal(msg.Message, msg.Exception);
                            break;
                        case PriorityLevel.Error:
                            logger.Error(msg.Message, msg.Exception);
                            break;
                        case PriorityLevel.Warning:
                            logger.Warn(msg.Message, msg.Exception);
                            break;
                        case PriorityLevel.Information:
                            logger.Info(msg.Message, msg.Exception);
                            break;
                        case PriorityLevel.Debug:
                            logger.Debug(msg.Message, msg.Exception);
                            break;
                        default:
                            break;
                    }
                    pushes.ForEach(a => a.Dispose());

                }
            }
            catch (Exception)
            {

                //throw;
                //eat it
            }

        }

        /// <summary>
        /// Logs error in application
        /// </summary>
        /// <param name="mess"></param>
        /// <param name="ex"></param>
        public void Fatal(LogMessage msg)
        {
            executeLog(msg.SetLevel(PriorityLevel.Fatal));
        }
        public void Fatal(string message, Exception ex = null, Dictionary<string, object> customValues = null)
        {
            Fatal(new LogMessage()
                    .SetMessage(message)
                    .SetException(ex)
                    .SetCustomKeyValues(customValues)
                );
        }

        public void Error(LogMessage msg)
        {
            executeLog(msg.SetLevel(PriorityLevel.Error));
        }
        public void Error(string message, Exception ex = null, Dictionary<string, object> customValues = null)
        {
            Error(new LogMessage()
                    .SetMessage(message)
                    .SetException(ex)
                    .SetCustomKeyValues(customValues)
                );
        }

        public void Warning(LogMessage msg)
        {
            executeLog(msg.SetLevel(PriorityLevel.Warning));
        }
        public void Warning(string message, Exception ex = null, Dictionary<string, object> customValues = null)
        {
            Warning(new LogMessage()
                    .SetMessage(message)
                    .SetException(ex)
                    .SetCustomKeyValues(customValues)
                );
        }

        public void Info(LogMessage msg)
        {
            executeLog(msg.SetLevel(PriorityLevel.Information));
        }
        public void Info(string message, Exception ex = null, Dictionary<string, object> customValues = null)
        {
            Info(new LogMessage()
                    .SetMessage(message)
                    .SetException(ex)
                    .SetCustomKeyValues(customValues)
                );
        }

        public void Debug(LogMessage msg)
        {
            executeLog(msg.SetLevel(PriorityLevel.Debug));
        }
        public void Debug(string message, Exception ex = null, Dictionary<string, object> customValues = null)
        {
            Debug(new LogMessage()
                    .SetMessage(message)
                    .SetException(ex)
                    .SetCustomKeyValues(customValues)
                );
        }





    }
}
