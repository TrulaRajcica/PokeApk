using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokeBackend.DTOs;

namespace PokeBackend.DTOs
{
    public class PokemonDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CryUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Types { get; set; } = new();
        public List<MoveDto> Moves { get; set; } = new();
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpAttack { get; set; }
        public int SpDefense { get; set; }
        public int Speed { get; set; }
        public int PreviousId { get; set; }
        public int NextId { get; set; }
    }
}