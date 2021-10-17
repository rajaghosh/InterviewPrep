using System;
using System.Threading;

namespace DesignPatterns
{
    // This SingletonPatternThreadSafe implementation is called "double check lock". It is safe
    // in multithreaded environment and provides lazy initialization for the
    // SingletonPatternThreadSafe object.
    class SingletonPatternThreadSafe
    {
        private SingletonPatternThreadSafe() { }

        private static SingletonPatternThreadSafe _instance;

        // We now have a lock object that will be used to synchronize threads
        // during first access to the SingletonPatternThreadSafe.
        private static readonly object _lock = new object();

        public static SingletonPatternThreadSafe GetInstance(string value)
        {
            // This conditional is needed to prevent threads stumbling over the
            // lock once the instance is ready.
            if (_instance == null)
            {
                // Now, imagine that the program has just been launched. Since
                // there's no SingletonPatternThreadSafe instance yet, multiple threads can
                // simultaneously pass the previous conditional and reach this
                // point almost at the same time. The first of them will acquire
                // lock and will proceed further, while the rest will wait here.
                lock (_lock)
                {
                    // The first thread to acquire the lock, reaches this
                    // conditional, goes inside and creates the SingletonPatternThreadSafe
                    // instance. Once it leaves the lock block, a thread that
                    // might have been waiting for the lock release may then
                    // enter this section. But since the SingletonPatternThreadSafe field is
                    // already initialized, the thread won't create a new
                    // object.
                    if (_instance == null)
                    {
                        _instance = new SingletonPatternThreadSafe();
                        _instance.Value = value;
                    }
                }
            }
            return _instance;
        }

        // We'll use this property to prove that our SingletonPatternThreadSafe really works.
        public string Value { get; set; }
    }

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        // The client code.

    //        Console.WriteLine(
    //            "{0}\n{1}\n\n{2}\n",
    //            "If you see the same value, then SingletonPatternThreadSafe was reused (yay!)",
    //            "If you see different values, then 2 SingletonPatternThreadSafes were created (booo!!)",
    //            "RESULT:"
    //        );

    //        Thread process1 = new Thread(() =>
    //        {
    //            TestSingletonPatternThreadSafe("FOO");
    //        });
    //        Thread process2 = new Thread(() =>
    //        {
    //            TestSingletonPatternThreadSafe("BAR");
    //        });

    //        process1.Start();
    //        process2.Start();

    //        process1.Join();
    //        process2.Join();
    //    }

    //    public static void TestSingletonPatternThreadSafe(string value)
    //    {
    //        SingletonPatternThreadSafe SingletonPatternThreadSafe = SingletonPatternThreadSafe.GetInstance(value);
    //        Console.WriteLine(SingletonPatternThreadSafe.Value);
    //    }
    //}

    class SingletonImplementation2
    {
        public SingletonImplementation2(){

            // The client code.
            Console.WriteLine(
                "{0}\n{1}\n\n{2}\n",
                "If you see the same value, then SingletonPatternThreadSafe was reused (yay!)",
                "If you see different values, then 2 SingletonPatternThreadSafes were created (booo!!)",
                "RESULT:"
            );

            Thread process1 = new Thread(() =>
            {
                TestSingletonPatternThreadSafe("FOO");
            });

            Thread process2 = new Thread(() =>
            {
                TestSingletonPatternThreadSafe("BAR");
            });

            process1.Start();
            process2.Start();

            process1.Join();
            process2.Join();

        }

        public static void TestSingletonPatternThreadSafe(string value)
        {
            SingletonPatternThreadSafe SingletonPatternThreadSafe = SingletonPatternThreadSafe.GetInstance(value);
            Console.WriteLine(SingletonPatternThreadSafe.Value);
        }

    }


}
