using System;
using System.Threading;

namespace Conceptual2
{
    class Maths
    {
        public int Num1;
        public int Num2;

        Random rand = new Random();
        public void Divide()
        {
            for (long i=0; i < 10000; i++)
            {
                //Here we have a critical section that is sa sharable variable is present
                //So for multithreading environment we can have trouble 
                //So we can use lock() - Only a single thread can work in the critical section. We also have Monitor,Mutex and Semaphore
                lock (this)
                {
                    Num1 = rand.Next(1, 2);
                    Num2 = rand.Next(1, 2);
                    Console.WriteLine("Num2 = {0}, Num3 = {1}", Num1, Num2);
                    int result = Num1 / Num2;
                    Num1 = 0;
                    Num2 = 0;
                }
            }
        }

    }

    class MultiThreadingThreadSafe
    {
        static Maths objMaths = new Maths();
        public MultiThreadingThreadSafe(){
            
            //This arrangement will cause trouble as there could be a set of Num1/Num2 = 0 from either of the threads
            
            Thread t1 = new Thread(objMaths.Divide);
            t1.Start(); // Child Thread
            objMaths.Divide(); // Main Thread



        }
    }
}
