using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    class Child
    {
        public void Method1()
        {
            Console.WriteLine("In the child Method 1");
        }

        public virtual void Method2()
        {
            Console.WriteLine("In the child Method 2");
        }

        public void Method3()
        {
            Console.WriteLine("In the child Method 3");
        }
    }

    class Student : Child
    {
        public new void Method1()
        {
            Console.WriteLine("In the Student Method 1");
        }

        public override void Method2()
        {
            Console.WriteLine("In the Student Method 2");
        }
        
    }

    class Employed : Child
    {
        public new void Method1()
        {
            Console.WriteLine("In the Employed Method 1");
        }

        public sealed override void Method2()
        {
            Console.WriteLine("In the Employed Method 2");
        }

        public new void Method3()
        {
            Console.WriteLine("In the Employed Method 3");
        }
    }

    class Check : Employed
    {
        public new void Method2()
        {
            Console.WriteLine("In the Check Method 2");
        }

        public new void Method3()
        {
            base.Method3();
        }
    }
}
