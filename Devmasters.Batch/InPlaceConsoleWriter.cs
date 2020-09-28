using System;

namespace Devmasters.Batch
{
    public class InPlaceConsoleWriter
    {

        object lockObj = new object();
        public void DefaultActionProgressFunction(ActionProgressData data)
        {
            string output = string.Format("{0}: {1} {2}  End:{3}",
                    DateTime.Now.ToLongTimeString().PadRight(12),
                    (data.ProcessedItems.ToString() + "/" + data.TotalItems.ToString()).PadRight(data.TotalItems.ToString().Length * 2 + 5),
                    (data.PercentDone / 100f).ToString("P3").PadRight(9),
                    data.EstimatedFinish == DateTime.MinValue ? "" : data.EstimatedFinish.ToString("dd.MM HH:mm:ss.f")
                );
            DefaultActionOutputFunction(output);
        }
        string previousText = string.Empty;

        public void DefaultActionOutputFunction(string data)
        {
            lock (lockObj)
            {
                if (!string.IsNullOrEmpty(previousText))
                {
                    Console.Write(new string('\b', previousText.Length));
                }
                previousText = data;

                if (!string.IsNullOrEmpty(data))
                    Console.Write(data);
            }
        }

    }
}
