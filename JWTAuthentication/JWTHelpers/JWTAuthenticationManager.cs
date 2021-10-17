using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTAuthentication.JWTHelpers
{
    public class JWTAuthenticationManager : IJWTAuthenticationManager
    {
        //Temporary Sample User List
        private readonly IDictionary<string, string> _users = new Dictionary<string, string>() {
                                                                                                    {"test1","password1" },
                                                                                                    {"test2","password2" }
                                                                                                };
        private readonly string _key;

        public JWTAuthenticationManager(string key)
        {
            this._key = key;
        }

        public string Authenticate(string userId, string pass)
        {
            try
            {
                //If user not matched
                if (!_users.Any(u => u.Key == userId && u.Value == pass))
                {
                    return null;
                }

                var _claim = new Claim[]{
                new Claim(ClaimTypes.UserData, userId)
                };

                var _tokenKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this._key));

                var _tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(_claim),
                    Expires = DateTime.Now.AddHours(1),
                    SigningCredentials = new SigningCredentials(_tokenKey, SecurityAlgorithms.HmacSha256)

                };

                var _tokenHandler = new JwtSecurityTokenHandler();
                var _tokenCreated = _tokenHandler.CreateToken(_tokenDescriptor);

                return _tokenHandler.WriteToken(_tokenCreated);
            }
            catch(Exception ex)
            {

            }
            return null;
        }


    }
}
