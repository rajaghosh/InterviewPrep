using DesignPatternsRepositoryPattern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DesignPatternsRepositoryPattern.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmployeeRepository _emp;

        public HomeController(ILogger<HomeController> logger, IEmployeeRepository emp)
        {
            _logger = logger;
            this._emp = emp;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IEnumerable<Employee> GetAllEmployee()
        {
            return _emp.GetAllEmployee();
        }

        public Employee GetEmployee(int Id)
        {
            return _emp.GetEmployee(Id);
        }

        public Employee Update(Employee employeeChanges)
        {
            return _emp.Update(employeeChanges);
        }

        public Employee Add(Employee employee)
        {
            return Add(employee);
        }

        public Employee Delete(int id)
        {
            return Delete(id);
        }
    }
}
