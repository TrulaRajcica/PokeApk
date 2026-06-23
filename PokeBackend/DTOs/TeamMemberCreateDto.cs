using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.DTOs
{
    public class TeamMemberCreateDto
    {
        public int PokemonId { get; set; }
        public List<int> MoveIds { get; set; } = new();
    }
}