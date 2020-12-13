using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Devmasters.Enums;

namespace Playground
{
    class Program
    {

        [ShowNiceDisplayName()]
        [Groupable]
        public enum Tester
        {
            [GroupValue("jine")]
            zero,
            [GroupValue("liche")]
            one,
            [GroupValue("sude")]
            two,
            [GroupValue("liche")]
            three,
            [GroupValue("sude")]
            four
        }

        public class TestClass
        {
            public class Inner { public string inProp { get; set; } = "inner1"; public int year { get; set; } = 2020; }
            public int number { get; set; } = 10;
            public string text { get; set; } = "asdfjj !! ěščřřžžýáí";
            public string[] arrayT { get; set; } = new string[] { "one", "two", "tři" };
            public DateTime DayBefore { get; set; } = DateTime.Now.AddDays(-1);

            public Inner[] arrayInn { get; set; } = new Inner[] { new Inner(), new Inner() { inProp = "inner2" } };
            public Inner innerObj { get; set; } = new Inner();

        }

        static void Main(string[] args)
        {
            var s = replaceNumbers("fifteen million fifty three thousand twenty nine point zero eight five eight oh two");
            return;



            return;
            TestAutoUpdatableCacheMem(); return;
            TestAutoUpdatableCacheFile(); return;
            var grps = EnumTools.Groups(typeof(Tester));
            var grpsLiche = EnumTools.InGroup(typeof(Tester), "liche");
            var grpsLiche2 = EnumTools.InGroup<Tester>("liche");
            var grpsLiche2x = EnumTools.InGroup<Tester>("xliche");


        }


        // usnesení číslo dvě stě třicet pět lomeno dvacet lomeno dvacet rada 
        //based on https://stackoverflow.com/a/17324733 
        public static String[] DIGITS = { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        public static String[] TENS = { null, "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
        public static String[] TEENS = { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        public static String[] MAGNITUDES = { "hundred", "thousand", "million", "point" };
        public static String[] ZERO = { "zero", "oh" };

        public static String replaceNumbers(String input)
        {
            String result = "";
            String[] decim = input.Split(new string[] { MAGNITUDES[3] }, StringSplitOptions.None);
            String[] millions = decim[0].Split(new string[] { MAGNITUDES[2] }, StringSplitOptions.None);

            for (int i = 0; i < millions.Length; i++)
            {
                String[] thousands = millions[i].Split(new string[] { MAGNITUDES[1] }, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < thousands.Length; j++)
                {
                    int[] triplet = { 0, 0, 0 };
                    string[] set = thousands[j].Split(" ,.;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    int currToken = 0;

                    if (set.Length == 1)
                    { //If there is only one token given in triplet
                        String uno = set[currToken]; currToken++;
                        triplet[0] = 0;
                        for (int k = 0; k < DIGITS.Length; k++)
                        {
                            if (uno == DIGITS[k])
                            {
                                triplet[1] = 0;
                                triplet[2] = k + 1;
                            }
                            if (uno == TENS[k])
                            {
                                triplet[1] = k + 1;
                                triplet[2] = 0;
                            }
                            if (uno == TEENS[k])
                            {
                                triplet[1] = 1;
                                triplet[2] = k;
                            }
                        }
                    }


                    else if (set.Count() == 2)
                    {  //If there are two tokens given in triplet
                        String uno = set[currToken]; currToken++;
                        String dos = set[currToken]; currToken++;
                        if (dos == MAGNITUDES[0])
                        {  //If one of the two tokens is "hundred"
                            for (int k = 0; k < DIGITS.Length; k++)
                            {
                                if (uno == TEENS[k])
                                {
                                    triplet[1] = 1;
                                    triplet[2] = k;
                                }
                                if (uno == DIGITS[k])
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
                            for (int k = 0; k < DIGITS.Length; k++)
                            {
                                if (uno == TENS[k])
                                {
                                    triplet[1] = k + 1;
                                }
                                if (dos == DIGITS[k])
                                {
                                    triplet[2] = k + 1;
                                }
                                if (uno == TEENS[k])
                                {
                                    triplet[1] = 1;
                                    triplet[2] = k;
                                }
                            }
                        }
                    }

                    else if (set.Count() == 3)
                    {  //If there are three tokens given in triplet
                        String uno = set[currToken]; currToken++;
                        String dos = set[currToken]; currToken++;
                        String tres = set[currToken]; currToken++;
                        for (int k = 0; k < DIGITS.Length; k++)
                        {
                            if (uno == DIGITS[k])
                            {
                                triplet[0] = k + 1;
                            }
                            if (tres == DIGITS[k])
                            {
                                triplet[1] = 0;
                                triplet[2] = k + 1;
                            }
                            if (tres == TENS[k])
                            {
                                triplet[1] = k + 1;
                                triplet[2] = 0;
                            }
                            if (uno == TEENS[k] || tres == TEENS[k])
                            {
                                triplet[1] = 1;
                                triplet[2] = k;
                            }

                        }
                    }

                    else if (set.Count() == 4)
                    {  //If there are four tokens given in triplet
                        String uno = set[currToken]; currToken++;
                        String dos = set[currToken]; currToken++;
                        String tres = set[currToken]; currToken++;
                        String cuatro = set[currToken]; currToken++;
                        for (int k = 0; k < DIGITS.Length; k++)
                        {
                            if (uno == DIGITS[k])
                            {
                                triplet[0] = k + 1;
                            }
                            if (cuatro == DIGITS[k])
                            {
                                triplet[2] = k + 1;
                            }
                            if (tres == TENS[k])
                            {
                                triplet[1] = k + 1;
                            }
                            if (uno == TEENS[k])
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
                    String w = decimalDigits[currToken]; currToken++;
                    //System.out.println(w);

                    if (w == ZERO[0] || w == ZERO[1])
                    {
                        result = result + "0";
                    }
                    for (int j = 0; j < DIGITS.Length; j++)
                    {
                        if (w == DIGITS[j])
                        {
                            result = result + (j + 1).ToString();
                        }
                    }

                }
            }

            return result;
        }

        static void TestAutoUpdatableCacheMem()
        {
            var fc = new Devmasters.Cache.Elastic.AutoUpdatebleElasticCache<string>(
                new[] { "http://10.10.100.160:9200", "http://10.10.100.161:9200" }, "DevmastersCache",
                TimeSpan.FromSeconds(1), "keycache",
                (o) =>
                {
                    //System.Threading.Thread.Sleep(500+DateTime.Now.Millisecond / 100);
                    return DateTime.Now.ToString("HH:mm:ss:fff");
                }
                );

            string prev = "";
            for (int i = 0; i < 500; i++)
            {
                var val = fc.Get();
                if (val != prev)
                {
                    Console.Write($"\n{i} {DateTime.Now.ToString("mm:ss:fff")} | {val}");
                    prev = val;
                }
                else
                    Console.Write($".");
                System.Threading.Thread.Sleep(120);
            }

        }

        static void TestAutoUpdatableCacheFile()
        {
            var fc = new Devmasters.Cache.File.AutoUpdatedFileCache<string>(@"c:\!\",
                TimeSpan.FromSeconds(1), "keycache",
                (o) =>
                {
                    //System.Threading.Thread.Sleep(500+DateTime.Now.Millisecond / 100);
                    return DateTime.Now.ToString("HH:mm:ss:fff");
                }
                );

            string prev = "";
            for (int i = 0; i < 500; i++)
            {
                var val = fc.Get();
                if (val != prev)
                {
                    Console.Write($"\n{i} {DateTime.Now.ToString("mm:ss:fff")} | {val}");
                    prev = val;
                }
                else
                    Console.Write($".");
                System.Threading.Thread.Sleep(120);
            }

        }


    }
}
