using System;
using System.Collections.Generic;
using System.Text;

namespace DesignPatterns
{
    class AggregateRootDesignPattern
    {
        //Here all the logic is there only in Customer class
        public class Customer
        {
            private List<Address> _Addresses { get; set; }

            public void Addr(Address adr)
            {
                foreach(Address a in _Addresses)
                {
                    if(a.Type == adr.Type)
                    {
                        throw new Exception("Not Allowed");
                    }
                }
                _Addresses.Add(adr);
            }
        }

        public class Address
        {
            public string Type { get; set; }
        }

        public AggregateRootDesignPattern()
        {
            Customer _cust = new Customer();
            _cust.Addr(new Address() { Type = "o" });
            _cust.Addr(new Address() { Type = "o" });
        }
    }
}

/*
 * Here in Aggregate Root, the single parent class or single root class through which all modifications are happening.
 * Here only a Single main class is responsible for implementing all the logic.
 * 
 * 
 * 
 */