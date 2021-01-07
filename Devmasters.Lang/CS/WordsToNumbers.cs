using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Devmasters.Lang.CS
{
    public class WordsToNumbers
    {


        public string OriginalText { get; private set; }
        public string ConvertedText { get; private set; }
        public ConversionType Conversion { get; private set; }
        public bool ConvertShortNumbers { get; private set; }

        public TimeSpan CalculationTime { get; private set; }

        public enum ConversionType
        {
            NumberInsteadText,
            NumberAndText,
            TextAndNumber
        }
        public WordsToNumbers(string text)
            : this(text, ConversionType.NumberInsteadText, false)
        { }
        public WordsToNumbers(string text, ConversionType ctype, bool convertShortNumbers)
        {
            this.OriginalText = text;
            Conversion = ctype;
            ConvertShortNumbers = convertShortNumbers;
            convert();
        }

        public void ConvertAgain(ConversionType ctype, bool solitaryNumbers)
        {
            ConvertShortNumbers = solitaryNumbers;
            Conversion = ctype;
            convert();
        }

        private void convert()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Reset();
            sw.Start();
            var converted = (ConvertShortNumbers ? findInTextRegex1 : findInTextRegex2).Replace(
                  this.OriginalText
                  , m =>
                  {
                      if (!string.IsNullOrEmpty(m.Value))
                      {
                          string[] partsTpConvert = new string[] { m.Value };
                          if (Lomeno.Any(l => m.Value.ToLower().RemoveAccents().Contains(l.ToLower().RemoveAccents())))
                          {
                              partsTpConvert = m.Value.SplitWithSeparators(
                                  StringSplitOptions.RemoveEmptyEntries,
                                  StringComparison.CurrentCultureIgnoreCase,
                                  Lomeno
                                  );
                          }

                          string[] partsConverted = new string[partsTpConvert.Length];
                          for (int i = 0; i < partsTpConvert.Length; i++)
                          {
                              var mval = replaceNumbers(partsTpConvert[i]);
                              string dval = "";
                              if (mval == "/")
                                  dval = mval;
                              else
                                  dval = Devmasters.ParseText.ToDecimal(mval, 0).Value.ToString("### ### ### ### ### ##0.##").Trim();

                              var endChar = char.IsWhiteSpace(m.Value.Last()) ? " " : "";

                              if (dval == "/")
                                  partsConverted[i] = $"{dval}{endChar}";
                              else
                              {
                                  switch (partsTpConvert.Length == 1 ? this.Conversion : ConversionType.NumberInsteadText)
                                  {
                                      case ConversionType.NumberInsteadText:
                                          partsConverted[i] = $"{dval}{endChar}";
                                          break;
                                      case ConversionType.NumberAndText:
                                          partsConverted[i] = $"{dval}({partsTpConvert[i]}){endChar}";
                                          break;
                                      case ConversionType.TextAndNumber:
                                      default:
                                          partsConverted[i] = $"{partsTpConvert[i]}({dval}){endChar}";
                                          break;
                                  }
                              }
                          }
                          var res = string.Join("", partsConverted);
                          if (partsTpConvert.Length > 0)
                          {
                              res = Regex.Replace(res, @"\s*/\s*", "/", regexOpt);
                          }
                          return res;
                      }
                      else
                          return m.Value;
                  }

            );
            sw.Stop();
            this.ConvertedText = converted;
            this.CalculationTime = sw.Elapsed;

        }





        // usnesení číslo dvě stě třicet pět lomeno dvacet lomeno dvacet rada 
        //based on https://stackoverflow.com/a/17324733 
        private static string[] Jednotky = { "jedna", "dvaa|dva|dvě", "třia|tři", "čtyřia|čtyři", "pěta|pět", "šesta|šest", "sedma|seduma|sedm", "osma|osuma|osm", "devěta|devět" };
        private static string[] Desitky = { null, "dvacet", "třicet", "čtyřicet", "padesát", "šedesát", "sedmdesát", "osmdesát", "devadesát" };
        private static string[] DesetDvacet = { "deset", "jedenáct", "dvanáct", "třináct", "čtvrnáct", "patnáct", "šestnáct", "sedmnáct", "osmnáct", "devatenáct" };
        private static string[] Rady = { "sto", "tisíce|tisíc", "milionů|miliony|milion", "celých|tečka" };
        private static string[] Nula = { "nula" };
        private static string[] Lomeno = { "lomeno", "/" };
        private static string[] Vyplne = { "set", "stě", "sta" };

        private static Regex findInTextRegex1 = null;
        private static Regex findInTextRegex2 = null;
        private static RegexOptions regexOpt = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;

        static WordsToNumbers()
        {

            var awords = Jednotky.Select(m => m.Split('|')).Where(m => m.Length > 1).Select(m => m[0]);
            List<string> awordsComb = new List<string>();
            foreach (var a in awords)
                foreach (var d in Desitky.Where(m => m != null))
                {
                    awordsComb.Add(a + d);
                }

            var words = Jednotky.Concat(Desitky).Concat(DesetDvacet).Concat(Rady).Concat(Nula).Concat(Lomeno).Concat(Vyplne).Concat(awordsComb)
                .Where(w => !string.IsNullOrEmpty(w))
                .SelectMany(w => w.Split('|'))
                .OrderByDescending(o => o.Length)
                .ToArray();
            var wordsOr = string.Join("|", words);

            var regex1 = $"(({wordsOr}) ($|\\s|[ ,.;\\-()]){{1,}} ){{1,}}";
            var regex2 = $"(({wordsOr}) ($|\\s|[ ,.;\\-()]){{1,}} ){{2,}}";

            findInTextRegex1 = new Regex(regex1, regexOpt);
            findInTextRegex2 = new Regex(regex2, regexOpt);
        }

        private static string[] bestSplitMatch(string source, string splits)
        {
            string[] parts = null;
            foreach (var s in splits.Split('|'))
            {
                parts = source.Split(new string[] { s }, StringSplitOptions.None);
                if (parts.Length > 1)
                    break;
            }
            return parts;
        }
        private static bool bestCompare(string source, string[] jednotky)
        {
            var same = false;
            foreach (var s in jednotky)
            {
                same = bestCompare(source, s);
                if (same)
                    return same;
            }
            return same;
        }

        private static bool bestCompare(string source, string jednotka)
        {
            if (jednotka == null)
                return false;
            foreach (var s in jednotka.Split('|'))
            {
                if (source.RemoveAccents().ToLower() == s.RemoveAccents().ToLower())
                    return true;
            }
            return false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="specificText">text obsahujici jen cisla, nic vic</param>
        /// <returns></returns>
        private static string replaceNumbers(string specificText)
        {
            string result = "";
            string[] decim = bestSplitMatch(specificText, Rady[3]);

            string[] millions = bestSplitMatch(decim[0], Rady[2]);

            for (int i = 0; i < millions.Length; i++)
            {
                string[] thousands = bestSplitMatch(millions[i], Rady[1]);

                for (int j = 0; j < thousands.Length; j++)
                {
                    int[] triplet = { 0, 0, 0 };
                    string[] set = thousands[j].Split(" ,.;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    int currToken = 0;

                    if (set.Length == 1)
                    { //If there is only one token given in triplet
                        if (bestCompare(set[currToken], Lomeno))
                        {
                            result = result + "/";
                            break;
                        }
                        string uno = set[currToken]; currToken++;
                        triplet[0] = 0;
                        for (int k = 0; k < Jednotky.Length; k++)
                        {
                            if (bestCompare(uno, Jednotky[k]))
                            {
                                triplet[1] = 0;
                                triplet[2] = k + 1;
                            }
                            if (bestCompare(uno, Desitky[k]))
                            {
                                triplet[1] = k + 1;
                                triplet[2] = 0;
                            }
                            if (bestCompare(uno, DesetDvacet[k]))
                            {
                                triplet[1] = 1;
                                triplet[2] = k;
                            }
                        }
                    }


                    else if (set.Count() == 2)
                    {  //If there are two tokens given in triplet
                        string uno = set[currToken]; currToken++;
                        string dos = set[currToken]; currToken++;
                        if (dos == Rady[0])
                        {  //If one of the two tokens is "hundred"
                            for (int k = 0; k < Jednotky.Length; k++)
                            {
                                if (bestCompare(uno, DesetDvacet[k]))
                                {
                                    triplet[1] = 1;
                                    triplet[2] = k;
                                }
                                if (bestCompare(uno, Jednotky[k]))
                                {
                                    triplet[0] = k + 1;
                                    triplet[1] = 0;
                                    triplet[2] = 0;
                                }
                            }
                        }
                        else
                        {
                            triplet[0] = 0;
                            for (int k = 0; k < Jednotky.Length; k++)
                            {
                                if (bestCompare(uno, Desitky[k]))
                                {
                                    triplet[1] = k + 1;
                                }
                                if (bestCompare(dos, Jednotky[k]))
                                {
                                    triplet[2] = k + 1;
                                }
                                if (bestCompare(uno, DesetDvacet[k]))
                                {
                                    triplet[1] = 1;
                                    triplet[2] = k;
                                }
                            }
                        }
                    }

                    else if (set.Count() == 3)
                    {  //If there are three tokens given in triplet
                        string uno = set[currToken]; currToken++;
                        string dos = set[currToken]; currToken++;
                        string tres = set[currToken]; currToken++;
                        for (int k = 0; k < Jednotky.Length; k++)
                        {
                            if (bestCompare(uno, Jednotky[k]))
                            {
                                triplet[0] = k + 1;
                            }
                            if (bestCompare(tres, Jednotky[k]))
                            {
                                triplet[1] = 0;
                                triplet[2] = k + 1;
                            }
                            if (bestCompare(tres, Desitky[k]))
                            {
                                triplet[1] = k + 1;
                                triplet[2] = 0;
                            }
                            if (bestCompare(uno, DesetDvacet[k]) || bestCompare(tres, DesetDvacet[k]))
                            {
                                triplet[1] = 1;
                                triplet[2] = k;
                            }

                        }
                    }

                    else if (set.Count() == 4)
                    {  //If there are four tokens given in triplet
                        string uno = set[currToken]; currToken++;
                        string dos = set[currToken]; currToken++;
                        string tres = set[currToken]; currToken++;
                        string cuatro = set[currToken]; currToken++;
                        for (int k = 0; k < Jednotky.Length; k++)
                        {
                            if (bestCompare(uno, Jednotky[k]))
                            {
                                triplet[0] = k + 1;
                            }
                            if (bestCompare(cuatro, Jednotky[k]))
                            {
                                triplet[2] = k + 1;
                            }
                            if (bestCompare(tres, Desitky[k]))
                            {
                                triplet[1] = k + 1;
                            }
                            if (bestCompare(uno, DesetDvacet[k]))
                            {
                                triplet[1] = 1;
                                triplet[2] = k;
                            }
                        }
                    }
                    else
                    {
                        triplet[0] = 0;
                        triplet[1] = 0;
                        triplet[2] = 0;
                    }

                    result = result + triplet[0].ToString() + triplet[1].ToString() + triplet[2].ToString();
                }
            }

            if (decim.Length > 1)
            {  //The number is a decimal
                string[] decimalDigits = decim[1].Split(" ,.;".ToCharArray());
                result = result + ".";
                //System.out.println(decimalDigits.countTokens() + " decimal digits");

                int currToken = 0;

                while (decimalDigits.Length > currToken)
                {
                    string w = decimalDigits[currToken]; currToken++;
                    //System.out.println(w);

                    if (bestCompare(w, Nula))
                    {
                        result = result + "0";
                    }
                    if (bestCompare(w, Lomeno))
                    {
                        result = result + "/";
                    }
                    for (int j = 0; j < Jednotky.Length; j++)
                    {
                        if (w == Jednotky[j])
                        {
                            result = result + (j + 1).ToString();
                        }
                    }

                }
            }

            return result;
        }


    }
}
