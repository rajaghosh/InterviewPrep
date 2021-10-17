using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptual2
{
    class ThreadSynchronization2
    {
        //MUTEX
        //Here the thread which Locks can Only Unlock
        //static Mutex _mut = new Mutex();
        //public static void Mutexmethod()
        //{
        //    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} starting...");
        //    _mut.WaitOne();
        //    Thread.Sleep(2000);
        //    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed...");
        //    _mut.ReleaseMutex();
        //}


        //SEMAPHORE - It is not a locking mechanism but a Signaling mechanism
        static Semaphore _sem = new Semaphore(1,1); //(Initial Count of Thread, Max Count of thread to be run in parallel)
        public static void Semaphoremethod()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} starting...");
            _sem.WaitOne();
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed...");
            _sem.Release();
        }

        public ThreadSynchronization2()
        {
            //MUTEX
            //for (int i = 0; i < 5; i++)
            //{
            //    new Thread(Mutexmethod).Start();
            //}

            //SEMAPHORE
            for (int i = 0; i < 10; i++)
            {
                new Thread(Semaphoremethod).Start();
            }
        }
    }
}
