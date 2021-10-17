using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class DelegateTesting
    {
        public delegate int MultiplierDelegate(int i, int j);

        public int MulFunc(int a, int b)
        {
            return a * b;
        }

        public DelegateTesting()
        {
            MultiplierDelegate md = new MultiplierDelegate(MulFunc);
            int res = md(10, 20);
            Console.WriteLine(res);
        }
    }
}
