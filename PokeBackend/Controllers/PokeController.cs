using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokeBackend.DTOs;
using PokeBackend.Interfaces;
using PokeBackend.Services;

namespace PokeBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PokeController : ControllerBase
    {
        private readonly PokeService _pokeService;
        private readonly IPokemonRepository _pokemonRepo;

        public PokeController(PokeService pokeService, IPokemonRepository pokemonRepo)
        {
            _pokeService = pokeService;
            _pokemonRepo = pokemonRepo;
        }

        [HttpGet("sync")]
        public async Task<IActionResult> Sync()
        {
            await _pokeService.SyncPokemonsAsync(1025);
            return Ok(new { message = "Sync pokemona završen!" });
        }

        [HttpGet("sync-types")]
        public async Task<IActionResult> SyncTypes()
        {
            await _pokeService.SyncPokemonTypesAsync(1025);
            return Ok(new { message = "Sync tipova završen!" });
        }

        [HttpGet("sync-items")]
        public async Task<IActionResult> SyncItems()
        {
            await _pokeService.SyncItemsAsync(175);
            return Ok(new { message = "Sync itema završen!" });
        }

        [HttpGet("sync-moves")]
        public async Task<IActionResult> SyncMoves()
        {
            await _pokeService.SyncMovesAsync(750);
            return Ok(new { message = "Sync poteza završen!" });
        }

        [HttpGet("sync-pokemon-moves")]
        public async Task<IActionResult> SyncPokemonMoves()
        {
            await _pokeService.SyncPokemonMovesAsync(1025);
            return Ok(new { message = "Sync pokemon-moves završen!" });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PokemonDetailsDto>> GetPokemonDetails(int id)
        {
            var pokemon = await _pokemonRepo.GetPokemonByIdAsync(id);

            if (pokemon == null)
                return NotFound("Pokemon nije pronađen u bazi.");

            var details = new PokemonDetailsDto
            {
                Id = pokemon.Id,
                Name = pokemon.Name,
                ImageUrl = pokemon.ImageUrl,
                CryUrl = pokemon.CryUrl,
                Description = pokemon.Description,
                HP = pokemon.HP,
                Attack = pokemon.Attack,
                Defense = pokemon.Defense,
                SpAttack = pokemon.SpAttack,
                SpDefense = pokemon.SpDefense,
                Speed = pokemon.Speed,
                Types = pokemon.PokemonTypes.Select(t => t.TypeName).Distinct().ToList(),
                Moves = pokemon.PokemonMoves.Select(pm => new MoveDto
                {
                    Name = pm.Move!.Name,
                    TypeName = pm.Move.TypeName,
                    Power = pm.Move.Power,
                    Accuracy = pm.Move.Accuracy,
                    PP = pm.Move.PP
                }).ToList(),
                PreviousId = id > 1 ? id - 1 : 1025,
                NextId = id < 1025 ? id + 1 : 1
            };

            return Ok(details);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPokemon([FromQuery] string? search)
        {
            var pokemons = await _pokemonRepo.GetAllPokemonsAsync();

            if (!string.IsNullOrEmpty(search))
                pokemons = pokemons.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            var result = pokemons.Select(p => new { p.Id, p.Name, p.ImageUrl });

            return Ok(result);
        }
    }
}