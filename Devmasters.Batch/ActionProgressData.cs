using System;

namespace Devmasters.Batch
{
    public class ActionProgressData
    {

        public ActionProgressData(long total, long processedCount, DateTime started, string prefix = null, string postfix = null)
        {
            this.TotalItems = total;
            this.ProcessedItems = processedCount;
            this.StartOfAll = started;
            DateTime now = DateTime.Now;
            if (!string.IsNullOrEmpty(prefix))
                this.Prefix = prefix;
            if (!string.IsNullOrEmpty(postfix))
                this.Postfix = postfix;

            if (ProcessedItems > 0)
            {
                float avgMSecondsPerItem = (float)((now - started).TotalMilliseconds) / (float)ProcessedItems;

                this.EstimatedFinish = now.AddMilliseconds(avgMSecondsPerItem * (this.TotalItems - this.ProcessedItems));
            }
        }
        public float PercentDone
        {
            get
            {
                if (TotalItems == 0)
                    return 1;

                return ((float)this.ProcessedItems / (float)this.TotalItems) * 100f;
            }
        }


        public long ProcessedItems { get; set; }
        public long TotalItems { get; set; }
        public DateTime StartOfAll { get; set; }
        public DateTime EstimatedFinish { get; set; }
        //public DateTime EstimatedFinish_Items { get; set; }

        public string Prefix { get; set; } = string.Empty;
        public string Postfix { get; set; } = string.Empty;
    }
}
