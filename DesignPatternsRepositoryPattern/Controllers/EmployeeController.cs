using DesignPatternsRepositoryPattern.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DesignPatternsRepositoryPattern.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository _emp;

        public EmployeeController(IEmployeeRepository emp)
        {
            this._emp = emp;
        }

        // GET: api/<EmployeeController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<EmployeeController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<EmployeeController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<EmployeeController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<EmployeeController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}

        [HttpGet("GetAllEmployee")]
        public IEnumerable<Employee> GetAllEmployee()
        {
            return _emp.GetAllEmployee();
        }

        [HttpGet("GetEmployeeById")]
        public Employee GetEmployee(int Id)
        {
            return _emp.GetEmployee(Id);
        }

        [HttpPut("UpdateEmployee")]
        public Employee Update(Employee employeeChanges)
        {
            return _emp.Update(employeeChanges);
        }

        [HttpPost("AddEmployee")]
        public Employee Add(Employee employee)
        {
            return _emp.Add(employee);
        }

        [HttpDelete("DeleteEmployee")]
        public Employee Delete(int id)
        {
            return _emp.Delete(id);
        }

    }
}
