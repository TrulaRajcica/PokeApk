using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PokeBackend.DTOs
{
    public class TeamAnalysisResultDto
    {
        public List<string> TeamMembers { get; set; } = new List<string>();
        public List<string> TeamTypes { get; set; } = new List<string>();
        public List<string> StrongAgainst { get; set; } = new List<string>();
        public List<string> WeakAgainst { get; set; } = new List<string>();
        public List<string> Summary { get; set; } = new();
    }
}