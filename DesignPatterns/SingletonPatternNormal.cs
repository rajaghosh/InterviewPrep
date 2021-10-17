using System;

namespace DesignPatterns
{
    // The Singleton class defines the `GetInstance` method that serves as an
    // alternative to constructor and lets clients access the same instance of
    // this class over and over.
    class SingletonPatternNormal
    {
        // The Singleton's constructor should always be private to prevent
        // direct construction calls with the `new` operator.
        private SingletonPatternNormal() { }

        // The Singleton's instance is stored in a static field. There are
        // multiple ways to initialize this field, all of them have various pros
        // and cons. In this example we'll show the simplest of these ways,
        // which, however, doesn't work really well in multithreaded program.
        private static SingletonPatternNormal _instance;

        // This is the static method that controls the access to the singleton
        // instance. On the first run, it creates a singleton object and places
        // it into the static field. On subsequent runs, it returns the client
        // existing object stored in the static field.
        public static SingletonPatternNormal GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SingletonPatternNormal();
            }
            return _instance;
        }

        // Finally, any singleton should define some business logic, which can
        // be executed on its instance.
        public static void someBusinessLogic()
        {
            // ...
        }
    }

    class SingletonImplementation1
    {
        public SingletonImplementation1()
        {
            // The client code.
            SingletonPatternNormal s1 = SingletonPatternNormal.GetInstance();
            SingletonPatternNormal s2 = SingletonPatternNormal.GetInstance();

            if (s1 == s2)
            {
                Console.WriteLine("Singleton works, both variables contain the same instance.");
            }
            else
            {
                Console.WriteLine("Singleton failed, variables contain different instances.");
            }

            Console.WriteLine("ABC..");
        }
    }

}