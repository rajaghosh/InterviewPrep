using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    interface ISum
    {
        int Sum(int a, int b);
        int Mul(int a, int b);
    }

    interface IMul
    {
        //Duplicate - Just for Testing
        int Mul(int a, int b);
    }

    public class TestClassInterface : ISum, IMul
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }

        //Here its a common name between the 2 interfaces - The child class just need to know the interface pattern and not from it comes from
        public int Mul(int a, int b)
        {
            return a * b;
        }
    } 
}
