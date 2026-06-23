using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace PokeBackend.Models
{
    public class PokemonTypeMapping
    {
        public int Id { get; set; }
        public int PokemonId { get; set; }
        public string TypeName { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual Pokemon? Pokemon { get; set; }
    }
}