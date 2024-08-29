using System;

namespace InterviewRelated
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            #region ABSTRACT CLASS VS INTERFACE
            //AbstractVSInterface a = new AbstractVSInterface();
            //a.AbstactClass();
            //a.Interface();
            #endregion

            #region VIRTUAL METHOD
            //Virtual vObj = new Virtual();
            //vObj.TestVirtualMethod();

            Virtual2 vObj2 = new Virtual2();
            vObj2.TestVirtualMethod();
            #endregion

            #region DEPENDENCY INJECTION
            //DependencyInjectionDemo di = new DependencyInjectionDemo();
            //di.DependencyInjectionTest();
            #endregion

            #region GENERICS
            Console.WriteLine(Generics.ValueEqual<string>("abc", "abc"));

            Console.WriteLine(Calculator<string>.ValueEqual("abc", "abc"));

            //DependencyInjectionDemo di1 = new DependencyInjectionDemo();
            //DependencyInjectionDemo di2 = new DependencyInjectionDemo();
            ////We would pass class type
            //Console.WriteLine(Calculator<DependencyInjectionDemo>.ValueEqual(di1, di2));
            #endregion

            #region EXTENSION METHOD
            string test1 = "Raja Ghosh";
            string result = test1.ChangeCaseFirstCharacter();
            #endregion
        }
    }
}
