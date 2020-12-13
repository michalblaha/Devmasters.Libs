using System;
using System.Collections.Generic;
using System.Text;

namespace Devmasters.SpeechToText
{
    public static class Extensions
    {

        public static string ToText(this IEnumerable<Term> terms, bool withParagraphs = false)
        {
            var vtt = new VoiceToTextFormatter(terms);
            return vtt.Text(withParagraphs);
        }
        public static List<VoiceToTextFormatter.TextWithTimestamp> ToTextWithTimestamps(this IEnumerable<Term> terms, TimeSpan timestampInterval, bool withParagraphs = false)
        {
            var vtt = new VoiceToTextFormatter(terms);
            return vtt.TextWithTimestamps( timestampInterval, withParagraphs);
        }
    }
}
