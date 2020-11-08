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
            Devmasters.IO.SimpleZipArchive z = new Devmasters.IO.SimpleZipArchive(@"c:\\!\a.zip", "ina.txt");
            z.Write(DateTime.Now.ToString());
            z.Write("-");
            z.WriteLine("EOF");
            System.Threading.Thread.Sleep(1000);
                        z.Write(DateTime.Now.ToString());
            z.Write("-");
            z.WriteLine("EOF");
            z.Flush();
            z.Dispose();

            return;
            TestAutoUpdatableCacheMem();return;
            TestAutoUpdatableCacheFile();return;
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
            var fc = new Devmasters.Cache.File.AutoUpdatedFileCache<string>(@"c:\!\", 
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
