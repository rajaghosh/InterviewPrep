using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptual2
{

    //ThreadSynchronization-ExclusiveLocks
    class ThreadSynchronization
    {
        //------------------------------------------------------------------------------------------
        //--------------------------------------LOCK------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //LOCK - This will force a tread to work in critical section then once completed the next tread will start working

        private static object _locker1 = new object();
        public static void Doworkwithlock()
        {
            lock (_locker1)
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} starting...");
                Thread.Sleep(2000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed...");
            }
        }

        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------




        //------------------------------------------------------------------------------------------
        //--------------------------------------MONITOR---------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //MONITOR - This will work same as Lock + will provide try catch mechanism

        private static object _locker2 = new object();
        public static void DoworkwithMonitor()
        {
            try
            {
                Monitor.Enter(_locker2);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} starting...");
                Thread.Sleep(2000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed...");
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Monitor.Exit(_locker2);
            }
        }

        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------




        //------------------------------------------------------------------------------------------
        //---------------------------------------MANUAL RESET---------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //MANUAL RESET - Here we have they events WaitOne(), Reset() and Set().
        //WaitOne() - This will force the flow to wait till all the block event get completed. | SIGNAL WAIT
        //Reset() - This will initiate a block event in the flow. | SIGNAL STOP
        //Set() - This will mark the block event is completed. | SIGNAL START

        private static ManualResetEvent _mre = new ManualResetEvent(false); //False will indicate there is no signal

        public static void Writer()
        {

            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} starting for Write...");
            _mre.Reset(); //This will cause a block
            Thread.Sleep(5000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed Writing...");
            _mre.Set(); //This will release the block to proceed
        }

        public static void Reader()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} waiting for Read...");
            _mre.WaitOne(); //Waits until the block event gets completed
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed Reading...");
        }

        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------





        //------------------------------------------------------------------------------------------
        //--------------------------------AUTOMATIC RESET-------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //private static AutoResetEvent _are = new AutoResetEvent(true);

        //public static void Method1()
        //{
        //    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} starting...");
        //    _are.WaitOne();
        //    Thread.Sleep(2000);
        //    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed...");
        //    _are.Set();
        //}
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------






        public ThreadSynchronization()
        {

            //LOCK 
            //for (int i = 0; i < 5; i++)
            //{
            //    new Thread(Doworkwithlock).Start(); 
            //}

            //MONITOR
            //for (int i = 0; i < 5; i++)
            //{
            //    new Thread(DoworkwithMonitor).Start();
            //}

            //Manual Reset
            //new Thread(Writer).Start();
            //for(int i=0;i<5;i++)
            //{
            //    new Thread(Reader).Start();
            //}

            //Automatic Reset
            //for (int i = 0; i < 5; i++)
            //{
            //    new Thread(Method1).Start();
            //}


        }
    }
}
//NOTE - The difference between AutomaticReset and ManualReset is 
//    In AutomaticReset we would need for each WaitOne() one Set() so 1:1 mapping so Single WaitOne() can be released using a single Set()
//    But in ManualReset a Single Set() can release all the WaitOne() so 1:N mapping