using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesignPatternsRepositoryPattern.Models
{
    public class EmployeeRepositoryLocal : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public EmployeeRepositoryLocal()
        {
            _employeeList = new List<Employee>()
            {
                new Employee() {Id = 1, Name ="Raja", DeptId = 1, Email = "abc@abc.com"},
                new Employee() {Id = 2, Name ="Krishna", DeptId = 2, Email = "a1@abc.com" }
            };
        }

        public Employee Add(Employee employee)
        {
            //throw new NotImplementedException();
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee Delete(int id)
        {
            //throw new NotImplementedException();
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == id);
            if (employee != null)
            {
                _employeeList.Remove(employee);
            }
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployee()
        {
            return _employeeList;
        }

        public Employee GetEmployee(int Id)
        {
            //throw new NotImplementedException();
            return _employeeList.FirstOrDefault(e => e.Id == Id);
        }

        public Employee Update(Employee employeeChanges)
        {
            //throw new NotImplementedException();
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == employeeChanges.Id);
            if(employee != null)
            {
                //_employeeList.Remove(employee);
                employee.Name = employeeChanges.Name;
                employee.Email = employeeChanges.Email;
                employee.DeptId = employeeChanges.DeptId;
            }
            return employee;
        }
    }
}
