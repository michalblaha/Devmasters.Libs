
/* Unmerged change from project 'Devmasters.Logging (net472)'
Before:
using log4net.Appender;
using log4net.Core;
using System;
After:
using System;
*/
using log4net.Appender;
using log4net.Core;

using log4net.Appender;
using log4net.Core;

namespace Devmasters.Logging
{
    public class NullAppender : AppenderSkeleton
    {
        #region Override implementation of AppenderSkeleton

        /// <summary>
        /// This method is called by the <see cref="AppenderSkeleton.DoAppend(LoggingEvent)"/> method. 
        /// </summary>
        /// <param name="loggingEvent">the event to log</param>
        /// <remarks>
        /// <para>Stores the <paramref name="loggingEvent"/> in the events list.</para>
        /// </remarks>
        override protected void Append(LoggingEvent loggingEvent)
        {
            // Do nothing here...
        }

        #endregion Override implementation of AppenderSkeleton
    }
}
