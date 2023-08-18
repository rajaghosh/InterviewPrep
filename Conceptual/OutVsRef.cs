using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Conceptual1
{
    class OutVsRef
    {
        public OutVsRef()
        {
            int a = 100;
            Funct1(ref a);

            Console.WriteLine("Inside : " + a);

            Funct2(out a);
            Console.WriteLine("Inside : " + a);
            
        }

        public void Funct1(ref int abc)
        {
            abc = abc + 10;
            Console.WriteLine("Outside Ref : " + abc);
        }

        public void Funct2(out int abc)
        {
            abc = 0;
            abc = abc + 10;
            Console.WriteLine("Outside Out : " + abc);
        }
    }
        
}

/*
    Out and Ref - They help to pass data by reference.
    Ref - Original data is shared between caller and called function.
    Out - Data initialised from the called function and value is returned.

 
 */