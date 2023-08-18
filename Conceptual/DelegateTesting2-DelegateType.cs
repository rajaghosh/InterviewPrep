using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class DelegateTesting2
    {
        //DelegateTesting2-DelegateType
        public DelegateTesting2()
        {
            //******************** BLOCK 1 **************************************************
            //Normal delegate to LAMBDA EXPRESSION (shorter version of delegate)

            //Normal delegate call
            double area = areadel(20);
            Console.WriteLine(area);

            //For the code above using anonymous methods
            CalcAreaDel area2del = new CalcAreaDel(delegate (int r)
            {
                return 3.14 * r * r;
            });
            double area2 = area2del(30);
            Console.WriteLine(area2);

            //Lambda expression for the above code - It is more shorter version of delegate
            CalcAreaDel area3del = r => 3.14 * r * r; // It has 2 parts "r" and "3.14 * r * r"
            double area3 = area2del(40);
            Console.WriteLine(area3);

            //******************** BLOCK 2 **************************************************
            //Generic delegates

            //Func <with input as double, output as double>
            //- It will take an input and produce and output (it might not have any input)
            Func<double,double> cptr1 = r => 3.14 * r * r;
            double area4 = cptr1(50);
            Console.WriteLine(area4);

            //Action
            //This will take an input and do some work (and may or may not return any output)
            Action<string> cptr2 = y => Console.WriteLine(y);
            cptr2("Hello world");

            //Predicate
            //This will take an input and will return and output as boolean
            Predicate<string> cptr3 = y => y.Length > 5;
            Console.WriteLine(cptr3("Hello World")); 

        }

        public delegate double CalcAreaDel(int r);
        static CalcAreaDel areadel = CalculateArea;
        static double CalculateArea(int r)
        {
            return 3.14 * r * r;
        }

    }
}

/*
 
    To make the delegate code more simpler we have 4 mechanism :
    1. Lambda expression
    2. Action expression
    3. Func expression
    4. Predicate expression

    This is particularly useful to use with List
 
 */