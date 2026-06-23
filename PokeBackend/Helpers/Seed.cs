using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokeBackend.Data;
using PokeBackend.Models;

namespace PokeBackend.Helpers
{
    public class Seed
    {
        private readonly DataContext _context;

        public Seed(DataContext context)
        {
            _context = context;
        }

        public async Task SeedDataContext()
        {
            if (!_context.Pokemons.Any())
            {

                Console.WriteLine("Baza je prazna, krećem s punjenjem...");

                var testPokemon = new Pokemon
                {
                    Name = "Squirtle",
                    ImageUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/7.png",
                    HP = 44,
                    Attack = 48,
                    Defense = 65,
                    SpAttack = 50,
                    SpDefense = 64,
                    Speed = 43
                };
                _context.Pokemons.Add(testPokemon);
                await _context.SaveChangesAsync();

                Console.WriteLine("Squirtle je uspješno ubačen!");
            }
        }
    }
}