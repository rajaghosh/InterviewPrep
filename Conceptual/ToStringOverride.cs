using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class ToStringOverride
    {
        public string Name { get; set; }
        public int Ward { get; set; }

        public override string ToString()
        {
            return "Person: " + Name + " " + Ward;
        }

        public ToStringOverride()
        {
            this.Name = "Raja";
            this.Ward = 26;

            Console.WriteLine(ToString());
        }
    }
}
