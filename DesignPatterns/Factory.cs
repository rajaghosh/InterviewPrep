using System;
using System.Collections.Generic;
using System.Text;

namespace DesignPatterns
{
    interface IFactory
    {
        public void GetName(string str);
    }

    class Factory
    {
        public Factory()
        {
            IFactory _if;

            _if = new Student();
            _if.GetName("Student 1");

            _if = new Teacher();
            _if.GetName("Teacher 1");

        }
    }

    class Student : IFactory
    {
        public void GetName(string str)
        {
            Console.WriteLine("This is a Student : " + str);
        }
    }

    class Teacher : IFactory
    {
        public void GetName(string str)
        {
            Console.WriteLine("This is a Teacher : " + str);
        }
    }

}
/*
 * Factory Pattern Implementation - Here we have a factory interface IFactory. IFactory is implemented by Student and Teacher classes. 
 * During run we will use IFactory instance but will instanciate IFactory with the needed class as required.
 * 
 * 
 */