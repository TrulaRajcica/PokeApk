using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }

        public int UserId { get; set; }

        public int PokemonId { get; set; }
        public Pokemon? Pokemon { get; set; }
    }
}