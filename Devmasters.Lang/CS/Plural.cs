using System;
using System.Globalization;
using System.Linq;

namespace Devmasters.Lang
{
    public class Plural
    {
        public static string Get(int number, Devmasters.IResourceManager2 resources, string key)
        {
            return Get(number, CultureInfo.CurrentUICulture, resources, key);
        }
        public static string Get(int number, CultureInfo culture, Devmasters.IResourceManager2 resources, string key)
        {
            return Get(number, resources.Manager.GetString(key, culture));
        }

        public static string Get(int number, string value, CultureInfo culture = null)
        {
            return Get(number, culture ?? CultureInfo.CurrentUICulture, value.Split(';'));
        }

        public static string Get(int number, params string[] value)
        {
            return Get(number, CultureInfo.CurrentUICulture, value);
        }
        public static string Get(int number, CultureInfo culture,params string[] value)
        {
            return Get(number, false, culture, value);
        }


        public static string GetWithZero(int number, Devmasters.IResourceManager2 resources, string key)
        {
            return GetWithZero(number, CultureInfo.CurrentUICulture, resources, key);
        }
        public static string GetWithZero(int number, CultureInfo culture, Devmasters.IResourceManager2 resources, string key)
        {
            return GetWithZero(number, resources.Manager.GetString(key, culture));
        }

        public static string GetWithZero(int number, string value)
        {
            return GetWithZero(number, value.Split(';'));
        }
        public static string GetWithZero(int number, params string[] value)
        {
            return Get(number, true, CultureInfo.CurrentUICulture, value);
        }


        public static string Get(int number, bool withZero, CultureInfo culture, params string[] val)
        {
            return Get(number, new PluralDef()
            {
                WithZero = withZero,
                Culture = culture,
                Values = val
            });
        }
        public static string Get(int number, PluralDef def)
        {
            if (def.Values == null || def.Values.Length == 0)
                return String.Empty;

            if (def.WithZero)
            {
                if (number == 0)
                    return FormatString(def.Values[0], number);
                else
                    return Get(number, new PluralDef()
                    {
                        WithZero = false,
                        Values = def.Values.Skip(1).ToArray(),
                        Culture = def.Culture
                    });
                //val = val.Skip(1).ToArray();
            }

            switch (def.Culture.TwoLetterISOLanguageName)
            {
                case "cs":
                    return GetCzechPlural(number, def.Values);
                case "pl":
                    return GetPolandPlural(number, def.Values);
                case "ru":
                    return GetRuPlural(number, def.Values);

                case "en":
                    return GetEnglishPlural(number, def.Values);

                case "de":
                    return GetGermanPlural(number, def.Values);

                case "ja":
                    return GetJapanesePlural(number, def.Values);

                default:
                    return GetEnglishPlural(number, def.Values);
                    //return val;
            }

            //return val;
        }


        private static string GetCzechPlural(int number, params string[] val)
        {
            string[] plural = val;
            if (plural.Length != 3)
                throw new InvalidResourceException("Invalid czech resource. The resource string  " + val + " doesn't contains 3 options.");


            if (Math.Abs(number) == 1)
                return FormatString(plural[0], number);

            if (Math.Abs(number) > 1 && Math.Abs(number) < 5)
                return FormatString(plural[1], number);

            return FormatString(plural[2], number);
        }
        private static string GetPolandPlural(int number, params string[] val)
        {
            string[] plural = val;
            if (plural.Length != 3)
                throw new InvalidResourceException("Invalid poland resource. The resource string  " + val + " doesn't contains 3 options.");


            if (number == 1)
                return FormatString(plural[0], number);

            if (number > 1 && number < 5)
                return FormatString(plural[1], number);

            return FormatString(plural[2], number);
        }
        private static string GetEnglishPlural(int number, params string[] val)
        {
            string[] plural = val;
            if (plural.Length != 2)
                throw new InvalidResourceException("Invalid english resource. The resource string " + val + " doesn't contains 2 options.");

            return number == 1 ? FormatString(plural[0], number) : FormatString(plural[1], number);
        }

        private static string GetGermanPlural(int number, params string[] val)
        {
            string[] plural = val;
            if (plural.Length != 2)
                throw new InvalidResourceException("Invalid german resource. The resource string  " + val + " doesn't contains 2 options.");

            return number == 1 ? FormatString(plural[0], number) : FormatString(plural[1], number);
        }

        private static string GetJapanesePlural(int number, params string[] val)
        {
            string[] plural = val;
            if (plural.Length != 1)
                throw new InvalidResourceException("Invalid german resource. The resource string  " + val + " doesn't contains 1 options.");
            return FormatString(val[0], number);
        }
        private static string GetRuPlural(int number, params string[] val)
        {
            string[] plural = val;
            if (plural.Length != 3)
                throw new InvalidResourceException("Invalid RU resource. The resource string  " + val + " doesn't contains 3 options.");

            if (number == 1)
                return FormatString(plural[0], number);

            if (number > 1 && number < 5)
                return FormatString(plural[1], number);

            return FormatString(plural[2], number);
        }


        private static string FormatString(string text, int number)
        {
            if (text.Contains("{") && text.Contains("}"))
                return string.Format(text, number);
            else
                return text;
        }
    }

    public class InvalidResourceException : ApplicationException
    {
        public InvalidResourceException(String message)
            : base(message)
        {
        }
    }
}
