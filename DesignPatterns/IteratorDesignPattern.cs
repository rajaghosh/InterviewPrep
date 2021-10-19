using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignPatterns
{
    class IteratorDesignPattern
    {
        public class Customer
        {
            private List<Address> _Addresses { get; set; }

            public void Addr(Address adr)
            {
                _Addresses = new List<Address>();
                var data = _Addresses.Any();
                if (_Addresses != null && _Addresses.Any())
                {
                    foreach (Address a in _Addresses)
                    {
                        if (a.Type == adr.Type)
                        {
                            throw new Exception("Not Allowed");
                        }
                    }
                }
                _Addresses.Add(adr);
            }

            public List<Address> GetAddresses()
            {
                return _Addresses;
            }

            public IEnumerable<Address> GetAddressesModified()
            {
                IEnumerable<Address> _AddressesModified = _Addresses;
                return _AddressesModified;
            }
        }

        public class Address
        {
            public string Type { get; set; }
        }

        public IteratorDesignPattern()
        {
            Customer _cust = new Customer();
            _cust.Addr(new Address() { Type = "o1" });

            //EXCEPTION "new code"
            //As we are exposing the list (Private) still we are able to add data as this will create a link to do so
            _cust.GetAddresses().Add(new Address() { Type = "o2" }); 

            foreach (var a in _cust.GetAddresses())
            {
                Console.WriteLine(a.Type);
            }

            //As we have Interface so we cant edit
            _cust.GetAddressesModified();

            foreach (var a in _cust.GetAddresses())
            {
                Console.WriteLine(a.Type);
            }
        }
    }
}

/*
 * Iterator is a behavioral design pattern 
 * that lets you traverse elements of a collection without exposing its underlying representation (list, stack, tree, etc.).
 * 
 * 
 * 
 */