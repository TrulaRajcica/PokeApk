using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeBackend.Data;
using PokeBackend.Models;
using PokeBackend.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PokeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly DataContext _context;

        public ReviewController(DataContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview(ReviewRequestDto request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("Niste ulogirani.");

            var userId = int.Parse(userIdClaim);

            var review = new Review
            {
                Comment = request.Comment,
                Rating = request.Rating,
                PokemonId = request.PokemonId,
                UserId = userId
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok("Recenzija uspješno spremljena!");
        }

        [HttpGet("pokemon/{pokemonId}")]
        public async Task<IActionResult> GetReviewsForPokemon(int pokemonId)
        {
            var reviews = await _context.Reviews
            .Where(r => r.PokemonId == pokemonId)
               .Select(r => new
               {
                   r.Id,
                   r.Comment,
                   r.Rating,
                   r.UserId
               })
       .ToListAsync();

            return Ok(reviews);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewRequestDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null) return NotFound("Recenzija nije pronađena.");

            review.Comment = request.Comment;
            review.Rating = request.Rating;

            await _context.SaveChangesAsync();
            return Ok("Recenzija ažurirana.");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null) return Unauthorized();

            var userId = int.Parse(userIdString);

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
                return NotFound("Recenzija nije pronađena ili niste vlasnik.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Recenzija je uspješno obrisana.");
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return NotFound("Recenzija ne postoji.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Recenzija je uspješno uklonjena od strane admina.");
        }
    }
}