using System;
using System.Collections.Generic;
using System.Text;

namespace DesignPatterns
{
    //Thread Safe Singleton
    public sealed class Singleton2
    {

        private static int counter = 0;

        private Singleton2()
        {
            counter++;
            Console.WriteLine("Counter Value -> " + counter);
        }

        private static Singleton2 _instanceVariable = null;


        private static readonly object _lockObj = new object();
        public static Singleton2 GetInstance
        {
            get
            {
                //if (_instanceVariable == null) - This is optional. This implementation is called Double Check Locking. Basically here if instance found to be null it will simply avoid move to Lock()
                lock (_lockObj) //Static Lock Object Will be Needed
                {
                    if (_instanceVariable == null)
                        _instanceVariable = new Singleton2();
                }
                return _instanceVariable;
            }
        }

        public void PrintDetails(string message)
        {
            Console.WriteLine(message);
        }
    }



    public class Singleton2Helper
    {
        public static void PrintEmployee2Details()
        {
            /*
             * Assuming Singleton is created from employee class
             * we refer to the GetInstance property from the Singleton class
             */
            Singleton2 fromEmployee = Singleton2.GetInstance;
            fromEmployee.PrintDetails("From Employee");
        }

        public static void PrintStudent2Details()
        {
            /*
            * Assuming Singleton is created from student class
            * we refer to the GetInstance property from the Singleton class
            */
            Singleton2 fromStudent = Singleton2.GetInstance;
            fromStudent.PrintDetails("From Student");
        }
    }

}

/*
 * We know for Single threaded Singleton Class we will instanciating the class only when the GetInstance Property is called. So it was Lazy instanciation
 * Now if there is scenario where many requests come at same point of time to instanciate "GetInstance" property. Then there are chances we might end up 
 * Creating multiple instances of Singleton class. So this could lead to Singleton Class principle violation.
 * 
 * So we are going to use the thread safe version.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */
