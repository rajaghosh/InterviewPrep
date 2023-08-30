using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Basic
{
    class Linq
    {
        List<Employee2> empList2;
        public class Employee2
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Salary { get; set; }
            public int DeptId { get; set; }
        }
        public List<Employee2> ListEmployee()
        {
            empList2 = new List<Employee2>()
            {
                new Employee2 { Id = 1 , Name ="Modi" , Salary = 10245, DeptId=1 },
                new Employee2 { Id = 2 , Name ="Amit" , Salary = 9245, DeptId=3 },
                new Employee2 { Id = 3 , Name ="Rahul" , Salary = 15245, DeptId=1 },
                new Employee2 { Id = 4 , Name ="Sonia" , Salary = 1245, DeptId=4 },
                new Employee2 { Id = 5 , Name ="Mamta" , Salary = 1545, DeptId=4 },
                new Employee2 { Id = 6 , Name ="Lalu" , Salary = 20000, DeptId=1 },
                new Employee2 { Id = 7 , Name ="Udvav" , Salary = 15789.50, DeptId=1 },
                new Employee2 { Id = 8 , Name ="KCR" , Salary = 10500, DeptId=3 },
                new Employee2 { Id = 9 , Name ="Nitish" , Salary = 25000, DeptId=2 },
                new Employee2 { Id = 10 , Name ="Mukul" , Salary = 10458, DeptId=1 }
            };

            return empList2;
        }

        public void Method1()
        {
            List<Employee2> l1 = ListEmployee();

            var test = l1.GroupBy(a => a.Name[0]);

            foreach (var t in test)
            {
                Console.WriteLine(t.Key);
                foreach (var items in t) 
                {
                    Console.WriteLine("\t{0}", items.Name);
                }
            }

        }

        public void Method2()
        {
            List<Employee2> l1 = ListEmployee();

            var test = l1.ToLookup(b => b.Name[0]);

            foreach(var a1 in test)
            {
                Console.WriteLine(a1.Key);
                foreach (var b1 in a1)
                {
                    Console.WriteLine(b1.Name + ":" + b1.Salary);
                }
            }

        }

    }
}
