using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class DelegateTesting
    {
        public delegate int MultiplierDelegate(int i, int j);
        public delegate int TestDelegate1(int a);

        public int MulFunc(int a, int b)
        {
            return a * b;
        }

        TestDelegate1 Td = (x) => x * x;

        public int NormalFunc(int x) => x * x; //This is a normal method with the scope of Delegate

        public DelegateTesting()
        {
            //Test 1
            //MultiplierDelegate md = new MultiplierDelegate(MulFunc);
            //int res = md(10, 20);
            //Console.WriteLine(res);

            //Test 2
            int a1 = Td(10);
            Console.WriteLine(a1);
        }
    }
}

/*
 * Delegate - Reference to a method. 
 * When invoked it can be mapped to any method with compatible signature
 * 
 * 
 * 
 * 
 * 
 */