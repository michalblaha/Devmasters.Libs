using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Devmasters.Batch
{
    public class Manager
    {

        public static void DoActionForAll<TSource>(
            IEnumerable<TSource> source,
            System.Func<TSource, ActionOutputData> action,
            bool parallel = true, int? maxDegreeOfParallelism = null, System.Action<Exception, TSource> onExceptionAction = null)
        {
            System.Func<TSource, object, ActionOutputData> coreAction = (t, o) =>
            {
                return action(t);
            };

            DoActionForAll<TSource, object>(
                source, coreAction, null,
                DefaultOutputWriter,
                new ActionProgressWriter(0.1f).Write,
                parallel, maxDegreeOfParallelism, onExceptionAction:onExceptionAction);
        }


        public static void DoActionForAll<TSource, TActionParam>(
            IEnumerable<TSource> source,
            System.Func<TSource, TActionParam, ActionOutputData> action,
            TActionParam actionParameters,
            bool parallel = true, int? maxDegreeOfParallelism = null, System.Action<Exception, TSource> onExceptionAction = null)
        {
            DoActionForAll<TSource, TActionParam>(
                source, action, actionParameters,
                DefaultOutputWriter,
                new ActionProgressWriter(0.1f).Write,
                parallel, maxDegreeOfParallelism, onExceptionAction:onExceptionAction);
        }

        public static void DoActionForAll<TSource>(
    IEnumerable<TSource> source,
    System.Func<TSource, ActionOutputData> action,
    System.Action<string> logOutputFunc,
    System.Action<ActionProgressData> progressOutputFunc,
    bool parallel = true,
    int? maxDegreeOfParallelism = null,
    bool cancelOnError = false, string prefix = "", String postfix = "", System.Action<Exception, TSource> onExceptionAction = null)
        {
            System.Func<TSource, object, ActionOutputData> coreAction = (t, o) =>
            {
                return action(t);
            };

            DoActionForAll<TSource, object>(
                source, coreAction, null,
                logOutputFunc,
                progressOutputFunc,
                parallel, maxDegreeOfParallelism, cancelOnError, prefix, postfix, onExceptionAction);

        }

        public static void DoActionForAll<TSource, TActionParam>(
            IEnumerable<TSource> source,
            System.Func<TSource, TActionParam, ActionOutputData> action,
            TActionParam actionParameters,
            System.Action<string> logOutputFunc,
            System.Action<ActionProgressData> progressOutputFunc,
            bool parallel = true,
            int? maxDegreeOfParallelism = null,
            bool cancelOnError = false, string prefix = null, string postfix = null, System.Action<Exception, TSource> onExceptionAction = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (action == null)
                throw new ArgumentNullException("action");

            if (maxDegreeOfParallelism.HasValue && maxDegreeOfParallelism.Value == 1)
                parallel = false;

            DateTime started = DateTime.Now;

            long processedCount = 0;
            long total = source.Count();

            bool canceled = false;

            if (parallel)
            {

                CancellationTokenSource cts = new CancellationTokenSource();
                try
                {
                    ParallelOptions pOptions = new ParallelOptions();
                    if (maxDegreeOfParallelism.HasValue)
                        pOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism.Value;
                    pOptions.CancellationToken = cts.Token;

                    Parallel.ForEach<TSource>(source, pOptions, (value) =>
                    {
                        ActionOutputData cancel = null;
                        try
                        {
                            cancel = action(value, actionParameters);
                            System.Threading.Interlocked.Increment(ref processedCount);
                            if (logOutputFunc != null && !string.IsNullOrEmpty(cancel.Log))
                                logOutputFunc(cancel.Log);

                            if (cancel.CancelRunning)
                                cts.Cancel();
                        }
                        catch (Exception e)
                        {
                            if (onExceptionAction == null)
                                onExceptionAction = (ex,val) 
                                        => Devmasters.Logging.Logger.Root.Error(
                                            $"DoActionForAll paraller action error for {Newtonsoft.Json.JsonConvert.SerializeObject(val)}",
                                            ex);
                            if (onExceptionAction != null)
                                onExceptionAction(e, value);
                            if (cancelOnError)
                                cts.Cancel();
                        }
                        finally
                        {
                            cancel = null;
                            if (progressOutputFunc != null)
                            {

                                ActionProgressData apd = new ActionProgressData(total, processedCount, started, prefix, postfix);
                                progressOutputFunc(apd);
                            }
                        }

                    });
                }
                catch (OperationCanceledException e)
                {
                    //Catestrophic Failure
                    canceled = true;
                }


            }
            else
            {
                foreach (var value in source)
                {
                    ActionOutputData cancel = action(value, actionParameters);
                    try
                    {
                        System.Threading.Interlocked.Increment(ref processedCount);

                        if (logOutputFunc != null && !string.IsNullOrEmpty(cancel.Log))
                            logOutputFunc(cancel.Log);

                        if (cancel.CancelRunning)
                        {
                            canceled = true;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Devmasters.Logging.Logger.Root.Error("DoActionForAll action error", e);
                        if (cancelOnError)
                            break;
                    }
                    finally
                    {
                        cancel = null;
                        if (progressOutputFunc != null)
                        {
                            ActionProgressData apd = new ActionProgressData(total, processedCount, started, prefix, postfix);
                            progressOutputFunc(apd);
                        }
                    }
                }
            }

        }


        public static void DefaultProgressWriter(ActionProgressData data)
        {
            var line = string.Format("{0} {1}: {2}/{3}   {4}%  End:{5} {6}",
                data.Prefix,
                DateTime.Now.ToLongTimeString(), data.ProcessedItems, data.TotalItems, data.PercentDone,
                data.EstimatedFinish == DateTime.MinValue ? "" : data.EstimatedFinish.ToString("dd.MM HH:mm:ss.f"),
                data.Postfix
                );


            Console.WriteLine(
                "\n" + line.Trim()
                );
        }
        public static void DefaultOutputWriter(string data)
        {
            if (!string.IsNullOrEmpty(data))
                Console.Write(data);
        }



    }
}
