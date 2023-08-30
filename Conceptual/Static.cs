using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    static class StaticTest
    {
        public static double pi = 3.14f;
        public static void Method1(int a)
        {
            var i = pi * 100 * a;
            Console.WriteLine(i);
        }
    }

    class StaticTest2
    {
        public static int a = 5;
        public static int counter = 0;
        public int b = 4;
        public void Method1()
        {
            Console.WriteLine(a * b);
            Console.WriteLine("Counter : " + counter++);
        }

        public static void Method1(int a1)
        {
            Console.WriteLine(a * a1 );
            Console.WriteLine("Counter : " + ++counter);
        }
    }

    class Print
    {
        private int a1 = 5;
        public int a2;
        public void Print1()
        {
            StaticTest.Method1(5);
        }

        public void Print2()
        {
            StaticTest2 st = new StaticTest2();
            StaticTest2.Method1(10);
            a2 = a1;
            Console.WriteLine(a2);
            st.Method1();
        }
    }
}
