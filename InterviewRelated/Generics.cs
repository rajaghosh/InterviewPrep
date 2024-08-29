using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewRelated
{
    class Generics
    {
        //Generic Method
        public static bool ValueEqual<T> (T a, T b)
        {
            return a.Equals(b);
        }
    
        //This does the same. But here there is no type checking
        //public static bool ValueEqual(object a,object b)
        //{
        //    return a.Equals(b);
        //}
    }

    //Generic Class
    class Calculator<T>
    {
        public static bool ValueEqual(T a,T b)
        {
            return a.Equals(b);
        }
    }

}
