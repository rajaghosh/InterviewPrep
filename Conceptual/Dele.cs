using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    class Lambda
    {
        public delegate int delFunc(int a);
        public delegate int delFunc2(int a, int b);
        public delegate void delFunc3(int a, int b);

        public void Method1()
        {
            delFunc df = delegate (int a)
            {
                Console.WriteLine("Inside the delegate anonymous method");
                return a * 2;
            };
            var a2 = df(10);
            Console.WriteLine(a2);
        }

        public void Method2()
        {
            delFunc df2 = (x) => x * x; //or x=>x*x . Here x => x*x is the expression where x = value stored and x*x is operation,
                                     

            int result = df2(10);

            Console.WriteLine(result);

        }

        public void Method3()
        {
            delFunc2 df3 = (x, y) => x * y; //using 2 parameters //(x,y) represents the input and x*y represents the output

            var a3 = df3(2, 5);

            Console.WriteLine(a3);

        }

        public void Method4()
        {
            delFunc3 df4 = (x, y) =>
            {
                Console.WriteLine(x * y);
            };
            df4(5, 10);
        }
    }
}
