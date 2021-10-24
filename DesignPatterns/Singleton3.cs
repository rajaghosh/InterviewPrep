using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DesignPatterns
{
    //Single Thread Eager Vs Lazy Loading Singleton Classes
    class Singleton3
    {
        public Singleton3()
        {
            Parallel.Invoke(
                new ParallelOptions() { MaxDegreeOfParallelism = 3 },
                () => Singleton3Helper.PrintEmployee3Eager(),
                () => Singleton3Helper.PrintStudent3Eager()
                );

            Parallel.Invoke(
                new ParallelOptions() { MaxDegreeOfParallelism = 2 },
                () => Singleton3Helper.PrintEmployee3Lazy(),
                () => Singleton3Helper.PrintStudent3Lazy()
                ); ;
        }

    }

    //Eager Lading and Thread Safety - The advantage of using Eager Loading in the Singleton design pattern
    //is that the CLR (Common Language Runtime) will take care of object initialization and thread-safety.
    //That means we will not require to write any code explicitly for handling the thread-safety for a multithreaded environment.
    class SingletonEagerLoading
    {
        private int counter = 0;
        private SingletonEagerLoading()
        {
            counter++;
            Console.WriteLine("EagerLoading Counter : " + counter);
        }

        private static readonly SingletonEagerLoading _eagerLoadingInstance = new SingletonEagerLoading();

        public static SingletonEagerLoading EagerLoadingInstance
        {
            get
            {
                return _eagerLoadingInstance;
            }
        }

        public void PrintDetails(string str)
        {
            Console.WriteLine(str);
        }
    }


    //Lazy Loading - The lazy keyword which was introduced as part of .NET Framework 4.0 provides
    //the built-in support for lazy initialization i.e. on-demand object initialization.
    //The most important point that you need to remember is the Lazy<T> objects are by default thread-safe.
    //In a multi-threaded environment, when multiple threads are trying to access the same Get Instance
    //property at the same time, then the lazy object will take care of thread safety.
    class SingletonLazyLoading
    {
        private int counter = 0;
        private SingletonLazyLoading()
        {
            counter++;
            Console.WriteLine("LazyLoading Counter : " + counter);
        }

        //The declare we use Lazy<> = to initialize we use Lazy<>(() => CONSTRUCTOR)
        private static readonly Lazy<SingletonLazyLoading> _lazyLoadingInstance = new Lazy<SingletonLazyLoading>(() => new SingletonLazyLoading());

        public static SingletonLazyLoading LazyLoadingInstance
        {
            get
            {
                return _lazyLoadingInstance.Value; //To access the value
            }
        }

        public void PrintDetails(string str)
        {
            Console.WriteLine(str);
        }
    }

    public class Singleton3Helper
    {
        public static void PrintEmployee3Eager()
        {
            SingletonEagerLoading fromEmployee = SingletonEagerLoading.EagerLoadingInstance;
            fromEmployee.PrintDetails("From Employee");
        }

        public static void PrintStudent3Eager()
        {
            SingletonEagerLoading fromStudent = SingletonEagerLoading.EagerLoadingInstance;
            fromStudent.PrintDetails("From Student");
        }

        public static void PrintEmployee3Lazy()
        {
            SingletonLazyLoading fromEmployee = SingletonLazyLoading.LazyLoadingInstance;
            fromEmployee.PrintDetails("From Employee");
        }

        public static void PrintStudent3Lazy()
        {
            SingletonLazyLoading fromStudent = SingletonLazyLoading.LazyLoadingInstance;
            fromStudent.PrintDetails("From Student");
        }
    }
}

/*
 * We can have 2 variety of Singleton Class Implementation 
 * Eager and Lazy Loading.
 * 
 * Lazy Loading - 
 * 1. Improves the performance.
 * 2. Avoids unncessary load till the point object is accessed.
 * 3. Reduce the memory footprint on the start-up.
 * 4. Faster application load.
 * 
 * Non-Lazy or Eager Loading
 * 1. Pre-Instantiation of the object.
 * 2. Commonly used in lower memory footprints
 * 
 * 
 */