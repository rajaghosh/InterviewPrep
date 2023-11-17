using System;

namespace DesignPatterns
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //***********Some Other Design Pattern Implementation
            //AggregateRootDesignPattern ardp1 = new AggregateRootDesignPattern();
            //IteratorDesignPattern i1 = new IteratorDesignPattern();
            //AdapterDesignPattern ad1 = new AdapterDesignPattern();
            //TemplateMethodDesignPattern tm1 = new TemplateMethodDesignPattern();

            //BridgeDesignPattern bp1 = new BridgeDesignPattern();
            //BridgeDesignPattern2 bp2 = new BridgeDesignPattern2();




            //***************FROM KUDVENKAT**********************

            //*************Singleton Example 1
            //Singleton1 fromEmployee = Singleton1.GetInstance;
            //fromEmployee.PrintDetails("From Employee");

            //Singleton1 fromStudent = Singleton1.GetInstance;
            //fromStudent.PrintDetails("From Student");


            ////This is a technique by which we can run several tasks in different THREADS "Parallely".
            ////If all the new THREADS are exhausted then there will be re-use of existing threads

            //Parallel.Invoke(

            //    new ParallelOptions() { MaxDegreeOfParallelism = 3 }, //Max 3 threads will be allowed to run. Just as an example as here we could run only 2 methods or 2 threads
            //    () => Singleton1Helper.PrintEmployee1Details(),
            //    () => Singleton1Helper.PrintStudent1Details()

            //    );
            //-------------------------------------------------------

            //*************Singleton Example 2
            //This is a technique by which we can run several tasks in different THREADS "Parallely".
            //If all the new THREADS are exhausted then there will be re-use of existing threads

            //Here we have called the Parallel Methods in thread safe implementation
            //Parallel.Invoke(

            //    new ParallelOptions() { MaxDegreeOfParallelism = 3 }, //Max 3 threads will be allowed to run. Just as an example as here we could run only 2 methods or 2 threads
            //    () => Singleton2Helper.PrintEmployee2Details(),
            //    () => Singleton2Helper.PrintStudent2Details()

            //    );
            //-------------------------------------------------------

            //*************Singleton Example 3
            //Singleton3 s3 = new Singleton3();

            //Factory f1 = new Factory();


            Console.WriteLine("Print Complete");
        }
    }
}
