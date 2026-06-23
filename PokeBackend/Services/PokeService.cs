using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PokeBackend.Data;
using PokeBackend.Models;
using PokeBackend.Interfaces;

namespace PokeBackend.Services
{
    public class PokeService
    {
        private readonly DataContext _context;
        private readonly HttpClient _httpClient;
        private readonly IPokemonRepository _pokemonRepo;

        public PokeService(DataContext context, IPokemonRepository pokemonRepo)
        {
            _context = context;
            _pokemonRepo = pokemonRepo;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task SyncPokemonsAsync(int count = 1025)
        {
            var tasks = new List<Task<(int id, JsonElement pokeData, JsonElement speciesData)>>();

            for (int i = 1; i <= count; i++)
            {
                if (await _context.Pokemons.AnyAsync(p => p.Id == i)) continue;
                int id = i;
                tasks.Add(Task.Run(async () =>
                {
                    var pokeData = await _httpClient.GetFromJsonAsync<JsonElement>($"https://pokeapi.co/api/v2/pokemon/{id}");
                    var speciesData = await _httpClient.GetFromJsonAsync<JsonElement>($"https://pokeapi.co/api/v2/pokemon-species/{id}");
                    return (id, pokeData, speciesData);
                }));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var (id, pokeData, speciesData) in results)
            {
                try
                {
                    string flavorText = "No description available.";
                    foreach (var entry in speciesData.GetProperty("flavor_text_entries").EnumerateArray())
                    {
                        if (entry.GetProperty("language").GetProperty("name").GetString() == "en")
                        {
                            flavorText = entry.GetProperty("flavor_text").GetString()
                                ?.Replace("\n", " ").Replace("\f", " ") ?? flavorText;
                            break;
                        }
                    }

                    var newPokemon = new Pokemon
                    {
                        Id = id,
                        Name = pokeData.GetProperty("name").GetString() ?? "Unknown",
                        ImageUrl = pokeData.GetProperty("sprites").GetProperty("other")
                            .GetProperty("official-artwork").GetProperty("front_default").GetString() ?? "",
                        CryUrl = pokeData.GetProperty("cries").GetProperty("latest").GetString() ?? "",
                        HP = pokeData.GetProperty("stats")[0].GetProperty("base_stat").GetInt32(),
                        Attack = pokeData.GetProperty("stats")[1].GetProperty("base_stat").GetInt32(),
                        Defense = pokeData.GetProperty("stats")[2].GetProperty("base_stat").GetInt32(),
                        SpAttack = pokeData.GetProperty("stats")[3].GetProperty("base_stat").GetInt32(),
                        SpDefense = pokeData.GetProperty("stats")[4].GetProperty("base_stat").GetInt32(),
                        Speed = pokeData.GetProperty("stats")[5].GetProperty("base_stat").GetInt32(),
                        Description = flavorText
                    };

                    _context.Pokemons.Add(newPokemon);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška na ID {id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Uspješno sinkronizirano {count} pokemona :)");
        }

        public async Task SyncMovesAsync(int count = 750)
        {
            var tasks = new List<Task<(int id, JsonElement moveData)>>();

            for (int i = 1; i <= count; i++)
            {
                if (await _context.Moves.AnyAsync(m => m.Id == i)) continue;
                int id = i;
                tasks.Add(Task.Run(async () =>
                {
                    var moveData = await _httpClient.GetFromJsonAsync<JsonElement>($"https://pokeapi.co/api/v2/move/{id}");
                    return (id, moveData);
                }));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var (id, moveData) in results)
            {
                try
                {
                    var newMove = new Move
                    {
                        Id = id,
                        Name = moveData.GetProperty("name").GetString() ?? "Unknown",
                        TypeName = moveData.GetProperty("type").GetProperty("name").GetString() ?? "normal",
                        Power = moveData.TryGetProperty("power", out var power) && power.ValueKind != JsonValueKind.Null
                            ? power.GetInt32() : null,
                        Accuracy = moveData.TryGetProperty("accuracy", out var acc) && acc.ValueKind != JsonValueKind.Null
                            ? acc.GetInt32() : null,
                        PP = moveData.TryGetProperty("pp", out var pp) && pp.ValueKind != JsonValueKind.Null
                            ? pp.GetInt32() : null
                    };

                    _context.Moves.Add(newMove);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška na move ID {id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Sinkronizirano {count} poteza");
        }

        public async Task SyncItemsAsync(int count = 175)
        {
            var tasks = new List<Task<(int id, JsonElement itemData)>>();

            for (int i = 1; i <= count; i++)
            {
                if (await _context.Items.AnyAsync(item => item.Id == i)) continue;
                int id = i;
                tasks.Add(Task.Run(async () =>
                {
                    var itemData = await _httpClient.GetFromJsonAsync<JsonElement>($"https://pokeapi.co/api/v2/item/{id}");
                    return (id, itemData);
                }));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var (id, itemData) in results)
            {
                try
                {
                    var newItem = new Item
                    {
                        Id = id,
                        Name = itemData.GetProperty("name").GetString() ?? "Unknown",
                        Effect = itemData.GetProperty("effect_entries").GetArrayLength() > 0
                            ? itemData.GetProperty("effect_entries")[0].GetProperty("short_effect").GetString() ?? ""
                            : "",
                        Description = itemData.GetProperty("flavor_text_entries").GetArrayLength() > 0
                            ? itemData.GetProperty("flavor_text_entries")[0].GetProperty("text").GetString() ?? ""
                            : "No description available.",
                        ImageUrl = itemData.GetProperty("sprites").GetProperty("default").GetString() ?? ""
                    };

                    _context.Items.Add(newItem);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška na item IDu {id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Sinkronizirano {count} itema");
        }

        public async Task SyncPokemonTypesAsync(int count = 1025)
        {
            var tasks = new List<Task<(int id, JsonElement pokeData)>>();

            for (int i = 1; i <= count; i++)
            {
                if (await _context.PokemonTypes.AnyAsync(pt => pt.PokemonId == i)) continue;
                int id = i;
                tasks.Add(Task.Run(async () =>
                {
                    var pokeData = await _httpClient.GetFromJsonAsync<JsonElement>($"https://pokeapi.co/api/v2/pokemon/{id}");
                    return (id, pokeData);
                }));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var (id, pokeData) in results)
            {
                try
                {
                    var types = pokeData.GetProperty("types");
                    foreach (var typeObj in types.EnumerateArray())
                    {
                        var newType = new PokemonTypeMapping
                        {
                            PokemonId = id,
                            TypeName = typeObj.GetProperty("type").GetProperty("name").GetString() ?? "unknown"
                        };
                        _context.PokemonTypes.Add(newType);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška na type ID {id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Sinkronizirani su tipovi za {count} pokemona!!!");
        }

        public async Task SyncPokemonMovesAsync(int count = 1025)
        {
            for (int i = 1; i <= count; i++)
            {
                if (await _context.PokemonMoves.AnyAsync(pm => pm.PokemonId == i)) continue;

                try
                {
                    var pokeData = await _httpClient.GetFromJsonAsync<JsonElement>($"https://pokeapi.co/api/v2/pokemon/{i}");
                    var moves = pokeData.GetProperty("moves");

                    foreach (var moveEntry in moves.EnumerateArray())
                    {
                        var moveName = moveEntry.GetProperty("move").GetProperty("name").GetString();
                        var move = await _context.Moves.FirstOrDefaultAsync(m => m.Name == moveName);
                        if (move == null) continue;

                        _context.PokemonMoves.Add(new PokemonMove
                        {
                            PokemonId = i,
                            MoveId = move.Id
                        });
                    }

                    Console.WriteLine($"Potezi za pokemona {i} su uspješno dodani");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška na ID-u {i}: {ex.Message}");
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}