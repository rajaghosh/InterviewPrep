using JWTAuthentication.JWTHelpers;
using JWTAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JWTAuthentication.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NameController : ControllerBase
    {
        private readonly IJWTAuthenticationManager _jwtAuth;
        public NameController(IJWTAuthenticationManager jwtAuth)
        {
            this._jwtAuth = jwtAuth;
        }

        [AllowAnonymous]
        [HttpPost("userauthenticate")]
        public IActionResult Authenticate([FromBody] UserCred _userCred)
        {
            var token = _jwtAuth.Authenticate(_userCred.Id, _userCred.Pass);
            if (token == null)
                return Unauthorized();
            return Ok(token);
        }

        // GET: api/<NameController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Kolkata", "Mumbai" };
        }

        // GET api/<NameController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<NameController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<NameController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<NameController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
