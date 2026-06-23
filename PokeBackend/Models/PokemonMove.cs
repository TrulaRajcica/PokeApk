using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.Models
{
    public class PokemonMove
    {
        public int Id { get; set; }
        public int PokemonId { get; set; }
        public Pokemon? Pokemon { get; set; }
        public int MoveId { get; set; }
        public Move? Move { get; set; }
    }
}