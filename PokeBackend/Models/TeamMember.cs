using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
        public int PokemonId { get; set; }
        public Pokemon? Pokemon { get; set; }

        public ICollection<TeamMemberMove> Moves { get; set; } = new List<TeamMemberMove>();

    }
}