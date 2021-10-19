using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class DelegateHelper
    {
    }

    public class Calculator
    {
        public delegate int Calculate(int input);

        public Calculator()
        {

        }

        public int Execute(Calculate calc, int data)
        {
            return calc(data);
        }
    }
}
