using System;

namespace Conceptual1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World");
            //JsonListCreation j1 = new JsonListCreation();

            //DELEGATE CALL
            //DelegateTesting1 dt1 = new DelegateTesting1();
            //DelegateTesting2 dt2 = new DelegateTesting2();
            //DelegateTesting3 dt3 = new DelegateTesting3();

            OutVsRef outVsRef = new OutVsRef();

            //SerializeVsDeserialize svd1 = new SerializeVsDeserialize(); 

            //LazyLoading ll1 = new LazyLoading();

            //Reflection r1 = new Reflection();

            //ToStringOverride tso1 = new ToStringOverride();

            //StringEqualsVsEqualequal sevee1 = new StringEqualsVsEqualequal();

            //ArrayOfJson aj1 = new ArrayOfJson();

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
