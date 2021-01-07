using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devmasters;
using Devmasters.Enums;
using Devmasters.Collections;

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




            var sc = new Devmasters.Lang.CS.WordsToNumbers (
                System.IO.File.ReadAllText(@"c:\!!\text.txt")
                , Devmasters.Lang.CS.WordsToNumbers.ConversionType.TextAndNumber
                , true
                );
            
            
            return;



            return;
            TestAutoUpdatableCacheMem(); return;
            TestAutoUpdatableCacheFile(); return;
            var grps = EnumTools.Groups(typeof(Tester));
            var grpsLiche = EnumTools.InGroup(typeof(Tester), "liche");
            var grpsLiche2 = EnumTools.InGroup<Tester>("liche");
            var grpsLiche2x = EnumTools.InGroup<Tester>("xliche");


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
