using System;

namespace Conceptual1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World");
            //JsonListCreation j1 = new JsonListCreation();

            //DelegateTesting dt1 = new DelegateTesting();
            //DelegateTesting2 dt2 = new DelegateTesting2();
            //DelegateTesting3 dt3 = new DelegateTesting3();

            //SerializeVsDeserialize svd1 = new SerializeVsDeserialize(); 

            LazyLoading ll1 = new LazyLoading();


            Console.ReadLine();

            //*********Calling Non Static Method inside Static
            //Program P = new Program();
            //P.Test();
        }

        public void Test()
        {
        }
    }
}
