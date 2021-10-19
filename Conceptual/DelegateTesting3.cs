using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class DelegateTesting3
    {
        public delegate double CalAreaPointer(int a);

        public double CalculateArea(int a) 
        {
            return 3.14 * a * a;
        }

        public static double CalculateSquare(int r) 
        {
            return r * r;
        }

        //As there can be issue with initiation so static will be assigned here
        static CalAreaPointer _calSqr = CalculateSquare;

        public DelegateTesting3()
        {
            CalAreaPointer _calArea = CalculateArea;
            double val1 = _calArea(3);

            Console.WriteLine(val1);

            double val2 = _calSqr(4);
            double val3 = _calSqr.Invoke(4); //Same as above

            Console.WriteLine(val2 + " " + val3);
        }

    }
}
