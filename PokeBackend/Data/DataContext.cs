using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PokeBackend.Models;

namespace PokeBackend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Pokemon> Pokemons { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<PokemonTypeMapping> PokemonTypes { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Move> Moves { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<TeamMemberMove> TeamMemberMoves { get; set; }
        public DbSet<PokemonMove> PokemonMoves { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}