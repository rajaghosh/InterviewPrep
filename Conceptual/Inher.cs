using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    class A
    {
        int a;
        public A()
        {
            Console.WriteLine("In the base class A constructor");
        }

        static A()
        {
            Console.WriteLine("In the base class A static constructor");
        }

        public A(string a)
        {
            Console.WriteLine("Inside base class A parameterised constructor with value {0}", a);
        }
        public virtual void Method1()
        {
            Console.WriteLine("Inside Method 1 of base class A");
        }
    }

    class B : A
    {
        public B()
        {
            Console.WriteLine("In the derived class B constructor");
        }

        static B()
        {
            Console.WriteLine("In the derived class B static constructor");
        }

        public B(int b) : base("A")
        {
            Console.WriteLine("Inside base class B parameterised constructor with value {0}", b);
        }

        public override void Method1()
        {
            Console.WriteLine("Inside Method 1 of derived class B");

        }

        public void Method2()
        {
            Console.WriteLine("Inside Method 2 of derived class B");
        }
    }

}
