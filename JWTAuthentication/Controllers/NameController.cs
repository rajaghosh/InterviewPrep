using JWTAuthentication.JWTHelpers;
using JWTAuthentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JWTAuthentication.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //This is needed to test the bearer token based authentication
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

        [HttpGet("GetData")]
        public IEnumerable<string> Get()
        {
            return new string[] { "Kolkata", "Mumbai" };
        }
    }
}
