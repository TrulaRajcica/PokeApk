using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PokeBackend.Models
{
    public class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CryUrl { get; set; } = string.Empty;
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpAttack { get; set; }
        public int SpDefense { get; set; }
        public int Speed { get; set; }
        public string Description { get; set; } = "Trenutno nemamo opis za ovoga pokemona :( ";

        public List<PokemonTypeMapping> PokemonTypes { get; set; } = new();
        public ICollection<PokemonMove> PokemonMoves { get; set; } = new List<PokemonMove>();

    }
}