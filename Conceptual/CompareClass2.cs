using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic
{
    class CompareClass2
    {
        public CompareClass2()
        {

        }

        static List<int> list = MakeList();
        static IList<int> iList = MakeList();
        static ICollection<int> iCollection = MakeList();
        static IEnumerable<int> iEnumerable = MakeList();


        public static TimeSpan Measure(Action f)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            f();
            stopWatch.Stop();
            return stopWatch.Elapsed;
        }

        public static List<int> MakeList()
        {
            var list = new List<int>();
            for (int i = 0; i < 100; ++i)
            {
                list.Add(i);
            }
            return list;
        }

        public void Test()
        {
            var time1 = Measure(() =>
            { // Measure time of enumerating List<int>
                for (int i = 1000000; i > 0; i--)
                {
                    foreach (var item in list)
                    {
                        var x = item;
                    }
                }
            });
            Console.WriteLine($"List<int> time: {time1}");

            var time2 = Measure(() =>
            { // IList<int>
                for (int i = 1000000; i > 0; i--)
                {
                    foreach (var item in iList)
                    {
                        var x = item;
                    }
                }
            });
            Console.WriteLine($"IList<int> time: {time2}");

            var time3 = Measure(() =>
            { // ICollection<int>
                for (int i = 1000000; i > 0; i--)
                {
                    foreach (var item in iCollection)
                    {
                        var x = item;
                    }
                }
            });
            Console.WriteLine($"ICollection<int> time: {time3}");

            var time4 = Measure(() =>
            { // IEnumerable<int>
                for (int i = 1000000; i > 0; i--)
                {
                    foreach (var item in iEnumerable)
                    {
                        var x = item;
                    }
                }
            });
            Console.WriteLine($"IEnumerable<int> time: {time4}");
        }
    }
}



