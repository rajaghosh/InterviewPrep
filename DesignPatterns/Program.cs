using System;

namespace DesignPatterns
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //***********SingletonPattern Implementation
            SingletonImplementation1 si1 = new SingletonImplementation1();
            SingletonImplementation2 si2 = new SingletonImplementation2();
        }
    }
}
