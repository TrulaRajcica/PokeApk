using System;
using System.Collections.Generic;
using System.Linq;
using AuthService.Models;
using System.Threading.Tasks;

namespace AuthService.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task<bool> SaveChangesAsync();
    }
}