using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conceptual2
{
    class TaskAndThread
    { 
        public TaskAndThread()
        {
            Console.WriteLine("Started.......");

            //Call using normal task - task using normal time slicing the main thread.
            Function1();    //Here portion of time slicling will be further partioned by 2 as there are 2 tasks
            Function2();    
            Function3();

            //Call using task parallel library (TPL) - Here each task will run on parallel thread of different cores of the CPU
            Task.Factory.StartNew(Function4);
            Task.Factory.StartNew(Function5);
        }


        //This method will run 2 tasks from a single method
        public void Function1()
        {
            Task.Delay(5000).Wait();
            Console.WriteLine("Task 1 Completed");
            Task.Delay(5000).Wait();
            Console.WriteLine("Task 2 Completed");

        }

        //Between Function2 and Function3 to increase efficiency context switching will happen - time slicing
        //That is the functions will run concurrently on same thread.
        public async void Function2()
        {
            await Task.Delay(5000);
            Console.WriteLine("Function 2 Completed");
        }

        public async void Function3()
        {
            await Task.Delay(5000);
            Console.WriteLine("Function 3 Completed");
        }

        //We would be running Function4 and Function5 as threads using TPL - Task Parallel Library
        //That is each task on different thread
        public void Function4()
        {
            Task.Delay(5000);
            Console.WriteLine("Function 4 Completed");
        }

        public void Function5()
        {
            Task.Delay(5000);
            Console.WriteLine("Function 5 Completed");
        }



    }
}
