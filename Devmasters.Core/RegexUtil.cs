using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Devmasters
{
    public static class RegexUtil
    {

        public static string ValueFromTheBestRegex(this string txt, string[] regexs, string groupname)
        {
            return GetValueFromTheBestRegex(txt, regexs, groupname);
        }
        public static string GetValueFromTheBestRegex(string txt, string[] regexs, string groupname)
        {
            string[] rets = GetValuesFromTheBestRegex(txt, regexs, groupname);
            if (rets != null && rets.Length > 0)
                return rets[0];
            else
                return null;
        }


        public static string[] ValuesFromTheBestRegex(this string txt, string[] regexs, string groupname)
        {
            return GetValuesFromTheBestRegex(txt, regexs, groupname);
        }
        public static string[] GetValuesFromTheBestRegex(string txt, string[] regexs, string groupname)
        {
            string[] ret = null;
            foreach (var r in regexs)
            {
                ret = GetRegexGroupValues(txt, r, groupname);
                if (ret != null && ret.Length > 0)
                    return ret;
            }

            return new string[] { };
        }

        public static string RegexGroupValue(this string txt, string regex, string groupname)
        {
            return GetRegexGroupValue(txt, regex, groupname);
        }

        public static string GetRegexGroupValue(string txt, string regex, string groupname)
        {
            if (string.IsNullOrEmpty(txt))
                return null;
            Regex myRegex = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
            foreach (Match match in myRegex.Matches(txt))
            {
                if (match.Success)
                {
                    if (match.Groups[groupname].Captures.Count > 1)
                        return match.Groups[groupname].Captures[0].Value;
                    else
                        return match.Groups[groupname].Value;
                }
            }
            return string.Empty;
        }

        public static string[] RegexGroupValues(this string txt, string regex, string groupname)
        {
            return GetRegexGroupValues(txt, regex, groupname);
        }
        public static string[] GetRegexGroupValues(string txt, string regex, string groupname)
        {
            if (string.IsNullOrEmpty(txt))
                return null;
            Regex myRegex = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
            List<string> results = new List<string>();
            foreach (Match match in myRegex.Matches(txt))
            {
                if (match.Success)
                {
                    if (match.Groups[groupname].Captures.Count > 1)
                        results.Add(match.Groups[groupname].Captures[0].Value); //pokud nalezeno vice vyskytu, vem prvni
                    else
                        results.Add(match.Groups[groupname].Value);
                }
            }
            return results.ToArray();
        }

        public static string ReplaceGroupMatchNameWithRegex(this string txt, string regex, string groupname, string replacement)
        {
            if (string.IsNullOrEmpty(txt))
                return txt;
            var dateIntervalM = Regex.Matches(txt, regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
            foreach (Match m in dateIntervalM)
            {
                if (m.Success && m.Groups[groupname].Success)
                {
                    txt = txt.Substring(0, m.Groups[groupname].Index)
                        + replacement
                        + txt.Substring(m.Groups[groupname].Index + m.Groups[groupname].Length);

                }
            }
            return txt;
        }


        public static string ReplaceWithRegex(this string txt, string replacement, string regex)
        {
            return GetStringReplaceWithRegex(regex, txt, replacement);
        }
        public static string GetStringReplaceWithRegex(string regex, string txt, string replacement)
        {
            Regex myRegex = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
            return myRegex.Replace(txt, replacement);
        }


    }
}
