using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewRelated.Service
{
    public interface IiocService
    {
        void Function1();
    }
    class IOCService:IiocService
    {
        public void Function1()
        {
            Console.WriteLine("Inside IOCService");
        }
    }
}
