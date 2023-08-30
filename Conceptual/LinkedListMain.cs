using System;
using System.Collections.Generic;
using System.Text;

namespace BasicDataStructure
{
    class LinkedListMain
    {
        //We are using .net own Linked List library

        public LinkedListMain()
        {
            Method1();
        }

        public void Method1()
        {
            LinkedList<string> strLL = new LinkedList<string>();

            strLL.AddLast("A1");
            strLL.AddLast("A2");
            strLL.AddLast("A3");
            strLL.AddLast("A3a");
            strLL.AddLast("A4");
            strLL.AddLast("A5");
            strLL.AddLast("A6");
            strLL.AddLast("A7");
            strLL.AddLast("A8");
            strLL.AddLast("A9");




            Console.WriteLine(strLL.First.Value);
            Console.WriteLine(strLL.First.Next.Value);
            Console.WriteLine(strLL.Last.Value);

            foreach(var ll in strLL)
            {
                Console.WriteLine("LL : " + ll);
            }

        }

        public void Method2()
        {
            LinkedList<string> strLL = new LinkedList<string>();

            strLL.AddLast("A1");
            strLL.AddLast("A2");
            strLL.AddLast("A3");
            var data1 = strLL.AddFirst("A3a");
            strLL.AddAfter(data1, "A3b");
            strLL.AddLast("A5");
            strLL.AddLast("A6");
            strLL.AddLast("A7");
            strLL.AddLast("A8");
            strLL.AddLast("A9");




            Console.WriteLine(strLL.First.Value);
            Console.WriteLine(strLL.First.Next.Value);
            Console.WriteLine(strLL.Last.Value);

            foreach (var ll in strLL)
            {
                Console.WriteLine("LL : " + ll);
            }

        }
    }
}
