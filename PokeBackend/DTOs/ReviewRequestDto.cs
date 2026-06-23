using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.DTOs
{
    public class ReviewRequestDto
    {
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int PokemonId { get; set; }
    }
}