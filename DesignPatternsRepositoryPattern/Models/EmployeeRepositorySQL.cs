using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesignPatternsRepositoryPattern.Models
{
    public class EmployeeRepositorySQL : IEmployeeRepository
    {
        private readonly AppDBContext context;

        public EmployeeRepositorySQL(AppDBContext context)
        {
            this.context = context;
        }

        public Employee Add(Employee employee)
        {
            context.EmployeeRepo1.Add(employee);
            context.SaveChanges();
            return employee;
        }

        public Employee Delete(int id)
        {
            //throw new NotImplementedException();
            Employee employee = context.EmployeeRepo1.Find(id);
            if(employee != null)
            {
                context.EmployeeRepo1.Remove(employee);
                context.SaveChanges();
            }
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployee()
        {
            //throw new NotImplementedException();
            return context.EmployeeRepo1;
        }

        public Employee GetEmployee(int Id)
        {
            //throw new NotImplementedException();
            return context.EmployeeRepo1.Find(Id);
        }

        public Employee Update(Employee employeeChanges)
        {
            //throw new NotImplementedException();
            var employee = context.EmployeeRepo1.Attach(employeeChanges);
            employee.State = EntityState.Modified;
            context.SaveChanges();
            return employeeChanges;
        }
    }
}
