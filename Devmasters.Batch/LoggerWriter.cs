using System;

namespace Devmasters.Batch
{
    public class LoggerWriter
    {

        Devmasters.Logging.Logger logger = null;
        Devmasters.Logging.PriorityLevel level = Logging.PriorityLevel.Debug;
        public LoggerWriter(Devmasters.Logging.Logger logger)
            : this(logger, Logging.PriorityLevel.Debug)
        { }
        public LoggerWriter(Devmasters.Logging.Logger logger, Devmasters.Logging.PriorityLevel level)
        {
            this.logger = logger;
            this.level = level;
        }

        object lockObj = new object();
        public void ProgressWriter(ActionProgressData data)
        {
            string output = string.Format("{0}: {1} {2}  End:{3}",
                    DateTime.Now.ToLongTimeString().PadRight(12),
                    (data.ProcessedItems.ToString() + "/" + data.TotalItems.ToString()).PadRight(data.TotalItems.ToString().Length * 2 + 5),
                    (data.PercentDone / 100f).ToString("P3").PadRight(9),
                    data.EstimatedFinish == DateTime.MinValue ? "" : data.EstimatedFinish.ToString("dd.MM HH:mm:ss.f")
                );
            OutputWriter(output);
        }
        string previousText = string.Empty;

        public void OutputWriter(string data)
        {
            //this.logger.Logger.Log(new log4net.Core.LoggingEvent(new log4net.Core.LoggingEventData()
            //{
            //    Level = this.level,
            //    Message = data,
            //    TimeStamp = DateTime.Now,

            //}));

            if (logger != null)
            {
                this.logger.executeLog(new
                    Logging.LogMessage()
                        .SetLevel(this.level)
                        .SetMessage(data)
                    );
            }
        }

    }
}
