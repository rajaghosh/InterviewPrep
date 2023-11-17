using System;
using System.Threading;

namespace Conceptual2
{
    class ThreadImplementation
    {
        public ThreadImplementation()
        {
            //By creating threads we will run main program in a main thread and other process on seperate threads
            Thread obj1 = new Thread(Function1);
            Thread obj2 = new Thread(Function2);

            obj1.Start();
            obj2.Start();


            Thread obj3 = new Thread(Function3);
            obj3.IsBackground = true; //making the thread background thread
            obj3.Start();

        }

        private void Function1()
        {
            for(int i = 0; i < 10; i++)
            {
                Console.WriteLine("Function 1 executed : " + i);
                //Wait for 4 seconds
                Thread.Sleep(4000);
            }
        }

        private void Function2()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Function 2 executed : " + i);
                //Wait for 4 seconds
                Thread.Sleep(4000);
            }
        }

        private void Function3()
        {
            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine("Function 3 executed : " + i);
            //    //Wait for 4 seconds
            //    Thread.Sleep(1000);
            //}
            Console.WriteLine("Function 3 is entered...");
            Console.ReadLine();
            Console.WriteLine("Function 3 has exited...");
        }

        
    }
}


//NOTE - 
/*
 * 
 * Threading - Parallel code execution.

We have forground and background threads.
Foreground Threads - Threads which keep on running even if main application exits. 
By default when a thread is created it is a foreground thread.

Background Threads - Threads which will quit if main application quits. 
Note - Even if any foreground thread is running it will keep on running. 
So to test simply test a single background thread.



*/

