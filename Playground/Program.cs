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

        static void Main(string[] args)
        {
            Devmasters.TextUtil.FormatPlainTextForArticle(Devmasters.TextUtil.ShortenText(@"Předmětem veřejné zakázky je realizace protipovodňových opatření formou dodávky a montáže varovného a informačního systému a jeho napojení do Jednotného systému varování a informování. 
", 200));
            return;
            TestAutoUpdatableCacheMem();return;
            TestAutoUpdatableCache();return;
            var grps = EnumTools.Groups(typeof(Tester));
            var grpsLiche = EnumTools.InGroup(typeof(Tester), "liche");
            var grpsLiche2 = EnumTools.InGroup<Tester>("liche");
            var grpsLiche2x = EnumTools.InGroup<Tester>("xliche");


        }

        
                static void TestAutoUpdatableCacheMem()
        {
            var fc = new Devmasters.Cache.LocalMemory.AutoUpdatedLocalMemoryCache<string>( 
                TimeSpan.FromSeconds(1),"keycache",
                (o)=> {
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
            var fc = new Devmasters.Cache.File.AutoUpdateFileCache<string>(@"c:\!\", 
                TimeSpan.FromSeconds(1),"keycache",
                (o)=> {
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
