using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewRelated
{
    //Virtual Method can be used in both Abstract and Non-Abstract class
    //Use - Have different definitions in Base and Derived Class.
    abstract class AbstractBase
    {
        public abstract void Method1();
        public virtual void VirtualMethod()
        {
            Console.WriteLine("This is virtual method in abstract class");
        }
    }

    class NonAbstractBase
    {
        public virtual void NonAbstractVirtualMethod()
        {
            Console.WriteLine("This is virtual method in non-abstract class");
        }
    }

    class DerivedClass1 : AbstractBase
    {
        public override void Method1()
        {
            Console.WriteLine("Abstract base class Method Overridden!!");
            //throw new NotImplementedException();
        }
        public override void VirtualMethod()
        {
            base.VirtualMethod();
            Console.WriteLine("Overriden the DerivedClass1 Virtual Method");
        }
    }

    class DerivedClass2 : NonAbstractBase
    {
        public override void NonAbstractVirtualMethod()
        {
            base.NonAbstractVirtualMethod();
            Console.WriteLine("Overriden the DerivedClass2 Virtual Method");
        }
    }

    class Virtual
    {
        public void TestVirtualMethod() 
        { 
            //Use of Abstract Class
            AbstractBase ab;
            ab = new DerivedClass1();

            ab.Method1();
            ab.VirtualMethod();

            DerivedClass2 dcObj = new DerivedClass2();
            dcObj.NonAbstractVirtualMethod();
        } 
    }
}
