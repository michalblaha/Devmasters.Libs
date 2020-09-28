using System.Globalization;

namespace Devmasters.Lang
{
    public class PluralDef
    {
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;
        public bool WithZero { get; set; } = false;
        public string[] Values { get; set; } = null;
    }
}
