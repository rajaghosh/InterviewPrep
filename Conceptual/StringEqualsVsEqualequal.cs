using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class StringEqualsVsEqualequal
    {
        public StringEqualsVsEqualequal()
        {
            object s1 = "Hello World";
            object s2 = s1;
            object s3 = new string("Hello World");

            Console.WriteLine(s1 == s2); //Both references same. So true
            Console.WriteLine(s1 == s3); //As both references are different so this will return false
            Console.WriteLine(s1.Equals(s3)); //Both data same. So true
        }
    }
}
