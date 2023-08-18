using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Conceptual1
{
    class DelegateTesting3
    {
        //DelegateTesting3-DelegateEvent
        public DelegateTesting3()
        {
            SomeClass x = new SomeClass();
            //Multicasting here we can make the delgate null - So we can think it as broadcast / also can be called as naked delegate
            x.senderObj += Receiver;
            x.senderObj += Receiver2;
            //x.senderObj = null; //Will give error as this makes the delegate null

            //Event - We cant make it null (Works same as multicast but we cant make it null)
            x.senderObj2 += Receiver;
            x.senderObj2 += Receiver2;


            //Another thread start
            Thread t1 = new Thread(new ThreadStart(x.HugeProcess));
            t1.Start();

            Console.WriteLine("DelegateTesting3 Completed");

        }

        public static void Receiver(int i)
        {
            Console.WriteLine(i);
        }

        public static void Receiver2(int i)
        {
            Console.WriteLine(i);
        }
    }

    class SomeClass
    {
        
        public delegate void Sender(int i);
        public Sender senderObj = null;

        public delegate void Sender2(int i);
        public event Sender2 senderObj2 = null;

        public void HugeProcess()
        {
            for(int i=0; i < 10; i++)
            {
                Thread.Sleep(500);
                senderObj(i); //Anonymous method - For Multicast delegate - Broadcast - Here consumer can make delegate null
                senderObj2(i); //Anonymous method - For Event - Producer Consumer Model - Here consumer has no control
            }
        }
    }

        
}

/*
    Event - It encapsulates delegates. That is it works on it. 
    It modifies the multicast model of delegates to Producer - Subscriber model.

 
 */