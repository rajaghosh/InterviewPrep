using System;
using System.Collections.Generic;
using System.Text;

namespace DesignPatterns
{
    class BridgeDesignPattern2
    {
        public BridgeDesignPattern2()
        {
            Role _role = new Associate();
            _role.roleSalary = new SalaryLevel1();
            _role.ShowRoleSalary();

            _role.roleSalary = new SalaryLevel2();
            _role.ShowRoleSalary();

            _role.roleSalary = new SalaryLevel3();
            _role.ShowRoleSalary();

            _role = new Manager();
            _role.roleSalary = new SalaryLevel3();
            _role.ShowRoleSalary();

        }
    }

    abstract class Role
    {
        public IRoleSalary roleSalary;
        public abstract void ShowRoleSalary();
    }

    //Note - The child class cannot have more access than parent class. That is if parent class access (not member) is private
    //then child cannot be protected or public. 
    //One more thing withing a namespace directly the class can be declared public other access specifier cannot be declared.
    class Associate : Role
    {
        public override void ShowRoleSalary()
        {
            roleSalary.SalaryLevel("Associate");
        }
    }

    class Manager : Role
    {
        public override void ShowRoleSalary()
        {
            roleSalary.SalaryLevel("Manager");
        }
    }

    interface IRoleSalary
    {
        public void SalaryLevel(string roleName);
    }

    public class SalaryLevel1 : IRoleSalary
    {
        public void SalaryLevel(string roleName)
        {
            Console.WriteLine("We have salary of Rs 10,000 for " + roleName);
        }
    }

    public class SalaryLevel2 : IRoleSalary
    {
        public void SalaryLevel(string roleName)
        {
            Console.WriteLine("We have salary of Rs 50,000 for " + roleName);
        }
    }

    public class SalaryLevel3 : IRoleSalary
    {
        public void SalaryLevel(string roleName)
        {
            Console.WriteLine("We have salary of Rs 1,00,000 for " + roleName);
        }
    }



}
