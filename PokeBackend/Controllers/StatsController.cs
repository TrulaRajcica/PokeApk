using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeBackend.Data;

namespace PokeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly DataContext _context;

        public StatsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                totalTeams = await _context.Teams.CountAsync(),
                totalReviews = await _context.Reviews.CountAsync(),
                totalPokemons = await _context.Pokemons.CountAsync(),
                totalMoves = await _context.Moves.CountAsync(),
                totalItems = await _context.Items.CountAsync()
            };
            return Ok(stats);
        }
    }
}