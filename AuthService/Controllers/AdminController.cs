using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AuthService.Data;
using System.Text.Json;
using System.Net.Http.Json;


namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AuthContext _context;

        public AdminController(AuthContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Username, u.Email, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Korisnik nije pronađen.");
            if (user.Role == "Admin") return BadRequest("Ne možete obrisati drugog admina.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok($"Korisnik {user.Username} uklonjen.");
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            using var httpClient = new HttpClient();
            try
            {
                var pokeStats = await httpClient.GetFromJsonAsync<JsonElement>("http://localhost:5033/api/stats");

                var stats = new
                {
                    TotalUsers = totalUsers,
                    TotalTeams = pokeStats.GetProperty("totalTeams").GetInt32(),
                    TotalReviews = pokeStats.GetProperty("totalReviews").GetInt32(),
                    TotalPokemons = pokeStats.GetProperty("totalPokemons").GetInt32()
                };
                return Ok(stats);
            }
            catch
            {
                return Ok(new { TotalUsers = totalUsers, Note = "PokeBackend nije dostupan" });
            }
        }
    }
}