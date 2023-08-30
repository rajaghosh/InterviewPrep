using System;
using System.Collections.Generic;
using System.Text;

namespace DesignPatterns
{
    //Single Thread Implementation.......
    public sealed class Singleton1
    {

        private static int counter = 0;//This is not mandatory and have been used to just validate
        private Singleton1() //This declaration is important as we want our constructor to be private, else a new instance can be created
        {
            counter++;
            Console.WriteLine("Counter Value -> " + counter);
        }

        //This will be instanciated only once.
        private static Singleton1 _instanceVariable = null; //Basically this is actually the instance variable which will be responsible for all the tasks to be done with this class.

        public static Singleton1 GetInstance //Static Property, this will be used to communicate with outside 
        {
            get 
            {
                if (_instanceVariable == null)
                    _instanceVariable = new Singleton1();
                return _instanceVariable;
            }
        }

        //Derived Nested Class - Instance of the Singleton1 class can be created from
        //DerivedSingleton class so we have made Singleton Class Sealed
        //public class DerivedSingleton : Singleton1
        //{

        //}

        public void PrintDetails(string message)
        {
            Console.WriteLine(message);
        }
    }

    //NOTE - In the class Singleton1Helper we have 2 methods which are actually calling
    public class Singleton1Helper
    {
        public static void PrintEmployee1Details()
        {
            /*
             * Assuming Singleton is created from employee class
             * we refer to the GetInstance property from the Singleton class
             */
            Singleton1 fromEmployee = Singleton1.GetInstance;
            fromEmployee.PrintDetails("From Employee");
        }

        public static void PrintStudent1Details()
        {
            /*
            * Assuming Singleton is created from student class
            * we refer to the GetInstance property from the Singleton class
            */
            Singleton1 fromStudent = Singleton1.GetInstance;
            fromStudent.PrintDetails("From Student");
        }
    }

}

/*NOTE - Referenced from Kudvenkat
 * 
 * This is a basic example and will be good for Single Threaded Environment. But if there is a need for multi-threading then we might have to 
 * implement thread safe implementation.
 * 
 * Its very important the class has to be
 * 1. Private Constructor - So that from outside no instance can be generated.
 * 2. Sealed class - Such that we cannot inherit it. Though there are private constructor, but if nested class is created then we can still generate another instance. So sealed is implemented.
 * 3. A public property which will be accessible outside for the work.
 * 4. A private static type Class instance which will actually do all the internal work
 * 
 * There are some concepts.
 * Here we have public property -> GetInstance. Here we will be delaying the invocation of the the Constructor till the "GetInstance" property is invoked.
 * This is called "Lazy Initialization" and it works fine in Single Threaded Environnment.
 * 
 * We will have violation of Singleton class if we run parallel tasks. This can be easily seen when we invoke PrintEmployee2Details() and PrintStudent1Details() 
 * parallely. Here we can see the 2 instances will be created. So there is race condition.
 * 
 * 
 */
