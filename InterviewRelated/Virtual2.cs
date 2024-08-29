using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewRelated
{
    //All Examples

    //The abstract keyword is used for classes and methods:
    //    Abstract class: is a restricted class that cannot be used to create objects(to access it, it must be inherited from another class).
    //    Abstract method: can only be used in an abstract class, and it does not have a body.The body is provided by the derived class (inherited from).

    class Virtual2
    {
        public void TestVirtualMethod()
        {
            //A objA = new A(); //Not Allowed
            B ObjB = new B();
            ObjB.Method_1();
            ObjB.Method_2();

            Console.WriteLine("-----------------------------------");

            A2 objA2 = new A2();
            B2 objB2 = new B2();
            A2 objA2B2 = new B2();

            C2 objC2 = new C2();
            A2 objA2C2 = new C2();

            D2 objD2 = new D2();
            A2 objA2D2 = new D2();

            objA2.Method_3();
            objB2.Method_3();
            objA2B2.Method_3();
            Console.WriteLine("--------------||||------------------");
            objC2.Method_3();
            objA2C2.Method_3();
            Console.WriteLine("--------------||||------------------");
            objD2.Method_3();
            objA2D2.Method_3();
            Console.WriteLine("--------------||||------------------");

            E2 objE2 = new E2();
            C2 objC2E2 = new E2();

            objE2.Method_3();
            objC2E2.Method_3();


            Console.WriteLine("-----------------------------------");

            A3 objA3 = new A3();
            B3 objB3 = new B3();
            A3 objA3B3 = new B3();

            objA3.Method_4();
            objB3.Method_4();
            objA3B3.Method_4();

        }
    }


    //Virtual Method can be used in both Abstract and Non-Abstract class
    //Use - Have different definitions in Base and Derived Class.
    public abstract class A
    {
        //Abstract
        public abstract void Method_1();

        //Virtual
        public virtual void Method_2()
        {
            Console.WriteLine("From Abstract Parent Class A -> Method_2 !!!");
        }
    }

    public class B : A
    {
        public override void Method_1()
        {
            //base.Method_1(); Not allowed
            Console.WriteLine("From Class B Method_1");
        }

        public override void Method_2()
        {
            base.Method_2();
            Console.WriteLine("From Class B Method_2");
        }
    }

    public class A2
    {
        public virtual void Method_3()
        {
            Console.WriteLine("From Non-Abstract Parent Class A2 -> Method_3 !!!");
        }
    }

    class B2 : A2
    {
        public override void Method_3()
        {
            base.Method_3();
            Console.WriteLine("Class B2 -> Method_#");
        }
    }

    class C2 : A2
    {
        //NOTE - OVERRIDE CANT BE USED WITH NEW OR VIRTUAL
        //If its not override then ClassA = objC it will call ClassA
        public new virtual void Method_3()
        {
            base.Method_3();
            Console.WriteLine("Class C2 -> Method_3");
        }
    }

    class D2 : C2
    {
        public override void Method_3()
        {
            base.Method_3();
            Console.WriteLine("Class D2 -> Method_3");
        }
    }

    class E2 : D2
    {
        public override void Method_3()
        {
            base.Method_3();
            Console.WriteLine("Class E2 -> Method_3");
        }
    }



    public class A3
    {
        public void Method_4()
        {
            Console.WriteLine("Class A3 -> Method_4");
        }
    }

    public class B3 : A3
    {
        public new void Method_4()
        {
            Console.WriteLine("Class B3 -> Method_4");
        }
    }



}
