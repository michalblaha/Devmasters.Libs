using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Devmasters.DT
{
    public enum DateTimeDiff
    {
        Day,
        Hour
    }

    public static class Util
    {
        static object lockObj = new object();
        static System.Resources.ResourceManager rm = null;
        static Util()
        {
            lock (lockObj)
            {
                rm = new System.Resources.ResourceManager("Devmasters.DT.Texts", typeof(Util).Assembly);
            }

            //zdroj: Zákon č. 245/2000 Sb o statních svátcích
            // a https://www.kurzy.cz/kalendar/statni-svatky/2021/
            Svatky.Add(2021, new DateTime[] {
                ToDate("01.01.2021").Value,
                ToDate("02.04.2021").Value,
                ToDate("05.04.2021").Value,
                ToDate("01.05.2021").Value,
                ToDate("08.05.2021").Value,
                ToDate("05.07.2021").Value,
                ToDate("06.07.2021").Value,
                ToDate("28.09.2021").Value,
                ToDate("28.10.2021").Value,
                ToDate("17.11.2021").Value,
                ToDate("24.12.2021").Value,
                ToDate("25.12.2021").Value,
                ToDate("26.12.2021").Value,
            });
            Svatky.Add(2020, new DateTime[] {
                ToDate("01.01.2020").Value,
                ToDate("10.04.2020").Value,
                ToDate("13.04.2020").Value,
                ToDate("01.05.2020").Value,
                ToDate("08.05.2020").Value,
                ToDate("05.07.2020").Value,
                ToDate("06.07.2020").Value,
                ToDate("28.09.2020").Value,
                ToDate("28.10.2020").Value,
                ToDate("17.11.2020").Value,
                ToDate("24.12.2020").Value,
                ToDate("25.12.2020").Value,
                ToDate("26.12.2020").Value,
            });
            Svatky.Add(2019, new DateTime[] {
                ToDate("01.01.2019").Value,
                ToDate("19.04.2019").Value,
                ToDate("22.04.2019").Value,
                ToDate("01.05.2019").Value,
                ToDate("08.05.2019").Value,
                ToDate("05.07.2019").Value,
                ToDate("06.07.2019").Value,
                ToDate("28.09.2019").Value,
                ToDate("28.10.2019").Value,
                ToDate("17.11.2019").Value,
                ToDate("24.12.2019").Value,
                ToDate("25.12.2019").Value,
                ToDate("26.12.2019").Value,
            });
            Svatky.Add(2018, new DateTime[] {
                ToDate("01.01.2018").Value,
                ToDate("30.03.2018").Value,
                ToDate("02.04.2018").Value,
                ToDate("01.05.2018").Value,
                ToDate("08.05.2018").Value,
                ToDate("05.07.2018").Value,
                ToDate("06.07.2018").Value,
                ToDate("28.09.2018").Value,
                ToDate("28.10.2018").Value,
                ToDate("17.11.2018").Value,
                ToDate("24.12.2018").Value,
                ToDate("25.12.2018").Value,
                ToDate("26.12.2018").Value,
            });
            Svatky.Add(2017, new DateTime[] {
                ToDate("01.01.2017").Value,
                ToDate("14.04.2017").Value,
                ToDate("17.04.2017").Value,
                ToDate("01.05.2017").Value,
                ToDate("08.05.2017").Value,
                ToDate("05.07.2017").Value,
                ToDate("06.07.2017").Value,
                ToDate("28.09.2017").Value,
                ToDate("28.10.2017").Value,
                ToDate("17.11.2017").Value,
                ToDate("24.12.2017").Value,
                ToDate("25.12.2017").Value,
                ToDate("26.12.2017").Value,
        });

            foreach (var year in CalendarYears)
            {
                DateTime start = new DateTime(year, 1, 1);
                Vikendy.Add(year,
                        Enumerable.Range(0, DateTime.IsLeapYear(year) ? 366 : 365)
                            .Select(day => start.AddDays(day))
                            .Where(dt => dt.DayOfWeek == DayOfWeek.Sunday ||
                                         dt.DayOfWeek == DayOfWeek.Saturday)
                            .ToArray()
                            );
            }
            foreach (var year in CalendarYears)
            {
                NepracovniDny.Add(year, Svatky[year].Concat(Vikendy[year]).Distinct().ToArray());
            }


        }

        public static DateTime? RodneCisloToDate(string rc)
        {
            try
            {
                if (string.IsNullOrEmpty(rc))
                    return null;
                var suffix = rc.Substring(0, 2);
                string syear = (Convert.ToInt32((DateTime.Now.Year - 18).ToString().Substring(2)) > Convert.ToInt32(suffix) ? "20" : "19") + suffix;
                int year = Convert.ToInt32(syear);
                int month = Convert.ToInt32(rc.Substring(2, 2));
                month = month > 50 ? month - 50 : month;
                int day = Convert.ToInt32(rc.Substring(4, 2));
                return new DateTime(year, month, day);
            }
            catch (Exception)
            {

                return null;
            }
        }


        static string[] dateFormats = new string[] {
                "d.M.yyyy", "d. M. yyyy",
                "dd.MM.yyyy", "dd. MM. yyyy",
                "dd.MM.yy", "dd. MM. yy",
                "d.M.yy", "d. M. yy",
                "yyyy-MM-dd", "yyyy-M-d",
                "yy-MM-dd", "yy-M-d",
        };
        static string[] timeFormats = new string[] {"",
                "H:m:s","HH:mm:ss",
                "H:m","HH:mm",
                "H: m: s","HH: mm: ss",
                "H: m: s","HH: mm: ss tt",
                "h: m: s","HH: mm: ss tt",
                "H:m:s.f","HH:mm:ss.f",
                "H:m:s.ff","HH:mm:ss.ff",
                "H:m:s.fff","HH:mm:ss.fff",
                "H:m:s.ffff","HH:mm:ss.ffff",
                "H:m:s.fffff","HH:mm:ss.fffff",
                "H:m:s.fK","HH:mm:ss.fK",
                "H:m:s.ffK","HH:mm:ss.ffK",
                "H:m:s.fffK","HH:mm:ss.fffK",
                "H:m:s.ffffK","HH:mm:ss.ffffK",
                "H:m:s.fffffK","HH:mm:ss.fffffK",
            };

        static object lockComb = new object();
        static string[] combinations = null;
        public static DateTime? ToDateTime(string value)
        {

            if (combinations == null)
                lock (lockComb)
                {
                    if (combinations == null)
                    {
                        List<string> cc = new List<string>();
                        cc.Add("yyyy-MM-ddTHH:mm:ss.ff");
                        cc.Add("yyyy-MM-ddTHH:mm:ss.ffK");
                        cc.Add("yyyy-MM-ddTHH:mm:ss.fff");
                        cc.Add("yyyy-MM-ddTHH:mm:ss.fffK");
                        cc.Add("yyyy-MM-ddTHH:mm:ss.ffff");
                        cc.Add("yyyy-MM-ddTHH:mm:ss.ffffK");
                        foreach (var d in dateFormats)
                            foreach (var t in timeFormats)
                                cc.Add((d + " " + t).Trim());

                        combinations = cc.ToArray();
                    }
                };


            return ToDateTime(value, combinations);

        }
        public static DateTime? ToDate(string value)
        {
            return ToDateTime(value,
                dateFormats
                );
        }
        public static DateTime? ToDateTimeFromCode(string value)
        {
            return ToDateTime(value, "yyyy-MM-dd", "yyyy-M-d", "yy-MM-D");
        }

        public static DateTime? ToDateTime(string value, params string[] formats)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            foreach (var f in formats)
            {
                var dt = ToDateTime(value, f);
                if (dt.HasValue)
                    return dt;
            }
            return null;
        }

        public static DateTime? ToDateTime(string value, string format)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            DateTime tmp;
            if (DateTime.TryParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal | System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tmp))
                return new DateTime?(tmp);
            else
                return null;
        }

        public static DateTime PreviousMonday(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }
        public static DateTime NextMonday(this DateTime dt, DayOfWeek startOfWeek)
        {
            return dt.PreviousMonday(startOfWeek).AddDays(7);
        }

        public static DateTime PreviousTopOfTheHour(this DateTime dt)
        {
            return dt.AddMinutes(-1 * dt.Minute).AddSeconds(-1 * dt.Second).AddMilliseconds(-1 * dt.Millisecond);
        }
        public static DateTime NextTopOfTheHour(this DateTime dt)
        {
            return dt.PreviousTopOfTheHour().AddHours(1);
        }


        public static int ToUnixTimeInSec(this DateTime dateInUTC)
        {
            return
                System.Convert.ToInt32(
                (dateInUTC - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds)
                ;
        }

        static System.Globalization.CultureInfo enCulture = new System.Globalization.CultureInfo("en-US");
        static DateTime epochTimeBegining = DateTime.MinValue.AddYears(1969);


        public static long ToEpochTimeFromUTC(this DateTime date)
        {
            return (long)((date.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds / 1000d);
        }

        public static DateTime FromEpochTimeToUTC(this long value)
        {
            return new DateTime(epochTimeBegining.AddMilliseconds(value * 1000d).Ticks, DateTimeKind.Utc);
        }

        public static DateTime ToUTCDateTime(this long value)
        {
            return new DateTime(value, DateTimeKind.Utc);
        }

        public static DateTime ToLocalDateTime(this long value)
        {
            return ToLocalDateTime(value, TimeZoneInfo.Local);
        }
        public static DateTime ToLocalDateTime(this long value, System.TimeZoneInfo tzi)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                new DateTime(value, DateTimeKind.Utc)
            , tzi);
        }


        public static int Diff(this DateTime? instance, DateTime date, DateTimeDiff compare)
        {
            if (instance == null)
                return 0;

            switch (compare)
            {
                case DateTimeDiff.Day:
                    {
                        TimeSpan span = date.Subtract(instance.Value);
                        return span.Days;
                    }

                case DateTimeDiff.Hour:
                    {
                        TimeSpan span = date.Subtract(instance.Value);

                        int hours = span.Hours;
                        hours += span.Days * 24;

                        return hours;
                    }
            }

            return 0;
        }

        public static DateTime? ParseDateTime(string date, DateTime? valueIfNull)
        {
            DateTime parsed;

            return DateTime.TryParse(date, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out parsed) ? parsed : valueIfNull;
        }


        public static DateTime SqlMinDate(this DateTime value)
        {
            return new DateTime(1753, 1, 1); // 01/01/1753 
        }

        public static bool IsSqlMinDate(this DateTime value)
        {
            return (value.CompareTo(DateTime.UtcNow.SqlMinDate()) == 0); // 01/01/1753 
        }

        public static DateTime SqlMaxDate(this DateTime value)
        {
            return new DateTime(9999, 12, 31, 23, 59, 59, 997); //9999-12-31 23:59:59.997 
        }

        public static bool IsSqlMaxDate(this DateTime value)
        {
            return (value.CompareTo(DateTime.UtcNow.SqlMaxDate()) == 0); // 01/01/1753 
        }
        public static string Ago(this DateTime? date)
        {
            return date.Ago(System.Threading.Thread.CurrentThread.CurrentUICulture, string.Empty);
        }
        public static string Ago(this DateTime? date, CultureInfo culture, string defaultString)
        {
            if (date.HasValue)
                return date.Value.Ago(culture);
            else
                return defaultString;
        }

        public static string Ago(this DateTime date)
        {
            return date.Ago(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }
        public static string Ago(this DateTime date, CultureInfo culture)
        {
            TimeSpan ts = DateTime.UtcNow - date.ToUniversalTime();
            return ts.Ago(culture);
        }


        public static string Ago(this TimeSpan interval, CultureInfo culture)
        {
            string timeago = string.Empty;

            TimeSpan ts = interval;
            int absDays = Math.Abs(ts.Days);
            int absHours = Math.Abs(ts.Hours);
            if (absDays < 1 && absHours < 6) //near
            {
                if (ts.TotalHours < 0)
                    timeago = rm.GetString("InHours" + (-ts.Hours).ToString(), culture);
                else
                    timeago = rm.GetString("HoursAgo" + ts.Hours.ToString(), culture);
            }
            else if (absDays < 8)
            {
                if (ts.Days < 0)
                    timeago = rm.GetString("InDays" + (-ts.Days).ToString(), culture);
                else
                    timeago = rm.GetString("DaysAgo" + ts.Days.ToString(), culture);
            }
            else
                if (ts.Days < 0)
                timeago = string.Format(rm.GetString("InDaysMore", culture), -ts.Days);
            else
                timeago = string.Format(rm.GetString("DaysAgoMore", culture), ts.Days);


            return timeago;
        }


        public static TimeSpan AgoIntervalNormalization(this DateTime date)
        {

            TimeSpan ts = DateTime.UtcNow - date.ToUniversalTime();
            int absDays = Math.Abs(ts.Days);
            int absHours = Math.Abs(ts.Hours);
            if (absDays < 1 && absHours < 6) //near
            {
                if (ts.TotalHours < 0)
                    return TimeSpan.FromHours(-1 * absHours);
                else
                    return TimeSpan.FromHours(absHours);
            }
            else
            {
                if (ts.TotalHours < 0)
                    return TimeSpan.FromDays(-1 * absDays);
                else
                    return TimeSpan.FromDays(absDays);
            }
        }

        public static string GetFormatedDateForTrip(DateTime? departureDate, DateTime? returnDate, DateTime? firstArticleDate)
        {
            return GetFormatedDateForTrip(departureDate, returnDate, firstArticleDate, System.Globalization.CultureInfo.CurrentUICulture);
        }

        public static string GetFormatedDateForTrip(DateTime? departureDate, DateTime? returnDate, DateTime? firstArticleDate, System.Globalization.CultureInfo culture)
        {
            if (culture.IsNeutralCulture)
                culture = Devmasters.TextUtil.GetSpecificCulture(culture);
            string ret = string.Empty;
            if (departureDate != null || returnDate != null)
            {
                if (departureDate != null)
                    if (returnDate == null)
                        ret = string.Format(rm.GetString("SinceDate", culture), departureDate.Value.ToShortDateString());
                    else
                        ret += departureDate.Value.ToShortDateString() + " ~ " + returnDate.Value.ToShortDateString();
                else if (returnDate != null)
                    ret = returnDate.Value.ToShortDateString();
            }
            else
            {
                if (firstArticleDate != null)
                    ret = string.Format(rm.GetString("SinceDate", culture), firstArticleDate.Value.ToShortDateString());
                else
                    ret = rm.GetString("NotStartedYet", culture);
            }
            return ret;

        }


        static NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = ":" };
        public static string ToGMTString(this TimeZoneInfo timezone)
        {

            string tzn = "GMT";
            if (timezone.BaseUtcOffset.Hours > 0)
                tzn += "+" + string.Format("{0:00}:{1:00}", timezone.BaseUtcOffset.Hours, timezone.BaseUtcOffset.Minutes);
            else if (timezone.BaseUtcOffset.Hours < 0)
                tzn += string.Format("{0:00}:{1:00}", timezone.BaseUtcOffset.Hours, Math.Abs(timezone.BaseUtcOffset.Minutes));
            return tzn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="template">Have to contains {0}{1}{2}</param>
        /// <param name="timezone"></param>
        /// <returns></returns>
        public static string ToFormatedStringWithTimezone(DateTime? date, string template, TimeZoneInfo timezone, bool showTimezone, CultureInfo culture)
        {
            if (!date.HasValue)
                return string.Empty;

            if (culture.IsNeutralCulture)
                culture = Devmasters.TextUtil.GetSpecificCulture(culture);

            string publishDate = date.Value.ToString("d", culture);
            string publishTime = date.Value.ToString("t", culture);

            if (showTimezone)
            {

                return string.Format(template,
                        publishDate,
                        publishTime,
                        timezone.ToGMTString()
                    );
            }
            else
                return string.Format(template,
                        publishDate,
                        publishTime,
                        string.Empty
                    );

        }

        public static string ToFormatedStringWithTimezone(DateTime? date, string template, TimeZoneInfo timezone, bool showTimezone)
        {
            return ToFormatedStringWithTimezone(date, template, timezone, showTimezone, System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        public static string ToFormatedStringWithTimezone(DateTime? date, TimeZoneInfo timezone, bool showTimezone)
        {
            return ToFormatedStringWithTimezone(date, "{0} {1} {2}", timezone, showTimezone, System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        public static TimeZoneInfo GMT_TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

        public static int[] CalendarYears = Enumerable.Range(2017, 5).ToArray();
        public static Dictionary<int, DateTime[]> Svatky = new Dictionary<int, DateTime[]>();
        public static Dictionary<int, DateTime[]> Vikendy = new Dictionary<int, DateTime[]>();

        public static Dictionary<int, DateTime[]> NepracovniDny = new Dictionary<int, DateTime[]>();

        public enum TypDne
        {
            Pracovni = 0,
            Vikend = 1,
            Svatek = 2
        }


        public enum DateTimePart
        {
            Year = 1,
            Month = 2,
            Day = 3,
            Hour = 4,
            Minute = 5,
            Second = 6
        }


        public static DateTime FirstDayOfMonth(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);
        }


        // https://cs.wikipedia.org/wiki/Přestupný_rok
        static int[] prestupneRoky = new int[] { 1904, 2000, 2096, 2196, 2296, 2396, 1908, 2004, 2104, 2204, 2304, 2400, 1912, 2008, 2108, 2208, 2308, 2404, 1916, 2012, 2112, 2212, 2312, 2408, 1920, 2016, 2116, 2216, 2316, 2412, 1924, 2020, 2120, 2220, 2320, 2416, 1928, 2024, 2124, 2224, 2324, 2420, 1932, 2028, 2128, 2228, 2328, 2424, 1936, 2032, 2132, 2232, 2332, 2428, 1940, 2036, 2136, 2236, 2336, 2432, 1944, 2040, 2140, 2240, 2340, 2436, 1948, 2044, 2144, 2244, 2344, 2440, 1952, 2048, 2148, 2248, 2348, 2444, 1956, 2052, 2152, 2252, 2352, 2448, 1960, 2056, 2156, 2256, 2356, 2452, 1964, 2060, 2160, 2260, 2360, 2456, 1968, 2064, 2164, 2264, 2364, 2460, 1972, 2068, 2168, 2268, 2368, 2464, 1976, 2072, 2172, 2272, 2372, 2468, 1980, 2076, 2176, 2276, 2376, 2472, 1984, 2080, 2180, 2280, 2380, 2476, 1988, 2084, 2184, 2284, 2384, 2480, 1992, 2088, 2188, 2288, 2388, 2484, 1996, 2092, 2192, 2292, 2392, 2488 };
        public static bool IsPrestupnyRok(int rok)
        {
            return prestupneRoky.Contains(rok);
        }

        public static TypDne TypeOfDay(DateTime date)
        {
            if (CalendarYears.Contains(date.Year))
            {
                if (Svatky[date.Year].Contains(date))
                    return TypDne.Svatek;

            }

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Saturday)
                return TypDne.Vikend;
            else
                return TypDne.Pracovni;
        }

        public static DateTime? LowerDate(DateTime? d1, DateTime? d2)
        {
            if (d1.HasValue == false || d2.HasValue == false)
                return null;
            else if (d1.Value < d2.Value)
                return d1;
            else
                return d2;
        }
        public static DateTime? HigherDate(DateTime? d1, DateTime? d2)
        {
            if (d1.HasValue == false || d2.HasValue == false)
                return null;
            else if (d1.Value > d2.Value)
                return d1;
            else
                return d2;
        }


        public static string DateDiffShort(DateTime first, DateTime sec, string beforeTemplate = "{0}", string afterTemplate = "{0}")
        {
            if (first < DateTime.MinValue)
                first = DateTime.MinValue;
            if (sec < DateTime.MinValue)
                sec = DateTime.MinValue;
            if (first > DateTime.MaxValue)
                first = DateTime.MaxValue;
            if (sec > DateTime.MaxValue)
                sec = DateTime.MaxValue;

            bool after = first > sec;
            DateTimeSpan dateDiff = DateTimeSpan.CompareDates(first, sec);
            string txtDiff = string.Empty;
            if (dateDiff.Years > 0)
            {
                txtDiff = Devmasters.Lang.Plural.Get(dateDiff.Years, "rok;{0} roky;{0} let");
            }
            else if (dateDiff.Months > 3)
            {
                txtDiff = Devmasters.Lang.Plural.Get(dateDiff.Months, "měsíc;{0} měsíce;{0} měsíců");
            }
            else
            {
                txtDiff = Devmasters.Lang.Plural.GetWithZero(dateDiff.Days, "dnes", "den", "{0} dny", "{0} dnů");
            }

            if (after)
                return string.Format(afterTemplate, txtDiff);
            else
                return string.Format(beforeTemplate, txtDiff);

        }

        public static string FormatIntervalSinglePart(TimeSpan ts, DateTimePart minDatePart, string numFormat = "N1")
        {

            var end = DateTime.Now;
            DateTimeSpan dts = DateTimeSpan.CompareDates(end - ts, end);
            string s = "";
            if (dts.Years > 0 && minDatePart > DateTimePart.Year)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Years, "{0} rok;{0} roky;{0} let");
            }
            else if (dts.Years > 0)
            {
                decimal part = dts.Years + dts.Months / 12m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} let", part);
                else
                    s += Devmasters.Lang.Plural.Get((int)part, " {0} rok; {0} roky; {0} let"); ;
                return s;
            }

            if (dts.Months > 0 && minDatePart > DateTimePart.Month)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Months, "{0} měsíc;{0} měsíce;{0} měsíců");
            }
            else if (dts.Months > 0)
            {
                decimal part = dts.Months + dts.Days / 30m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} měsíců", part);
                else
                    s += Devmasters.Lang.Plural.Get((int)part, " {0} měsíc; {0} měsíce; {0} měsíců"); ;
                return s;
            }

            if (dts.Days > 0 && minDatePart > DateTimePart.Day)
            {
                s = " " + Devmasters.Lang.Plural.Get(dts.Days, " {0} den;{0} dny;{0} dnů");
            }
            else if (dts.Days > 0)
            {
                decimal part = dts.Days + dts.Hours / 24m;
                if (part % 1 > 0)
                    s = " " + string.Format(" {0:" + numFormat + "} dní", part);
                else
                    s = " " + Devmasters.Lang.Plural.Get((int)part, " {0} den;{0} dny;{0} dnů"); ;
                return s;
            }

            if (dts.Hours > 0 && minDatePart > DateTimePart.Hour)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Hours, " {0} hodinu;{0} hodiny;{0} hodin");
            }
            else if (dts.Hours > 0)
            {
                decimal part = dts.Hours + dts.Minutes / 60m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} hodin", part);
                else
                    s += " " + Devmasters.Lang.Plural.Get((int)part, " {0} hodinu;{0} hodiny;{0} hodin");
                return s;
            }

            if (dts.Minutes > 0 && minDatePart > DateTimePart.Minute)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Minutes, "minutu;{0} minuty;{0} minut");
            }
            else if (dts.Minutes > 0)
            {
                decimal part = dts.Minutes + dts.Seconds / 60m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} minut", part);
                else
                    s += " " + Devmasters.Lang.Plural.Get((int)part, "minutu;{0} minuty;{0} minut"); ;
                return s;
            }

            if (dts.Seconds > 0 && minDatePart > DateTimePart.Second)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Seconds, "sekundu;{0} sekundy;{0} sekund");
            }
            else
            {
                decimal part = dts.Seconds + dts.Milliseconds / 1000m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} sekund", part);
                else
                    s += " " + Devmasters.Lang.Plural.Get((int)part, "sekundu;{0} sekundy;{0} sekund"); ;
                return s;
            }

            //if (dts.Milliseconds > 0)
            //    s += " " + Devmasters.Lang.Plural.Get(dts.Milliseconds, "{0} ms;{0} ms;{0} ms");

            return s.Trim();

        }

        public static string FormatInterval(TimeSpan ts)
        {
            return FormatInterval(ts, System.Globalization.CultureInfo.CurrentUICulture);
        }
        public static string FormatInterval(TimeSpan ts, System.Globalization.CultureInfo culture)
        {
            var end = DateTime.Now;
            DateTimeSpan dts = DateTimeSpan.CompareDates(end - ts, end);
            string s = "";
            if (dts.Years > 0)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Years,culture, "rok","{0} roky","{0} let");
            }
            if (dts.Months > 0)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Months, "měsíc;{0} měsíce;{0} měsíců", culture);
            }
            if (dts.Days > 0)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Days, "den;{0} dny;{0} dnů", culture);
            }
            if (dts.Hours > 0)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Hours, "hodinu;{0} hodiny;{0} hodin", culture);
            }
            if (dts.Minutes > 0)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Minutes, "minutu;{0} minuty;{0} minut", culture);
            }
            if (dts.Seconds > 0)
            {
                s += " " + Devmasters.Lang.Plural.Get(dts.Seconds, "sekundu;{0} sekundy;{0} sekund", culture);
            }
            if (dts.Milliseconds > 0)
                s += " " + Devmasters.Lang.Plural.Get(dts.Milliseconds, "{0} ms;{0} ms;{0} ms", culture);

            return s.Trim();

        }


        public static bool IsDateInInterval(DateTime? from, DateTime? to, DateTime? date)
        {
            if (date == null)
                return false;
            else if (
                        (from <= date && date <= to)
                        || (from == null && date <= to)
                        || (date <= to && to == null)
                    )
                return true;
            else
                return false;
        }

        public static bool IsContinuingIntervals(DateTime? dateIntervalFrom,
                                            DateTime? dateIntervalTo,
                                            DateTime? dateRelFrom,
                                            DateTime? dateRelTo)
        {
            if (IsOverlappingIntervals(dateIntervalFrom, dateIntervalTo, dateRelFrom, dateRelTo))
                return true;
            DateTime? int1End = null;
            DateTime? int2Start = null;
            if (dateIntervalTo < dateRelFrom)
            {
                int1End = dateIntervalTo;
                int2Start = dateRelFrom;
            }
            else
            {
                int1End = dateRelTo;
                int2Start = dateIntervalFrom;
            }
            int1End = int1End ?? new DateTime(1980, 1, 1);
            int2Start = int1End ?? new DateTime(1990, 1, 1);

            if (int1End == new DateTime(1980, 1, 1) || int2Start == new DateTime(1990, 1, 1))
                System.Diagnostics.Debugger.Break();

            var days = Math.Abs((int2Start.Value - int1End.Value).TotalDays);
            return (days <= 1);
        }

        //based on sql dbo.IsSomehowInInterval
        public static bool IsOverlappingIntervals(DateTime? dateIntervalFrom,
                                            DateTime? dateIntervalTo,
                                            DateTime? dateRelFrom,
                                            DateTime? dateRelTo)
        {
            int oks = 0;

            if (dateIntervalFrom is null && dateIntervalTo is null)
                return true;
            if (dateRelFrom is null && dateRelTo is null)
                return true;

            if (dateRelFrom is null)
                dateRelFrom = new DateTime(1900, 01, 01);
            if (dateRelTo is null)
                dateRelTo = DateTime.Now.AddDays(1000);


            if (IsDateInInterval(dateRelFrom, dateRelTo, dateIntervalFrom))
                oks = oks + 1;

            if (IsDateInInterval(dateRelFrom, dateRelTo, dateIntervalTo))
                oks = oks + 1;

            if (dateIntervalFrom <= dateRelFrom && dateRelTo <= dateIntervalTo
                && dateIntervalFrom != null && dateRelFrom != null && dateRelTo != null && dateIntervalTo != null
                )
                oks = oks + 1;

            if (oks == 0
                && dateIntervalFrom is null
                && dateIntervalTo > dateRelTo
                )
                oks = oks + 1;

            if (oks == 0
                && dateIntervalTo is null
                && dateIntervalFrom < dateRelFrom
                )
                oks = oks + 1;

            if (oks > 0)
                return true;
            else
                return false; ;

        }

    }
}