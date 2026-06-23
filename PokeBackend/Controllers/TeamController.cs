using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeBackend.Data;
using PokeBackend.Models;
using PokeBackend.DTOs;
using PokeBackend.Services;

namespace PokeBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly TeamAnalyzerService _analyzerService;
        private readonly TeamService _teamService;
        private readonly DataContext _context;

        public TeamController(TeamAnalyzerService analyzerService, TeamService teamService, DataContext context)
        {
            _analyzerService = analyzerService;
            _teamService = teamService;
            _context = context;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeMyTeam([FromBody] TeamRequestDto request)
        {
            if (!_teamService.IsTeamSizeValid(request.Members.Count))
            {
                return BadRequest("Tim mora imati između 1 i 6 Pokemona!");
            }

            var analysis = await _analyzerService.AnalyzeTeamAsync(request);
            return Ok(analysis);
        }
        [HttpPost("analyze-saved/{teamId}")]
        [Authorize]
        public async Task<IActionResult> AnalyzeSavedTeam(int teamId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null) return Unauthorized();
            var userId = int.Parse(userIdString);

            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                    .ThenInclude(m => m.Moves)
                        .ThenInclude(tm => tm.Move)
                .FirstOrDefaultAsync(t => t.Id == teamId && t.UserId == userId);

            if (team == null) return NotFound("Tim nije pronađen.");

            var request = new TeamRequestDto
            {
                Name = team.Name,
                Members = team.TeamMembers.Select(m => new TeamMemberRequestDto
                {
                    PokemonId = m.PokemonId,
                    MoveIds = m.Moves.Select(mv => mv.MoveId).ToList()
                }).ToList()
            };

            var analysisResult = await _analyzerService.AnalyzeTeamAsync(request);
            return Ok(analysisResult);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTeam([FromBody] TeamRequestDto teamDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("Korisnik nije prepoznat.");

            var success = await _teamService.CreateTeamAsync(userId, teamDto);

            if (!success)
            {
                return BadRequest("Neuspješno kreiranje tima. Provjerite broj članova (max 6).");
            }

            return Ok(new { message = "Tim je uspješno kreiran i spremljen!" });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] TeamRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null) return Unauthorized();
            var userId = int.Parse(userIdString);

            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (team == null) return NotFound("Tim nije pronađen.");

            team.Name = request.Name;

            _context.TeamMembers.RemoveRange(team.TeamMembers);

            foreach (var memberDto in request.Members)
            {
                var member = new TeamMember
                {
                    TeamId = team.Id,
                    PokemonId = memberDto.PokemonId
                };
                _context.TeamMembers.Add(member);
            }

            await _context.SaveChangesAsync();
            return Ok("Tim je uspješno ažuriran!");
        }

        [HttpPut("member/{memberId}")]
        [Authorize]
        public async Task<IActionResult> UpdateTeamMember(int memberId, [FromBody] int newPokemonId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var member = await _context.TeamMembers
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == memberId && m.Team.UserId == userId);

            if (member == null) return NotFound("Član tima nije pronađen.");

            member.PokemonId = newPokemonId;
            await _context.SaveChangesAsync();
            return Ok("Pokemon u timu je zamijenjen.");
        }


        [HttpDelete("member/{memberId}")]
        [Authorize]
        public async Task<IActionResult> RemoveMember(int memberId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var member = await _context.TeamMembers
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == memberId && m.Team.UserId == userId);

            if (member == null) return NotFound("Član nije pronađen.");

            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();
            return Ok("Član uklonjen iz tima.");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null) return Unauthorized();
            var userId = int.Parse(userIdString);

            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (team == null) return NotFound("Tim nije pronađen.");

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return Ok("Tim obrisan.");
        }


        [HttpGet("my-teams")]
        [Authorize]
        public async Task<IActionResult> GetMyTeams()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var teams = await _context.Teams
                .Where(t => t.UserId == userId)
                .Include(t => t.TeamMembers)
                    .ThenInclude(m => m.Pokemon)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.CreatedAt,
                    Members = t.TeamMembers.Select(m => new
                    {
                        m.Id,
                        m.PokemonId,
                        PokemonName = m.Pokemon != null ? m.Pokemon.Name : "Unknown",
                        PokemonImage = m.Pokemon != null ? m.Pokemon.ImageUrl : ""
                    }).ToList()
                })
                .ToListAsync();

            return Ok(teams);
        }


    }
}