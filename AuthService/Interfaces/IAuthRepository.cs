using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;


namespace AuthService.Interfaces
{
    public interface IAuthRepository
    {
        string CreateToken(Models.User user);
    }
}