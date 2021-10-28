using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Conceptual1
{
    class Reflection
    {
        public Reflection()
        {
            int i = 42;
            Type type = i.GetType();
            Console.WriteLine(type);

            Assembly info = typeof(int).Assembly;
            Console.WriteLine(info);
        }
    }
}
/*
 * NOTE - 
 * Reflection provides objects (of type Type) that describe assemblies, modules, and types.
 * You can use reflection to dynamically create an instance of a type, bind the type to an 
 * existing object, or get the type from an existing object and invoke its methods or access 
 * its fields and properties. If you are using attributes in your code, reflection enables 
 * you to access them. 
 * 
 * 
 * 
 * 
 */
