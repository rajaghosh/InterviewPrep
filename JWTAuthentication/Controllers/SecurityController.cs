using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JWTAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {

        //This will be called once User checked-in
        public string GenerateJsonWebToken(string username)
        {
            string _saltKey = "SALT_1234567889";



            var _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_saltKey));
            var _credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256); //Security Key + Algorithm

            //This is role
            var _claims = new[] {
                new Claim("Issuer", "Raja"),
                new Claim("Admin","true"),
                new Claim(JwtRegisteredClaimNames.UniqueName, username)
            };

            //This is token - Total Security Token
            var _token = new JwtSecurityToken(
                                        "Raja", //Created by
                                        "Raja123", //Created for
                                        _claims,
                                        expires: DateTime.Now.AddMinutes(120),
                                        signingCredentials: _credentials
                                        );

            return new JwtSecurityTokenHandler().WriteToken(_token);
        }

        [HttpGet]
        public string Get()
        {
            return GenerateJsonWebToken("Raja123");
        }

        // GET: api/<SecurityController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<SecurityController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<SecurityController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<SecurityController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SecurityController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
