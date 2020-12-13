using System;
using System.Linq;
using System.Collections.Generic;

namespace Devmasters.SpeechToText
{
    public class VoiceToTextFormatter
    {
        private string mp3file = null;

        private bool converted = false;

        public decimal MinDelayToCreateParagraph = 1.0m;


        public VoiceToTextFormatter(IEnumerable<Term> terms)
        {
            this.Terms = terms;
        }



        private IEnumerable<Term> _terms;
        public IEnumerable<Term> Terms
        {
            get
            {
                return _terms;
            }

            private set => _terms = value;
        }

        public string Text(bool withParagraphs)
        {
            return TextWithTimestampsToText(
                TermsToTextWithTimestamps(this.Terms, TimeSpan.FromMinutes(1), withParagraphs, this.MinDelayToCreateParagraph)
                );

        }

        public List<TextWithTimestamp> TextWithTimestamps(TimeSpan timestampInterval, bool withParagraphs)
        {
            return TermsToTextWithTimestamps(this.Terms, timestampInterval, withParagraphs, this.MinDelayToCreateParagraph);
        }
        public static string TextWithTimestampsToText(List<TextWithTimestamp> withTimestamps)
        {
            return withTimestamps
                .Select(m => m.Text)
                .Aggregate((f, s) => f + s);
        }

        public static List<TextWithTimestamp> TermsToTextWithTimestamps(IEnumerable<Term> terms, TimeSpan timestampInterval, bool withParagraphs = false, decimal minDelayToCreateParagraph = 1.0m)
        {
            List<TextWithTimestamp> res = new List<TextWithTimestamp>();
            TimeSpan prevStart = TimeSpan.Zero;
            var lTerms = terms.OrderBy(m=>m.Timestamp).ToList();
            System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);

            TimeSpan floatingTSinterval = timestampInterval;

            for (int i = 0; i < lTerms.Count; i++)
            {

                Term t = lTerms[i];

                if ((t.TimestampInTS - prevStart) >= floatingTSinterval)
                {
                    res.Add(new TextWithTimestamp()
                    {
                        Start = prevStart,
                        Text = sb.ToString().ReplaceDuplicates("\n").ReplaceDuplicates("\r").ReplaceDuplicates("\r\n").ReplaceDuplicates(" ")
                    }
                        );
                    floatingTSinterval = new TimeSpan(timestampInterval.Ticks - ((t.TimestampInTS - prevStart) - floatingTSinterval).Ticks); //zkrat dalsi, pokud predchozi pretahnul
                    prevStart = t.TimestampInTS;
                    sb.Clear();
                }

                switch (t.Character)
                {
                    case Term.TermCharacter.Separator:
                        sb.Append(t.Value);
                        break;
                    case Term.TermCharacter.Noise:
                        if (withParagraphs)
                        {
                            var nl = NoiseLength(lTerms, i);
                            if (nl > minDelayToCreateParagraph)
                                sb.AppendLine();
                        }
                        break;
                    case Term.TermCharacter.Word:
                        if (withParagraphs)
                        {
                            var delay = 0m;
                            if (i > 0)
                                delay = t.Timestamp - lTerms[i - 1].Timestamp;

                            if (delay > minDelayToCreateParagraph) //ticho mezi slovy
                                sb.AppendLine();
                        }
                        sb.Append(t.Value);

                        break;
                    case Term.TermCharacter.SpeakersNoise:
                    default:
                        break;
                }


            }

            if (sb.Length > 0)
                res.Add(new TextWithTimestamp()
                {
                    Start = prevStart,
                    Text = sb.ToString().ReplaceDuplicates("\n").ReplaceDuplicates("\r").ReplaceDuplicates("\r\n").ReplaceDuplicates(" ")
                }
                );


            return res;
        }
        public class TextWithTimestamp
        {
            public TimeSpan Start { get; set; }
            public string Text { get; set; }
        }


        private static decimal NoiseLength(List<Term> terms, int forItemNum)
        {
            if (forItemNum == 0)
                return terms[forItemNum].Timestamp;

            decimal noiseEnd = terms[forItemNum].Timestamp;
            if (terms.Count > forItemNum + 1)
                noiseEnd = terms[forItemNum + 1].Timestamp;

            decimal noiseStart = terms[forItemNum].Timestamp;
            for (int i = forItemNum - 1; i > 0; i--)
            {
                if (terms[i].Character == Term.TermCharacter.Noise
                    //|| terms[i].Character == Term.TermCharacter.SpeakersNoise
                    )
                    noiseStart = terms[i].Timestamp;
                else
                    break;
            }


            return noiseEnd - noiseStart;
        }



    }
}

