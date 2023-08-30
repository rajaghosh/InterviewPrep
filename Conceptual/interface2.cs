using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    interface ISender
    {
        int Id { get; set; }
        void Send();

         //virtual void Method();
    }

    interface ITest
    {
        void Send();
    }

    class EmailSender : ISender,ITest
    {
        private int _name;
        int ISender.Id
        {
            get =>_name;
            set=>_name = value;
        }

        void ISender.Send()
        {
            Console.WriteLine("Of ISender.Send()");
        }

        void ITest.Send()
        {
            Console.WriteLine("Of ITest.Send()");
        }

    }

    class GreetingSender
    {
        ISender _sender;
        public GreetingSender(ISender sender)
        {
            _sender = sender;
        }
        void SendGreetings()
        {
            _sender.Send();
        }
    }

}
