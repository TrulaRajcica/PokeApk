using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.Models
{
    public class Move
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public int? Power { get; set; }
        public int? Accuracy { get; set; }
        public int? PP { get; set; }
    }
}