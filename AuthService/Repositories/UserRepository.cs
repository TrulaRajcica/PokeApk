using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.Interfaces;
using AuthService.Models;
using System.Threading.Tasks;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthContext _context;
        public UserRepository( AuthContext context) => _context = context;

        public async Task<User?> GetByEmailAsync(string email) => 
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task AddAsync(User user) => await _context.Users.AddAsync(user);

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}