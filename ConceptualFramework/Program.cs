using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conceptual2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("*****************************************************");

            //Just Create the instance this will run the logic

            //***********Foreground and Background Thread Example
            //ThreadImplementation ti1 = new ThreadImplementation();

            //***********Multithreading Thread Safe Example
            //MultiThreadingThreadSafe mtts1 = new MultiThreadingThreadSafe();

            //***********Thread Synchronization
            //ThreadSynchronization ts1 = new ThreadSynchronization();
            //ThreadSynchronization2 ts2 = new ThreadSynchronization2();

            //***********Task VS Thread
            TaskAndThread tat1 = new TaskAndThread();

            Console.WriteLine("*****************************************************");
            //Testing ts = new Testing();
            Console.ReadKey();
        }
    }
}
