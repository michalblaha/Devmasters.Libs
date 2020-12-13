using System;
using System.Linq;
using System.Collections.Generic;
using Devmasters.SpeechToText;

namespace KaldiASR.SpeechToText
{
    public class VoiceToTerms
    {
        private string mp3file = null;

        private bool converted = false;

        public decimal MinDelayToCreateParagraph = 1.0m;


        public VoiceToTerms(string rawData)
        {
            converted = true;
            this.Raw = rawData;
            this.Terms = RawToTerms(this.Raw);
        }

        private string _raw;
        public string Raw
        {
            get
            {
                if (converted == false)
                    throw new ApplicationException("Start Conversion with Convert() method first.");
                else return _raw;
            }
            private set => _raw = value;
        }


        private IEnumerable<Term> _terms;
        public IEnumerable<Term> Terms
        {
            get
            {
                if (converted == false)
                    throw new ApplicationException("Start Conversion with Convert() method first.");
                else return _terms;
            }

            private set => _terms = value;
        }



        /// <summary>
        /// raw in CTM format http://my.fit.edu/~vkepuska/ece5527/sctk-2.3-rc1/doc/infmts.htm#ctm_fmt_name_0
        /// 
        /// CTM :== <F> <C> <BT> <DUR> word [ <CONF> ]
        /// Where :
        /// <F> ->
        /// The waveform filename. NOTE: no pathnames or extensions are expected.
        /// <C> ->
        /// The waveform channel. Either "A" or "B". The text of the waveform channel is not restricted by sclite. The text can be any text string without witespace so long as the matching string is found in both the reference and hypothesis input files.
        /// <BT> ->
        /// The begin time (seconds) of the word, measured from the start time of the file.
        /// <DUR> ->
        /// The duration (seconds) of the word.
        /// <CONF> ->
        /// Optional confidence score. It is proposed that this score will be used in the future.

        /// </summary>
        /// <param name="rawCTMformat">raw in CTM format http://my.fit.edu/~vkepuska/ece5527/sctk-2.3-rc1/doc/infmts.htm#ctm_fmt_name_0</param>
        /// <returns></returns>
        public static IEnumerable<Term> RawToTerms(string rawCTMformat)
        {
            List<Term> terms = new List<Term>();
            var lines = rawCTMformat.Split('\r', '\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line.Trim()))
                    continue;
                string[] parts = line.Split(' ');

                Term t = new Term();
                t.Character = Term.TermCharacter.Word;
                t.Timestamp = decimal.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                var wlenght = decimal.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                t.Value = parts[4];
                if (parts.Count() > 5)
                    t.Confidence = decimal.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture);

                terms.Add(t);

                //add separator
                terms.Add(new Term()
                {
                    Character = Term.TermCharacter.Separator,
                    Timestamp = t.Timestamp + wlenght,
                    Value=" "
                });
            }

            return terms.OrderBy(m=>m.Timestamp);

        }
    }
}

