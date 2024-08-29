using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewRelated
{

    #region ABSTRACT CLASS
    //For interface we need to implement all the members but for abstract class it just what we need
    public abstract class AbstractBaseClass
    {
        //public abstract int tempVal; - Abstract class can't have abstract variable
        
        //Abstract Methods are only applicable for Abstract Class
        public abstract int Var1 { get; set; }  //Abstract Property
        public abstract int AnAbstractMethod(); //As we use abstract keyword so we need to implement it - if need (no strict checking)
    
        public int VarA { get; set; }
        public int GetVarA(int a= 1) { return a; }
    }

    public class AbstractDerivedClassA : AbstractBaseClass
    {
        //This is a must as in the base class it is "abstract" - Implementation 1

        //"override" keyword is used to consume "abstract" - Method
        //This is the standard implementation code
        //public override int Var1 
        //{
        //    get => throw new NotImplementedException(); 
        //    set => throw new NotImplementedException();
        //}

        public override int Var1 { get ; set; }

        //"override" keyword is used to consume "abstract" - Method
        public override int AnAbstractMethod()  
        {
            return Var1 * 10;
        }
    }

    public class AbstractDerivedClassB : AbstractBaseClass
    {
        //"override" keyword is used to consume "abstract" - Method
        //This is the standard implementation code
        //public override int Var1
        //{
        //    get => throw new NotImplementedException();
        //    set => throw new NotImplementedException();
        //}
        public override int Var1 { get; set; }

        //"override" keyword is used to consume "abstract" - Method
        public override int AnAbstractMethod()
        {
            return Var1 * 100;
        }
    }

    public class TestAbstractClass
    {
        public void CallAbstractClass()
        {
            //Derived class call
            AbstractDerivedClassA objA = new AbstractDerivedClassA();
            AbstractDerivedClassB objB = new AbstractDerivedClassB();

            objA.Var1 = 10;
            Console.WriteLine(objA.AnAbstractMethod());

            objB.Var1 = 20;
            Console.WriteLine(objB.AnAbstractMethod());

            //Base class call
            //We would not use the new Keyword here
            AbstractBaseClass objAbstract1;   
            objAbstract1 = new AbstractDerivedClassA();

            objAbstract1.Var1 = 10;
            Console.WriteLine(objA.AnAbstractMethod());


            AbstractBaseClass objAbstract2;
            objAbstract2 = new AbstractDerivedClassB();

            objAbstract2.Var1 = 20;
            Console.WriteLine(objB.AnAbstractMethod());


        }
    }

    #endregion

    #region INTERFACE

    //By default public 
    interface ITestInteraface1
    {
        //Static members allowed in C# from C# 8
        //public static int var1; 
        public int TestFunc();
        public int Var { get; set; }
    }

    interface ITestInteraface2
    {
        //Static members allowed in C# from C# 8
        //public static int var1; 
        public int TestFunc();
        public int TestFunc2(int a);
    }

    public class CheckInterface: ITestInteraface1, ITestInteraface2
    {
        public int TestFunc()
        {
            return 1;
        }
        public int TestFunc2(int a)
        {
            return 2;
        }

        public int Var { get; set; }
    }

    public class CheckInterface2 : ITestInteraface1
    { 
        public int TestFunc()
        {
            return 1;
        }

        public int Var { get; set; }
    }


    public class TestInterface
    {
        public void GetInterfaceVal() 
        {
            CheckInterface objI = new CheckInterface();
            Console.WriteLine(objI.TestFunc());
            objI.Var = 2;
            Console.WriteLine(objI.TestFunc2(objI.Var));

            //ITestInteraface2 iInstance;// = null;
            //iInstance = new CheckInterface2();

        } 
    }
    #endregion

    class AbstractVSInterface
    {
        public void AbstactClass() 
        {
            TestAbstractClass obj1 = new TestAbstractClass();
            obj1.CallAbstractClass();
        }

        public void Interface()
        {
            TestInterface obj2 = new TestInterface();
            obj2.GetInterfaceVal();
        }
    }
}
