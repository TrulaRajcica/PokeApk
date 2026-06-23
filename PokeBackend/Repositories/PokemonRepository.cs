using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PokeBackend.Data;
using PokeBackend.Interfaces;
using PokeBackend.Models;

namespace PokeBackend.Repositories
{
    public class PokemonRepository : IPokemonRepository
    {
        private readonly DataContext _context;

        public PokemonRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Pokemon?> GetPokemonByIdAsync(int id)
        {
            return await _context.Pokemons
                .Include(p => p.PokemonTypes)
                .Include(p => p.PokemonMoves).ThenInclude(pm => pm.Move)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pokemon>> GetAllPokemonsAsync()
        {
            return await _context.Pokemons.ToListAsync();
        }

        public async Task<bool> PokemonExistsAsync(int id)
        {
            return await _context.Pokemons.AnyAsync(p => p.Id == id);
        }

        public async Task AddPokemonAsync(Pokemon pokemon)
        {
            await _context.Pokemons.AddAsync(pokemon);
        }

        public async Task AddMoveAsync(Move move)
        {
            await _context.Moves.AddAsync(move);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddTeamAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
        }

        public async Task AddTeamMemberAsync(TeamMember member)
        {
            await _context.TeamMembers.AddAsync(member);
        }

        public async Task AddTeamMemberMoveAsync(TeamMemberMove memberMove)
        {
            await _context.AddAsync(memberMove);
        }

    }
}