using System;
using System.Collections.Generic;
using System.Text;

namespace Devmasters
{
    public static class ParseText
    {
        public static System.Globalization.CultureInfo enCulture = System.Globalization.CultureInfo.InvariantCulture; //new System.Globalization.CultureInfo("en-US");
        public static System.Globalization.CultureInfo czCulture = System.Globalization.CultureInfo.GetCultureInfo("cs-CZ");
        public static System.Globalization.CultureInfo csCulture = System.Globalization.CultureInfo.GetCultureInfo("cs");

        public static decimal? FromTextToDecimal(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            return ToDecimal(Devmasters.TextUtil.NormalizeToNumbersOnly(value));
        }

        public static decimal? ToDecimal(this string value, decimal? ifInvalidValue = null)
        {
            if (string.IsNullOrEmpty(value))
                return ifInvalidValue;

            value = value.Replace(" ", "").Trim();
            if (value.StartsWith(",") || value.StartsWith("."))
                value = value.Substring(1);
            if (value.EndsWith(",") || value.EndsWith("."))
                value = value.Substring(0, value.Length - 1);
            if (value.EndsWith(",-") || value.EndsWith(".-"))
                value = value.Substring(0, value.Length - 2);
            if (value.EndsWith(",--") || value.EndsWith(".--"))
                value = value.Substring(0, value.Length - 3);
            if (value.EndsWith(",00") || value.EndsWith(".00"))
                value = value.Substring(0, value.Length - 3);

            //System.Globalization.NumberStyles styles = System.Globalization.NumberStyles.AllowLeadingWhite
            //    | System.Globalization.NumberStyles.AllowTrailingWhite
            //    | System.Globalization.NumberStyles.AllowThousands
            //    | System.Globalization.NumberStyles.AllowCurrencySymbol
            //    ;
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, czCulture, out decimal tmp)
                || decimal.TryParse(value, System.Globalization.NumberStyles.Any, enCulture, out tmp))
                return tmp;
            else
                return ifInvalidValue;
        }

        public static int? ToInt(string value, int? ifInvalidValue = null)
        {
            if (string.IsNullOrEmpty(value))
                return ifInvalidValue;
            int ret = 0;
            if (int.TryParse(value, out ret))
            {
                return ret;
            }
            else
                return ifInvalidValue;
        }

    }
}
