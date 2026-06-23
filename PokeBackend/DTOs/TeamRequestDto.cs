using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PokeBackend.DTOs
{
    public class TeamRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public List<TeamMemberRequestDto> Members { get; set; } = new();
        public List<int> ItemIds { get; set; } = new();
    }
}