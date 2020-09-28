using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Devmasters
{
    public static class TextUtil
    {

        private const string RANDOMCHARACTERS = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static string DEFAULTCOLLECTIONDIVIDER = "|";
        public static string Space = " ";


        private static CultureInfo[] scis = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
        public static CultureInfo USCulture = new System.Globalization.CultureInfo("en-US");
        private static System.Collections.Generic.Dictionary<string, string> HTMLEntities = new System.Collections.Generic.Dictionary<string, string>();
        private static object lockObj = new object();

        static TextUtil()
        {
            FillHTMLEntityTable();
        }


        public static string Reverse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            System.Text.StringBuilder sb = new StringBuilder(text.Length);
            for (int i = text.Length - 1; i >= 0; i--)
            {
                sb.Append(text[i]);
            }
            return sb.ToString();
        }

        static string crlf = cr + lf;
        static string br = @"<br>";
        public static string FormatPlainTextForArticle(string plainText)
        {
            string lf = "\n";
            string cr = "\r";
            string crlf = cr + lf;
            string br = @"<br>";
            string newBody;
            newBody = RemoveHTML(plainText);
            newBody = ReplaceCRFL(newBody, br);
            if (newBody.Length > 5)
            {
                int pos = 0;
                while (newBody.IndexOf(br + br + br, pos) > -1)
                {
                    newBody = newBody.Replace(br + br + br, br + br);
                }
            }
            return newBody;
        }
        public static string NormalizeToURL(string title, string body)
        {
            string done = string.Empty;
            if (title.Trim().Length > 0)
            {
                done = NormalizeToURL(ShortenText(title, 45));
            }
            if (done.Length == 0)
            {
                done = NormalizeToURL(ShortenText(body, 45));
            }

            return done;


        }
        public static string NormalizeToURL(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            s = RemoveDiacritics(s).ToLower();
            s = Regex.Replace(s, "[^a-z0-9]", "-");
            s = s.Trim(new char[] { '-' });
            while (s.IndexOf("--") > -1)
            {
                s = s.Replace("--", "-");
            }
            return s;
        }

        public static string ShortenText(string sText, int Len)
        {
            return ShortenText(sText, Len, " ", string.Empty);
        }
        public static string ShortenText(string sText, int Len, string afterShorten)
        {
            return ShortenText(sText, Len, " ", afterShorten);
        }

        public static string ShortenText(string sText, int Len, string Delimiter, string afterShorten)
        {
            if (string.IsNullOrEmpty(sText))
                return string.Empty;
            if (sText.Length > Len)
            {
                string res = sText.Substring(0, Len);
                if (Delimiter.Length > 0)
                {
                    if (sText.Substring(Len, 1) == Delimiter)
                    {
                        res = sText.Substring(0, Len);
                    }
                    else
                    {
                        int iTmp = sText.LastIndexOf(Delimiter, Len, StringComparison.InvariantCulture);
                        if (iTmp > 0)
                        {
                            res = sText.Substring(0, iTmp);
                        }
                    }
                }

                if (afterShorten.Length > 0)
                    res += afterShorten;

                return res;
            }
            return sText;
        }


        public static string ReplaceDuplicates(string text, char duplicated)
        { return ReplaceDuplicates(text, duplicated, duplicated); }

        public static string ReplaceDuplicates(string text, char duplicated, char replacement)
        {
            return ReplaceDuplicates(text, duplicated, replacement, 2);
        }
        public static string ReplaceDuplicates(string text, char duplicated, char replacement, int minNumOfOccurences)
        {
            string regex = string.Format("([{0}]{{{1},}})", duplicated, minNumOfOccurences);
            return Regex.Replace(text, regex, replacement.ToString());
        }

        public static string ReplaceDuplicates(string text, string duplicatedRegexReady)
        { return ReplaceDuplicates(text, duplicatedRegexReady, duplicatedRegexReady); }

        public static string ReplaceDuplicates(string text, string duplicatedRegexReady, string replacement)
        {
            return ReplaceDuplicates(text, duplicatedRegexReady, replacement, 2);
        }

        public static string ReplaceDuplicates(string text, string duplicatedRegexReady, string replacement, int minNumOfOccurences)
        {
            string regex = string.Format("(({0}){{{1},}})", duplicatedRegexReady, minNumOfOccurences);
            return Regex.Replace(text, regex, replacement);
        }

        public static string NormalizeToPureTextLower(this string s)
        {
            return normalizeToPureTextLower(s);
        }
        private static string normalizeToPureTextLower(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return ReplaceDuplicates(
                System.Text.RegularExpressions.Regex.Replace(RemoveDiacritics(s).ToLower(), @"[^a-z0-9\- ]", " "),
                " ");
        }


        public static string NormalizeToNumbersOnly(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(RemoveDiacritics(s).ToLower(), @"[^0-9\-.,]", string.Empty);
        }
        public static string NormalizeToDateTime(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(RemoveDiacritics(s).ToLower(), @"[^0-9\-:.,]", string.Empty);
        }

        //public static string RemoveDiacritics(this string s)
        //{
        //    return removeDiacritics(s);
        //}
        public static string RemoveDiacritics(string s)
        {
            return removeDiacritics(s);
        }

        private static string removeDiacritics(string s)
        {
            if (s == null)
                return null;
            if (s.Length == 0)
                return string.Empty;
            s = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(s[i]) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }

        public static string NormalizeToBlockText(string sText)
        {
            if (string.IsNullOrEmpty(sText))
                return sText;

            sText = RemoveHTML(sText);

            //remove strange

            sText = ReplaceCRFL(sText, Space);

            sText = sText.Replace("\f", Space);
            sText = sText.Replace("\r", Space);
            sText = sText.Replace("\t", Space);


            return ReplaceDuplicates(sText, ' ').Trim();

        }

        public static string ShortenHTML(string sText, int Len, string[] AllowedTags)
        {
            return ShortenHTML(sText, Len, AllowedTags, " ", "...");
        }


        public static string ShortenHTML(string sText, int Len, string[] AllowedTags, string Delimiter)
        {
            return ShortenHTML(sText, Len, AllowedTags, Delimiter, "...");
        }


        public static string RemoveHTML(string sText)
        {
            if (sText == null)
                return null;
            sText = Regex.Replace(sText, "<br\\s?/?>", " ", RegexOptions.IgnoreCase);
            return ReplaceHTMLEntities(Regex.Replace(sText, "<[^>]*>", string.Empty));
        }


        public static string ShortenHTML(string sText, int Len, string[] AllowedTags, string Delimiter, string AfterShort)
        {
            if (string.IsNullOrEmpty(sText))
                return string.Empty;

            foreach (string s in AllowedTags)
            {
                sText = sText.Replace("<" + s.ToLower() + ">", "|" + s.ToLower() + ">"); //<i> -> |i>
                sText = sText.Replace("<" + s.ToLower() + " ", "|" + s.ToLower() + " "); //<li ...> -> |li ....>
                sText = sText.Replace("</" + s.ToLower() + ">", "|/" + s.ToLower() + ">");

                sText = sText.Replace("<" + s.ToUpper() + ">", "|" + s.ToLower() + ">");
                sText = sText.Replace("<" + s.ToUpper() + " ", "|" + s.ToLower() + " ");
                sText = sText.Replace("</" + s.ToUpper(), "|/" + s.ToLower());
            }
            sText = RemoveHTML(sText);
            foreach (string s in AllowedTags)
            {
                sText = sText.Replace("|" + s.ToLower(), "<" + s);
                sText = sText.Replace("|/" + s.ToLower(), "</" + s);
            }
            if (sText.Length > Len)
            {
                string sShortText = string.Empty;
                bool bFirstTagOpen = false;
                bool bSecondTagOpen = false;
                bool bTagOpen = false;
                int _Vb_t_i4_2 = Len - 1;
                for (int i = 0; i <= _Vb_t_i4_2; i++)
                {
                    string sBuff = string.Empty;
                    string sChar = sText.Substring(i, 1);
                    if ((sChar == "<") & !bFirstTagOpen)
                    {
                        bFirstTagOpen = true;
                        if ((sText.Length > (i + 4)) && (sText.Substring(i, 5).ToLower() == "<img "))
                        {
                            bFirstTagOpen = false;
                            bTagOpen = true;
                            bSecondTagOpen = true;
                        }
                    }
                    if ((sChar == "<") & bTagOpen)
                    {
                        bSecondTagOpen = true;
                    }
                    if ((sChar == ">") & bFirstTagOpen)
                    {
                        bFirstTagOpen = false;
                        bTagOpen = true;
                    }
                    if ((sChar == ">") & bTagOpen)
                    {
                        bFirstTagOpen = false;
                        bTagOpen = false;
                        bSecondTagOpen = false;
                        sShortText = sShortText + sBuff;
                        sBuff = string.Empty;
                    }
                    if (bTagOpen)
                    {
                        sBuff = sBuff + sChar;
                    }
                    else
                    {
                        sShortText = sShortText + sChar;
                    }

                    System.Diagnostics.Debug.Write(bSecondTagOpen);
                }
                if (Delimiter.Length > 0)
                {
                    if (sShortText.EndsWith(Delimiter))
                    {
                        return (sShortText + AfterShort);
                    }
                    int iTmp = sShortText.LastIndexOf(Delimiter, StringComparison.InvariantCulture);
                    if (iTmp > 0)
                    {
                        return (sShortText.Substring(0, iTmp) + AfterShort);
                    }
                }
                return (sShortText + AfterShort);
            }
            return sText;
        }




        /// <summary>
        /// returns random string of specified length usign letters from text
        /// </summary>
        private static string genRandomString(Random random, string text, int length)
        {
            length = length > 0 ? length : text.Length;

            var builder = new StringBuilder();

            for (int i = 0; i < length; i++)
                builder.Append(text[random.Next(text.Length)]);

            return builder.ToString();
        }
        public static string GenRandomString(string choosefromCharacters, int length)
        {
            return genRandomString(Core.Rnd, choosefromCharacters, length);

        }
        public static string GenRandomString(int length)
        {
            return GenRandomString(RANDOMCHARACTERS, length);
        }

        public static double Rnd()
        {
            return Core.Rnd.NextDouble();
        }


        public static bool IsNumeric(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            bool containsNonNumericChars = Regex.IsMatch(text, @"[^0-9\-.,]", RegexOptions.IgnoreCase);

            return containsNonNumericChars == false;
        }

        public static bool IsASCIIText(string text)
        {
            foreach (char c in text)
            {
                int ci = (int)c;
                if (ci > 127 || (ci < 32 && !(ci == 9 || ci == 10 || ci == 13)))
                    return false;
            }
            return true;
        }
        public static CultureInfo GetNeutralCulture(CultureInfo culture)
        {
            return GetNeutralCulture(culture, CultureInfo.GetCultureInfo("en"));
        }

        public static CultureInfo GetNeutralCulture(CultureInfo culture, CultureInfo defaultCultureInfo)
        {
            if (culture.IsNeutralCulture == false)
                return culture.Parent;
            else
                return culture;

        }


        public static CultureInfo GetSpecificCulture(CultureInfo culture)
        {
            return GetSpecificCulture(culture, CultureInfo.GetCultureInfo("en-US"));
        }

        public static CultureInfo GetSpecificCulture(CultureInfo culture, CultureInfo defaultCultureInfo)
        {
            if (culture.IsNeutralCulture)
            {
                foreach (CultureInfo ci in scis)
                {
                    if (ci.Parent.LCID == culture.LCID)
                    {
                        return ci;
                    }
                }
                return defaultCultureInfo;
            }
            return culture;
        }

        public static List<CultureInfo> GetSpecificCultures(CultureInfo culture)
        {
            List<CultureInfo> cultures = new List<CultureInfo>();
            if (culture.IsNeutralCulture)
            {
                foreach (CultureInfo ci in scis)
                {
                    if (ci.Parent.LCID == culture.LCID)
                    {
                        cultures.Add(ci);
                    }
                }
            }
            return cultures;
        }

        public static string NiceTimeSpan(DateTime date1, DateTime date2)
        {
            throw new NotImplementedException();

            //			int sDay = date1.Day;
            //			int eDay = date2.Day;
            //			int sMonth = date1.Month;
            //			int eMonth = date2.Month;
            //			int sYear = date1.Year;
            //			int eYear = date2.Year;
            //
            //			int tYears = eYear - sYear;
            //			int tMonths;
            //			int tDays;
            //
            //			if (eMonth >= sMonth)
            //				tMonths = eMonth - sMonth;
            //			else
            //			{
            //				tYears -= 1;
            //				tMonths = (12 - sMonth) + eMonth;
            //			}
            //
            //			if (eDay >= sDay)
            //				tDays = eDay - sDay;
            //			else
            //			{
            //				tMonths -= 1;
            //				tDays = (DateTime.DaysInMonth(sYear, sMonth) - sDay) + eDay;
            //			}
            //
            //			if (date1.Hour < date1.Hour)
            //			{
            //				// Not a full day so subtract fom the calculated days
            //				tDays--;
            //
            //			}
        }
        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[a-z0-9_\.-]+@[a-z0-9\._-]+\.[a-z]{2,6}$", RegexOptions.IgnoreCase);
        }


        public static string InsertSpacesBeforeUpperCase(string value)
        {
            const string PATTERN = @"(?!^)([A-Z])(?<! [A-Z])";

            Match match;
            do
            {
                match = Regex.Match(value, PATTERN);
                if (!match.Success)
                    break;

                value = value.Replace(match.Value, String.Concat(" ", match.Value));

            } while (match.Success);

            return value;
        }

        public static int? ConvertToInt(string value, int? valueIfNull)
        {
            int val;
            return int.TryParse(value, out val) ? val : valueIfNull;
        }

        public static string EncodeJsString(string value)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in value)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;

                    case '\\':
                        sb.Append("\\\\");
                        break;

                    case '\b':
                        sb.Append("\\b");
                        break;

                    case '\f':
                        sb.Append("\\f");
                        break;

                    case '\n':
                        sb.Append("\\n");
                        break;

                    case '\r':
                        sb.Append("\\r");
                        break;

                    case '\'':
                        sb.Append("\\'");
                        break;

                    case '\t':
                        sb.Append("\\t");
                        break;

                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        public static string IfEmpty(string value, string nullvalue)
        {
            return (String.IsNullOrEmpty(value)) ? nullvalue : value;
        }

        public static string Capitalize(this string instance)
        {
            if (String.IsNullOrEmpty(instance))
                return String.Empty;

            return instance.Substring(0, 1).ToUpper() + instance.Substring(1).ToLower();
        }



        static string lf = "\n";
        static string cr = "\r";


        static Regex findEntityRegex = new Regex("(&[a-zA-Z0-9#]*;)", RegexOptions.Compiled);
        public static string ReplaceHTMLEntities(string text)
        {
            MatchEvaluator myEvaluator = new MatchEvaluator(ReplaceHTMLEntity);
            return findEntityRegex.Replace(text, myEvaluator);
        }

        private static string ReplaceHTMLEntity(Match m)
        {
            if (m.Success)
            {
                if (m.Value.Contains("#"))
                {
                    int? ascii = ConvertToInt(m.Value.Substring(2, m.Value.Length - 3), 0);
                    if (ascii > 31 && ascii < 8192)
                        return Convert.ToChar(ascii).ToString();
                }
                else
                {
                    string val = m.Value;

                    if (HTMLEntities.ContainsKey(m.Value))
                        return HTMLEntities[m.Value];
                    else
                    {
                        val = val.ToLower();
                        if (HTMLEntities.ContainsKey(val))
                            return HTMLEntities[val];
                    }
                }
            }
            return m.Value;
        }

        public static string ReplaceCRFL(string text, string newValue)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (newValue == null)
                throw new ArgumentNullException("newValue");

            if (text.Contains(crlf))
            {
                text = text.Replace(crlf, newValue);
            }
            if (text.Contains(cr))
            {
                text = text.Replace(cr, newValue);
            }
            if (text.Contains(lf))
            {
                text = text.Replace(lf, newValue);
            }
            return text;
        }

        //prepare HTML Filter , filter dangerous HTML elements        
        public static string FormatHTMLForArticle(string data, string[] allowedHtmlTags)
        {
            int AllowedRepetition = 2;

            if (string.IsNullOrEmpty(data))
                return string.Empty;

            string text = ReplaceCRFL(data, " "); ;
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            //validate and fix HTML
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(text);

            //remove unnecessary attributes
            if (doc != null && doc.DocumentNode != null && doc.DocumentNode.SelectNodes("//*") != null)
            {
                HtmlAgilityPack.HtmlNode prevNode = null;
                int sameNodeNum = 0;
                foreach (HtmlAgilityPack.HtmlNode node in doc.DocumentNode.SelectNodes("//*"))
                {

                    if (prevNode != null)
                        if (
                            (prevNode.NextSibling == node && (node.Name == "br" || node.Name == "p")) //more <br><p> togethe, keep only 1
                            )
                        {
                            string currInnText = RemoveHTML(node.InnerHtml.Replace(cr, string.Empty).Replace(lf, string.Empty)).Trim();
                            if (currInnText.Length == 0)
                                sameNodeNum++;
                        }
                        else
                        {
                            //prevNode.NextSibling should be current node, but now isn't
                            if (prevNode.NextSibling != null && prevNode.NextSibling.NodeType == HtmlAgilityPack.HtmlNodeType.Text && (node.Name == "br" || node.Name == "p"))
                            {
                                string prevInnText = RemoveHTML(prevNode.NextSibling.InnerHtml.Replace(cr, string.Empty).Replace(lf, string.Empty)).Trim();
                                string currInnText = RemoveHTML(node.InnerHtml.Replace(cr, string.Empty).Replace(lf, string.Empty)).Trim();
                                if (prevInnText.Length == 0 && currInnText.Length == 0)
                                    sameNodeNum++;
                                else
                                    sameNodeNum = 0;
                            }
                            else
                                sameNodeNum = 0;
                        }
                    if (sameNodeNum > AllowedRepetition)
                    {
                        node.Remove();
                        sameNodeNum--;
                        continue;
                    }
                    if (node.HasAttributes)
                    {
                        //ui
                        if (node.Attributes.Contains("style"))
                            node.Attributes["style"].Remove();
                        if (node.Attributes.Contains("width"))
                            node.Attributes["width"].Remove();
                        if (node.Attributes.Contains("height"))
                            node.Attributes["height"].Remove();
                        if (node.Attributes.Contains("size"))
                            node.Attributes["size"].Remove();
                        if (node.Attributes.Contains("class"))
                            node.Attributes["class"].Remove();
                        if (node.Attributes.Contains("target"))
                            node.Attributes["target"].Remove();
                        if (node.Attributes.Contains("id"))
                            node.Attributes["id"].Remove();
                    }

                    prevNode = node;
                }
                text = doc.DocumentNode.InnerHtml;
            }
            //replace &nbsp;
            text = ReplaceHTMLEntities(text);

            return TextUtil.ShortenHTML(text, text.Length, allowedHtmlTags);

        }



        private static void FillHTMLEntityTable()
        {
            lock (lockObj)
            {
                HTMLEntities.Clear();

                HTMLEntities.Add("&quot;", "\"");
                HTMLEntities.Add("&amp;", "&");
                HTMLEntities.Add("&apos;", "'");
                //HTMLEntities.Add("&lt;", "<");
                //HTMLEntities.Add("&gt;", ">");
                HTMLEntities.Add("&nbsp;", " ");
                HTMLEntities.Add("&iexcl;", "¡");
                HTMLEntities.Add("&cent;", "¢");
                HTMLEntities.Add("&pound;", "£");
                HTMLEntities.Add("&curren;", "¤");
                HTMLEntities.Add("&yen;", "¥");
                HTMLEntities.Add("&brvbar;", "¦");
                HTMLEntities.Add("&sect;", "§");
                HTMLEntities.Add("&uml;", "¨");
                HTMLEntities.Add("&copy;", "©");
                HTMLEntities.Add("&ordf;", "ª");
                HTMLEntities.Add("&laquo;", "«");
                HTMLEntities.Add("&not;", "¬");
                HTMLEntities.Add("&shy;", Convert.ToChar(173).ToString());
                HTMLEntities.Add("&reg;", "®");
                HTMLEntities.Add("&macr;", "¯");
                HTMLEntities.Add("&deg;", "°");
                HTMLEntities.Add("&plusmn;", "±");
                HTMLEntities.Add("&sup2;", "²");
                HTMLEntities.Add("&sup3;", "³");
                HTMLEntities.Add("&acute;", "´");
                HTMLEntities.Add("&micro;", "µ");
                HTMLEntities.Add("&para;", "¶");
                HTMLEntities.Add("&middot;", "·");
                HTMLEntities.Add("&cedil;", "¸");
                HTMLEntities.Add("&sup1;", "¹");
                HTMLEntities.Add("&ordm;", "º");
                HTMLEntities.Add("&raquo;", " »");
                HTMLEntities.Add("&frac14;", "¼");
                HTMLEntities.Add("&frac12;", "½");
                HTMLEntities.Add("&frac34;", "¾");
                HTMLEntities.Add("&iquest;", "¿");
                HTMLEntities.Add("&Agrave;", "À");
                HTMLEntities.Add("&Aacute;", "Á");
                HTMLEntities.Add("&Acirc;", "Â");
                HTMLEntities.Add("&Atilde;", "Ã");
                HTMLEntities.Add("&Auml;", "Ä");
                HTMLEntities.Add("&Aring;", "Å");
                HTMLEntities.Add("&AElig;", "Æ");
                HTMLEntities.Add("&Ccedil;", "Ç");
                HTMLEntities.Add("&Egrave;", "È");
                HTMLEntities.Add("&Eacute;", "É");
                HTMLEntities.Add("&Ecirc;", "Ê");
                HTMLEntities.Add("&Euml;", "Ë");
                HTMLEntities.Add("&Igrave;", "Ì");
                HTMLEntities.Add("&Iacute;", "Í");
                HTMLEntities.Add("&Icirc;", "Î");
                HTMLEntities.Add("&Iuml;", "Ï");
                HTMLEntities.Add("&ETH;", "Ð");
                HTMLEntities.Add("&Ntilde;", "Ñ");
                HTMLEntities.Add("&Ograve;", "Ò");
                HTMLEntities.Add("&Oacute;", "Ó");
                HTMLEntities.Add("&Ocirc;", "Ô");
                HTMLEntities.Add("&Otilde;", "Õ");
                HTMLEntities.Add("&Ouml;", "Ö");
                HTMLEntities.Add("&times;", "×");
                HTMLEntities.Add("&Oslash;", "Ø");
                HTMLEntities.Add("&Ugrave;", "Ù");
                HTMLEntities.Add("&Uacute;", "Ú");
                HTMLEntities.Add("&Ucirc;", "Û");
                HTMLEntities.Add("&Uuml;", "Ü");
                HTMLEntities.Add("&Yacute;", "Ý");
                HTMLEntities.Add("&THORN;", "Þ");
                HTMLEntities.Add("&szlig;", "ß");
                HTMLEntities.Add("&agrave;", "à");
                HTMLEntities.Add("&aacute;", "á");
                HTMLEntities.Add("&acirc;", "â");
                HTMLEntities.Add("&atilde;", "ã");
                HTMLEntities.Add("&auml;", "ä");
                HTMLEntities.Add("&aring;", "å");
                HTMLEntities.Add("&aelig;", "æ");
                HTMLEntities.Add("&ccedil;", "ç");
                HTMLEntities.Add("&egrave;", "è");
                HTMLEntities.Add("&eacute;", "é");
                HTMLEntities.Add("&ecirc;", "ê");
                HTMLEntities.Add("&euml;", "ë");
                HTMLEntities.Add("&igrave;", "ì");
                HTMLEntities.Add("&iacute;", "í");
                HTMLEntities.Add("&icirc;", "î");
                HTMLEntities.Add("&iuml;", "ï");
                HTMLEntities.Add("&eth;", "ð");
                HTMLEntities.Add("&ntilde;", "ñ");
                HTMLEntities.Add("&ograve;", "ò");
                HTMLEntities.Add("&oacute;", "ó");
                HTMLEntities.Add("&ocirc;", "ô");
                HTMLEntities.Add("&otilde;", "õ");
                HTMLEntities.Add("&ouml;", "ö");
                HTMLEntities.Add("&divide;", "÷");
                HTMLEntities.Add("&oslash;", "ø");
                HTMLEntities.Add("&ugrave;", "ù");
                HTMLEntities.Add("&uacute;", "ú");
                HTMLEntities.Add("&ucirc;", "û");
                HTMLEntities.Add("&uuml;", "ü");
                HTMLEntities.Add("&yacute;", "ý");
                HTMLEntities.Add("&thorn;", "þ");
                HTMLEntities.Add("&yuml;", "ÿ");
                HTMLEntities.Add("&OElig;", "Œ");
                HTMLEntities.Add("&oelig;", "œ");
                HTMLEntities.Add("&Scaron;", "Š");
                HTMLEntities.Add("&scaron;", "š");
                HTMLEntities.Add("&Yuml;", "Ÿ");
                HTMLEntities.Add("&fnof;", "ƒ");
                HTMLEntities.Add("&circ;", "ˆ");
                HTMLEntities.Add("&tilde;", "˜");
                HTMLEntities.Add("&Alpha;", "Α");
                HTMLEntities.Add("&Beta;", "Β");
                HTMLEntities.Add("&Gamma;", "Γ");
                HTMLEntities.Add("&Delta;", "Δ");
                HTMLEntities.Add("&Epsilon;", "Ε");
                HTMLEntities.Add("&Zeta;", "Ζ");
                HTMLEntities.Add("&Eta;", "Η");
                HTMLEntities.Add("&Theta;", "Θ");
                HTMLEntities.Add("&Iota;", "Ι");
                HTMLEntities.Add("&Kappa;", "Κ");
                HTMLEntities.Add("&Lambda;", "Λ");
                HTMLEntities.Add("&Mu;", "Μ");
                HTMLEntities.Add("&Nu;", "Ν");
                HTMLEntities.Add("&Xi;", "Ξ");
                HTMLEntities.Add("&Omicron;", "Ο");
                HTMLEntities.Add("&Pi;", "Π");
                HTMLEntities.Add("&Rho;", "Ρ");
                HTMLEntities.Add("&Sigma;", "Σ");
                HTMLEntities.Add("&Tau;", "Τ");
                HTMLEntities.Add("&Upsilon;", "Υ");
                HTMLEntities.Add("&Phi;", "Φ");
                HTMLEntities.Add("&Chi;", "Χ");
                HTMLEntities.Add("&Psi;", "Ψ");
                HTMLEntities.Add("&Omega;", "Ω");
                HTMLEntities.Add("&alpha;", "α");
                HTMLEntities.Add("&beta;", "β");
                HTMLEntities.Add("&gamma;", "γ");
                HTMLEntities.Add("&delta;", "δ");
                HTMLEntities.Add("&epsilon;", "ε");
                HTMLEntities.Add("&zeta;", "ζ");
                HTMLEntities.Add("&eta;", "η");
                HTMLEntities.Add("&theta;", "θ");
                HTMLEntities.Add("&iota;", "ι");
                HTMLEntities.Add("&kappa;", "κ");
                HTMLEntities.Add("&lambda;", "λ");
                HTMLEntities.Add("&mu;", "μ");
                HTMLEntities.Add("&nu;", "ν");
                HTMLEntities.Add("&xi;", "ξ");
                HTMLEntities.Add("&omicron;", "ο");
                HTMLEntities.Add("&pi;", "π");
                HTMLEntities.Add("&rho;", "ρ");
                HTMLEntities.Add("&sigmaf;", "ς");
                HTMLEntities.Add("&sigma;", "σ");
                HTMLEntities.Add("&tau;", "τ");
                HTMLEntities.Add("&upsilon;", "υ");
                HTMLEntities.Add("&phi;", "φ");
                HTMLEntities.Add("&chi;", "χ");
                HTMLEntities.Add("&psi;", "ψ");
                HTMLEntities.Add("&omega;", "ω");
                HTMLEntities.Add("&thetasym;", "ϑ");
                HTMLEntities.Add("&upsih;", "ϒ");
                HTMLEntities.Add("&piv;", "ϖ");
                HTMLEntities.Add("&ensp", Convert.ToChar(8196).ToString());
                HTMLEntities.Add("&emsp", Convert.ToChar(8195).ToString());
                HTMLEntities.Add("&thinsp", Convert.ToChar(8201).ToString());
                HTMLEntities.Add("&zwnj", Convert.ToChar(8204).ToString());
                HTMLEntities.Add("&zwj", Convert.ToChar(8205).ToString());
                HTMLEntities.Add("&lrm", Convert.ToChar(8206).ToString());
                HTMLEntities.Add("&rlm", Convert.ToChar(8207).ToString());
                HTMLEntities.Add("&ndash;", "–");
                HTMLEntities.Add("&mdash;", "—");
                HTMLEntities.Add("&lsquo;", "‘");
                HTMLEntities.Add("&rsquo;", "’");
                HTMLEntities.Add("&sbquo;", "‚");
                HTMLEntities.Add("&ldquo;", "“");
                HTMLEntities.Add("&rdquo;", "”");
                HTMLEntities.Add("&bdquo;", "„");
                HTMLEntities.Add("&dagger;", "†");
                HTMLEntities.Add("&Dagger;", "‡");
                HTMLEntities.Add("&bull;", "•");
                HTMLEntities.Add("&hellip;", "…");
                HTMLEntities.Add("&permil;", "‰");
                HTMLEntities.Add("&prime;", "′");
                HTMLEntities.Add("&Prime;", "″");
                HTMLEntities.Add("&lsaquo;", "‹");
                HTMLEntities.Add("&rsaquo;", "›");
                HTMLEntities.Add("&oline;", "‾");
                HTMLEntities.Add("&frasl;", "⁄");
                HTMLEntities.Add("&euro;", "€");
                HTMLEntities.Add("&image;", "ℑ");
                HTMLEntities.Add("&weierp;", "℘");
                HTMLEntities.Add("&real;", "ℜ");
                HTMLEntities.Add("&trade;", "™");
                HTMLEntities.Add("&alefsym;", "ℵ");
                HTMLEntities.Add("&larr;", "←");
                HTMLEntities.Add("&uarr;", "↑");
                HTMLEntities.Add("&rarr;", "→");
                HTMLEntities.Add("&darr;", "↓");
                HTMLEntities.Add("&harr;", "↔");
                HTMLEntities.Add("&crarr;", "↵");
                HTMLEntities.Add("&lArr;", "⇐");
                HTMLEntities.Add("&uArr;", "⇑");
                HTMLEntities.Add("&rArr;", "⇒");
                HTMLEntities.Add("&dArr;", "⇓");
                HTMLEntities.Add("&hArr;", "⇔");
                HTMLEntities.Add("&forall;", "∀");
                HTMLEntities.Add("&part;", "∂");
                HTMLEntities.Add("&exist;", "∃");
                HTMLEntities.Add("&empty;", "∅");
                HTMLEntities.Add("&nabla;", "∇");
                HTMLEntities.Add("&isin;", "∈");
                HTMLEntities.Add("&notin;", "∉");
                HTMLEntities.Add("&ni;", "∋");
                HTMLEntities.Add("&prod;", "∏");
                HTMLEntities.Add("&sum;", "∑");
                HTMLEntities.Add("&minus;", "−");
                HTMLEntities.Add("&lowast;", "∗");
                HTMLEntities.Add("&radic;", "√");
                HTMLEntities.Add("&prop;", "∝");
                HTMLEntities.Add("&infin;", "∞");
                HTMLEntities.Add("&ang;", "∠");
                HTMLEntities.Add("&and;", "∧");
                HTMLEntities.Add("&or;", "∨");
                HTMLEntities.Add("&cap;", "∩");
                HTMLEntities.Add("&cup;", "∪");
                HTMLEntities.Add("&int;", "∫");
                HTMLEntities.Add("&there4;", "∴");
                HTMLEntities.Add("&sim;", "∼");
                HTMLEntities.Add("&cong;", "≅");
                HTMLEntities.Add("&asymp;", "≈");
                HTMLEntities.Add("&ne;", "≠");
                HTMLEntities.Add("&equiv;", "≡");
                HTMLEntities.Add("&le;", "≤");
                HTMLEntities.Add("&ge;", "≥");
                HTMLEntities.Add("&sub;", "⊂");
                HTMLEntities.Add("&sup;", "⊃");
                HTMLEntities.Add("&nsub;", "⊄");
                HTMLEntities.Add("&sube;", "⊆");
                HTMLEntities.Add("&supe;", "⊇");
                HTMLEntities.Add("&oplus;", "⊕");
                HTMLEntities.Add("&otimes;", "⊗");
                HTMLEntities.Add("&perp;", "⊥");
                HTMLEntities.Add("&sdot;", "⋅");
                HTMLEntities.Add("&lceil;", "⌈");
                HTMLEntities.Add("&rceil;", "⌉");
                HTMLEntities.Add("&lfloor;", "⌊");
                HTMLEntities.Add("&rfloor;", "⌋");
                HTMLEntities.Add("&lang;", "〈");
                HTMLEntities.Add("&rang;", "〉");
                HTMLEntities.Add("&loz;", "◊");
                HTMLEntities.Add("&spades;", "♠");
                HTMLEntities.Add("&clubs;", "♣");
                HTMLEntities.Add("&hearts;", "♥");
                HTMLEntities.Add("&diams;", "♦");
            }
        }

             public static Tuple<decimal,long> WordsVarianceInText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new Tuple<decimal, long>(0,0);

            var words = text.Split( new char[] {
                ' ','\t', '\n','\r','\v','\b', // regex \s
                ';',',',';','.','!',
                '(',')','<','>','{','}','[',']','_','-','~','|','=','&','#' }, StringSplitOptions.RemoveEmptyEntries )
                .Select(m=>m.ToLower());

            var wordNums = words
                .GroupBy(w => w, w => w, (w1, w2) => new { w = w1, c = w2.LongCount() })
                .ToDictionary(k => k.w, v => v.c);

            //var idxNorm = MathTools.Herfindahl_Hirschman_IndexNormalized(wordNums);
            var idx = MathTools.Herfindahl_Hirschman_Index(wordNums.Values.Select(m => (decimal)m));
            return new Tuple<decimal, long>(idx, wordNums.LongCount());
        }

        public static List<Tuple<string, bool>> SplitStringToPartsWithQuotes(string query, char quoteDelimiter)
        {
            //split newquery into part based on ", mark "xyz" parts
            //string , bool = true ...> part withint ""
            List<Tuple<string, bool>> textParts = new List<Tuple<string, bool>>();
            int[] found = CharacterPositionsInString(query, quoteDelimiter);
            if (found.Length > 0 && found.Length % 2 == 0)
            {
                int start = 0;
                bool withIn = false;
                foreach (var idx in found)
                {
                    int startIdx = start;
                    int endIdx = idx;
                    if (withIn)
                        endIdx++;
                    textParts.Add(new Tuple<string, bool>(query.Substring(startIdx, endIdx - startIdx), withIn));
                    start = endIdx;
                    withIn = !withIn;
                }
                if (start < query.Length)
                    textParts.Add(new Tuple<string, bool>(query.Substring(start), withIn));
            }
            else
            {
                textParts.Add(new Tuple<string, bool>(query, false));
            }
            return textParts;
        }

        public static int[] CharacterPositionsInString(string text, char lookingFor)
        {
            if (string.IsNullOrEmpty(text))
                return new int[] { };

            char[] textarray = text.ToCharArray();
            List<int> found = new List<int>();
            for (int i = 0; i < text.Length; i++)
            {
                if (textarray[i].Equals(lookingFor))
                {
                    found.Add(i);
                }
            }
            return found.ToArray();
        }



        public static string[] SplitWithSeparators(this string s, StringSplitOptions splitOptions = StringSplitOptions.None, params char[] separators)
        {
            return SplitWithSeparators(s, splitOptions, StringComparison.OrdinalIgnoreCase , separators.Select(m => m.ToString()).ToArray());
        }
        public static string[] SplitWithSeparators(this string s,
            StringSplitOptions splitOptions,
            StringComparison comparisonType,
            params char[] separators)
        {
            return SplitWithSeparators(s, splitOptions, StringComparison.OrdinalIgnoreCase, separators.Select(m => m.ToString()).ToArray());
        }
        public static string[] SplitWithSeparators(this string s, 
            StringSplitOptions splitOptions, 
            StringComparison comparisonType,
            params string[] separators)
        {
            if (string.IsNullOrEmpty(s))
                return new string[] { };

            List<string> parts = new List<string>();
            int prevStart = 0;
            for (int i = 0; i < s.Length; i++)
            {
                foreach (var sep in separators)
                {
                    if (i + sep.Length <= s.Length)
                    {
                        if (s.Substring(i, sep.Length).Equals(sep, comparisonType))
                        {
                            int partLen = i - prevStart;
                            if (partLen > 0)
                                parts.Add(s.Substring(prevStart, partLen));
                            parts.Add(sep);
                            i = i + sep.Length;
                            prevStart = i;
                            break;
                        }
                    }
                }
            }
            if (prevStart < s.Length)
                parts.Add(s.Substring(prevStart));

            if (splitOptions == StringSplitOptions.RemoveEmptyEntries)
                return parts.Where(m => !string.IsNullOrEmpty(m)).ToArray();

            return parts.ToArray();
        }
        
        public static string RemoveAccents(this string input)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string KeepLettersNumbersAndSpace(this string input)
        {
            var res = Regex.Replace(input, @"[^\w ]", "", RegexOptions.CultureInvariant);
            res = res.Replace("_", "");
            return res;
        }

    }
}
