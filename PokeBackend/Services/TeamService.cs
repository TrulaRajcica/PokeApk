using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokeBackend.Data;
using PokeBackend.Models;
using PokeBackend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace PokeBackend.Services
{
    public class TeamService
    {
        private readonly DataContext _context;

        public TeamService(DataContext context)
        {
            _context = context;
        }

        public bool IsTeamSizeValid(int memberCount)
        {
            return memberCount > 0 && memberCount <= 6;
        }

        public async Task<bool> CreateTeamAsync(string userId, TeamRequestDto request)
        {
            if (!IsTeamSizeValid(request.Members.Count))
                return false;

            var newTeam = new Team
            {
                Name = request.Name,
                UserId = int.Parse(userId),
                CreatedAt = DateTime.Now
            };

            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            foreach (var memberDto in request.Members)
            {
                var teamMember = new TeamMember
                {
                    TeamId = newTeam.Id,
                    PokemonId = memberDto.PokemonId
                };

                _context.TeamMembers.Add(teamMember);
                await _context.SaveChangesAsync();
                foreach (var moveId in memberDto.MoveIds)
                {
                    var memberMove = new TeamMemberMove
                    {
                        TeamMemberId = teamMember.Id,
                        MoveId = moveId
                    };
                    _context.Add(memberMove);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}