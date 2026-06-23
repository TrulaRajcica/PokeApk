using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.Models
{
    public class TypeEffectivness
    {
        public int Id { get; set; }
        public string AttackingType { get; set; } = string.Empty;
        public string DefendingType { get; set; } = string.Empty;
        public double Multiplier { get; set; }
    }
}