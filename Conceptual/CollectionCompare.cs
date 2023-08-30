using System;
using System.Collections.Generic;
using System.Linq;

namespace Basic
{
    class CollectionCompare
    {
        public int Test()
        {
            var dic = new Dictionary<int, string>();
            for (int i = 0; i < 20000; i++)
            {
                dic.Add(i, i.ToString());
            }

            var list = dic.Where(f => f.Value.StartsWith("1")).Select(f => f.Key); //......................1
            Console.WriteLine(list.GetType());
            var list2 = dic.Where(f => list.Contains(f.Key)).ToList(); //..................................2
            Console.WriteLine(list2.Count());

            //-----------------------------------------------------------------------------

            //The logic behind the second Where without ToList looks like this:

            // The logic is expanded for illustration only.
            var list2_a = new List<KeyValuePair<int, string>>();
            foreach (var d in dic)
            {
                var list_a = new List<int>();
                // This nested loop does the same thing on each iteration,
                // redoing n times what could have been done only once.
                foreach (var f in dic)
                {
                    if (f.Value.StartsWith("1"))
                    {
                        list_a.Add(f.Key);
                    }
                }
                if (list_a.Contains(d.Key))
                {
                    list2_a.Add(d);
                }
            }


            //The logic with ToList looks like this:

            // The list is prepared once, and left alone
            var list_b = new List<int>();
            foreach (var f in dic)
            {
                if (f.Value.StartsWith("1"))
                {
                    list_b.Add(f.Key);
                }
            }
            var list2_b = new List<KeyValuePair<int, string>>();
            // This loop uses the same list in all its iterations.
            foreach (var d in dic)
            {
                if (list.Contains(d.Key))
                {
                    list2_b.Add(d);
                }
            }


            return 0;
        }
    }

}
