using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokeBackend.Models;

namespace PokeBackend.Interfaces
{
    public interface IPokemonRepository
    {
        Task<Pokemon?> GetPokemonByIdAsync(int id);
        Task<IEnumerable<Pokemon>> GetAllPokemonsAsync();
        Task<bool> PokemonExistsAsync(int id);
        Task AddPokemonAsync(Pokemon pokemon);
        Task AddMoveAsync(Move move);
        Task SaveAsync();
    }
}