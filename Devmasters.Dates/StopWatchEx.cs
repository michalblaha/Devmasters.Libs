using System;
using System.Diagnostics;

namespace Devmasters.DT
{

    public class StopWatchEx : Stopwatch
    {

        public static void ExecutionTime(
        Action codeToExecute,
        Action<StopWatchEx> processResult)
        {
            if (codeToExecute == null)
                throw new ArgumentNullException("codeToExecute");

            var sw = new StopWatchEx();
            sw.Start();
            codeToExecute();
            sw.Stop();
            if (processResult != null)
                processResult(sw);

        }



        /// <summary>
        /// Gets the exact elapsed miliseconds.
        /// </summary>
        /// <value>The exact elapsed miliseconds.</value>
        public double ExactElapsedMiliseconds
        {

            get
            {

                //long ticketsperMs = TimeSpan.TicksPerMillisecond ;

                double d = (double)this.Elapsed.Ticks / (double)TimeSpan.TicksPerMillisecond;

                return Math.Round(d, 5);

            }

        }

    }

}
