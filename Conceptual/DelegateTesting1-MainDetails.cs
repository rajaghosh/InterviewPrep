using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class DelegateTesting1
    {
        //DelegateTesting1-MainDetails
        public DelegateTesting1()
        {
            //Basic delegate
            DelgateMethod1 delMet1 = new DelgateMethod1(Method1);
            
            //Two ways to call
            delMet1.Invoke();   //Process 1 - Call Style 1
            delMet1();          //Process 1 - Call Style 2

            delMet2.Invoke();   //Process 2 - Call Style 1

            //Delegate as a function param
            LongRunning(MyCallbackMethod);

            //Multicast delegate
            DelegateCalc delegateCalc = new DelegateCalc(Mul);
            delegateCalc += Sum;
            delegateCalc(10, 20);

            //Anonymous delegate
            DelegateCalc delegateCalc1 = delegate (int a, int b)
            {
                Console.WriteLine("Anonymous : " + (a + b));
            };
            delegateCalc1(10, 30);
        }

        //Basic delegate
        public delegate void DelgateMethod1();
        public static DelgateMethod1 delMet2 = Method1;
        public static void Method1()
        {   
            Console.WriteLine("Method1");
        }



        //Delegate as a param
        public delegate void DelgateCallback(int i);
        public void LongRunning(DelgateCallback callbackobj)
        {
            for (int i = 0; i < 10; i++)
            {
                callbackobj(i);
            }
        }
        public static void MyCallbackMethod(int i)
        {
            Console.WriteLine(i);
        }

        //Multi cast delegate 
        public delegate void DelegateCalc(int a, int b);
        public static void Sum(int a, int b)
        {
            Console.WriteLine("Sum : " + (a + b));
        }
        public static void Mul(int a, int b)
        {
            Console.WriteLine("Mul : " + a * b);
        }


    }
}

/*
 
 We have delegate shown here -
    1. Normal Delegate 
    2. Delegate as param
    3. Multicast delegate
    4. Anonymous delegate - Delegate used with anonymous methods
 
 */