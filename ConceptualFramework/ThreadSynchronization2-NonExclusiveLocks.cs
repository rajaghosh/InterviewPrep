using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptual2
{
    //ThreadSynchronization2-NonExclusiveLocks
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
        static Semaphore _sem = new Semaphore(1, 1); //(Initial Count of Thread, Max Count of thread to be run in parallel)
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


/*
    CONTEXT 

    Mutex and Semaphore in thread, for thread synchronization.

    What is Mutex?
    Mutex works like a lock in C# for thread synchronization, but it works across multiple processes. 
    Mutex provides safety against the external threads.
 
    What is Semaphore?
    Semaphore allows one or more threads to enter and execute their task with thread safety. 
    Object of semaphore class takes two parameters. First parameter explains the number of processes 
    for initial start and the second parameter is used to define the maximum number of processes 
    which can be used for initial start. The second parameter must be equal or greater than the first parameter. 
 
    ------------------------------------------------------------------------
    Difference

    Lock
    A lock only allows one thread to enter the part of code inside the locked scope.

    For example, in the gym, there is one locker shared by multiple users. If someone has already used it, 
    it will be locked, anyone else can not use it until the previous person unlocks it.

    Lock is not shared with any other processes, it can be only used by current process.



    Mutex
    A mutex (Mutual exclusion) is the same as a lock but it can be system wide (shared by multiple processes). 
    It is used to synchronise access to a resource.

    Semaphore
    A semaphore restricts the number of simultaneous users of a shared resource up to a maximum number. 
    Multiple threads can get the access to the resource (decrementing the semaphore), 
    and can signal that they have finished the usage of the resource (incrementing the semaphore).
    For example, everyday the company gym only gives maximum 3 free access cards. 
    The first 3 persons will get the access card. The next person comes, he/she need to wait until any
    of previous 3 persons returned the card.

 */