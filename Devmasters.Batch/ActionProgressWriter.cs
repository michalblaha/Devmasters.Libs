using System;

namespace Devmasters.Batch
{
    public class ActionProgressWriter
    {
        const float automaticMinPercentChange = float.MinValue;
        private float previousProgressValue = 0;
        private long previousProcessedItems = 0;

        public float MinPercentChange { get; set; } = 0;
        private int minProcessedItems = 0;
        private System.Action<ActionProgressData> outputFunc = Manager.DefaultProgressWriter;
        public ActionProgressWriter()
        {
            this.MinPercentChange = automaticMinPercentChange;
        }

        public ActionProgressWriter(System.Action<ActionProgressData> outputFunc = null)
        {
            if (outputFunc != null)
                this.outputFunc = outputFunc;
        }
        public ActionProgressWriter(float minPercentChange, System.Action<ActionProgressData> outputFunc = null)
            : this(outputFunc)
        {
            this.MinPercentChange = minPercentChange;
        }
        public ActionProgressWriter(int minProcessedItems, System.Action<ActionProgressData> outputFunc = null)
            : this(outputFunc)
        {
            this.minProcessedItems = minProcessedItems;
        }



        public void Write(ActionProgressData data)
        {

            float value = data.PercentDone;

            if (this.MinPercentChange == automaticMinPercentChange)
            {
                if (data.TotalItems != 0)
                {
                    this.MinPercentChange = Math.Max(0.1f, 10f / (float)data.TotalItems);
                }
                else
                    this.MinPercentChange = 1f;
                if (this.MinPercentChange > 50f)
                {
                    this.MinPercentChange = 10f;
                }
            }

            if (MinPercentChange > 0)
            {
                if (Math.Abs(value - previousProgressValue) > MinPercentChange)
                {
                    this.previousProgressValue = value;
                    this.outputFunc(data);
                }
            }
            else if (minProcessedItems > 0)
            {
                if (Math.Abs(data.ProcessedItems - previousProcessedItems) >= minProcessedItems)
                {
                    this.previousProcessedItems = data.ProcessedItems;

                    this.outputFunc(data);
                }
            }
            else
                this.outputFunc(data);



        }

    }
}
