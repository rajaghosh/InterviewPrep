using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class DelegateTesting2
    {
        public int Square(int i) => i * i;

        public DelegateTesting2()
        {
            var calculator = new Calculator();

            //The delegate is present in a different class. We can refer it like a "Static Member" (Not a static member but the syntax is like that)
            Calculator.Calculate calc = Square; //Calc Will be the delegate declaration and "Square" is assigned to it

            var response = calculator.Execute(calc, 5);

            Console.WriteLine($"Calculated Value {response}");
        }
    }
}
