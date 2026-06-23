using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokeBackend.Models;
using PokeBackend.DTOs;
using PokeBackend.Interfaces;

namespace PokeBackend.Services
{
    public class TeamService
    {
        private readonly IPokemonRepository _pokemonRepo;

        public TeamService(IPokemonRepository pokemonRepo)
        {
            _pokemonRepo = pokemonRepo;
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

            await _pokemonRepo.AddTeamAsync(newTeam);
            await _pokemonRepo.SaveAsync();

            foreach (var memberDto in request.Members)
            {
                var teamMember = new TeamMember
                {
                    TeamId = newTeam.Id,
                    PokemonId = memberDto.PokemonId
                };

                await _pokemonRepo.AddTeamMemberAsync(teamMember);
                await _pokemonRepo.SaveAsync();

                foreach (var moveId in memberDto.MoveIds)
                {
                    var memberMove = new TeamMemberMove
                    {
                        TeamMemberId = teamMember.Id,
                        MoveId = moveId
                    };
                    await _pokemonRepo.AddTeamMemberMoveAsync(memberMove);
                }
            }

            await _pokemonRepo.SaveAsync();
            return true;
        }
    }
}