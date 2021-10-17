using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTAuthentication.JWTHelpers
{
    public interface IJWTAuthenticationManager
    {
        string Authenticate(string userId, string pass);
    }
}
