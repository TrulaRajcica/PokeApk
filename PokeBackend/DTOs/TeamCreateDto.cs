using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.DTOs
{
    public class TeamCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public List<TeamMemberCreateDto> Members { get; set; } = new();
    }
}