using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class LazyLoading
    {
        public LazyLoading()
        {
            Customer1 c1 = new Customer1();
            Console.WriteLine(c1.CustomerName);

            foreach(Order1 o1 in c1.Orders)
            {
                Console.WriteLine(o1.OrderName);
            }
        }
    }

    public class Customer1
    {
        //Here we are going to lazy load the Order1 List 
        private Lazy<List<Order1>> _orders = null;
        public List<Order1> Orders
        {
            get 
            { 
                return _orders.Value; //Lazy
            }
        }

        private string _customerName;
        public string CustomerName 
        {
            get { return _customerName; } 
            set { _customerName = value; } 
        }

        public Customer1()
        {
            CustomerName = "Raja";
            _orders = new Lazy<List<Order1>>(()=> LoadOrders()); //Lazy Loading - Even if the Customer1() is called other members will be loaded immediately but the lazy members will be initiated only when they are actually utilized
        }

        private List<Order1> LoadOrders()
        {
            List<Order1> temp = new List<Order1>();
            Order1 o1 = new Order1();
            o1.OrderId = 1;
            o1.OrderName = "Order1";
            temp.Add(o1);

            Order1 o2 = new Order1();
            o2.OrderId = 2;
            o2.OrderName = "Order2";
            temp.Add(o2);

            return temp;
        }
    }

    public class Order1
    {
        public int OrderId { get; set; }
        public string OrderName { get; set; }
    }
}
/*
 * Advantages of Lazy Loading -
 * 1. Minimizes start up time of the application.
 * 2. Application consumes less memory. (on-demand loading).
 * 3. Unnecessary database SQL execution is avoided.
 * 
 * Disadvantage -
 * 1. Code gets complicated.
 * 2. Decrease in performance.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */