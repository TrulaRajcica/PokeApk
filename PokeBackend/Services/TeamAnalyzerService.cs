using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokeBackend.DTOs;
using PokeBackend.Interfaces;
using PokeBackend.Models;

namespace PokeBackend.Services
{
    public class TeamAnalyzerService
    {
        private readonly IPokemonRepository _pokemonRepo;

        private static readonly Dictionary<string, (string[] Strong, string[] Weak)> TypeChart = new()
        {
            { "normal", (new string[] { }, new string[] { "rock", "steel" }) },
            { "fire", (new[] { "grass", "bug", "ice", "steel" }, new[] { "fire", "water", "rock", "dragon" }) },
            { "water", (new[] { "fire", "ground", "rock" }, new[] { "water", "grass", "dragon" }) },
            { "grass", (new[] { "water", "ground", "rock" }, new[] { "fire", "grass", "poison", "flying", "bug", "dragon", "steel" }) },
            { "electric", (new[] { "water", "flying" }, new[] { "electric", "grass", "dragon" }) },
            { "ice", (new[] { "grass", "ground", "flying", "dragon" }, new[] { "fire", "water", "ice", "steel" }) },
            { "fighting", (new[] { "normal", "ice", "rock", "dark", "steel" }, new[] { "poison", "flying", "psychic", "bug", "fairy" }) },
            { "poison", (new[] { "grass", "fairy" }, new[] { "poison", "ground", "rock", "ghost" }) },
            { "ground", (new[] { "fire", "electric", "poison", "rock", "steel" }, new[] { "grass", "bug" }) },
            { "flying", (new[] { "grass", "fighting", "bug" }, new[] { "electric", "rock", "steel" }) },
            { "psychic", (new[] { "fighting", "poison" }, new[] { "psychic", "steel" }) },
            { "bug", (new[] { "grass", "psychic", "dark" }, new[] { "fire", "fighting", "poison", "flying", "ghost", "steel", "fairy" }) },
            { "rock", (new[] { "fire", "ice", "flying", "bug" }, new[] { "fighting", "ground", "steel" }) },
            { "ghost", (new[] { "psychic", "ghost" }, new[] { "dark" }) },
            { "dragon", (new[] { "dragon" }, new[] { "steel" }) },
            { "dark", (new[] { "psychic", "ghost" }, new[] { "fighting", "dark", "fairy" }) },
            { "steel", (new[] { "ice", "rock", "fairy" }, new[] { "fire", "water", "electric", "steel" }) },
            { "fairy", (new[] { "fighting", "dragon", "dark" }, new[] { "fire", "poison", "steel" }) }
        };

        public TeamAnalyzerService(IPokemonRepository pokemonRepo)
        {
            _pokemonRepo = pokemonRepo;
        }

        public async Task<TeamAnalysisResultDto> AnalyzeTeamAsync(TeamRequestDto request)
        {
            var result = new TeamAnalysisResultDto();
            var pokemonIds = request.Members.Select(m => m.PokemonId).Distinct().ToList();
            var moveIds = request.Members.SelectMany(m => m.MoveIds).Distinct().ToList();

            var team = new List<Pokemon>();
            foreach (var id in pokemonIds)
            {
                var pokemon = await _pokemonRepo.GetPokemonByIdAsync(id);
                if (pokemon != null)
                {
                    team.Add(pokemon);
                }
            }

            var moves = team
                .Where(p => p.PokemonMoves != null)
                .SelectMany(p => p.PokemonMoves.Select(pm => pm.Move))
                .Where(m => m != null && moveIds.Contains(m.Id))
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .ToList();

            var pokemonTypes = team
                .Where(p => p.PokemonTypes != null)
                .SelectMany(p => p.PokemonTypes.Select(pt => pt.TypeName?.ToLower() ?? "unknown"))
                .Distinct().ToList();

            var moveTypes = moves
                .Select(m => m.TypeName.ToLower())
                .Distinct().ToList();

            result.TeamMembers = team.Select(p => p.Name).ToList();
            result.TeamTypes = pokemonTypes;

            var offensiveStrengths = new HashSet<string>();
            var defensiveWeaknesses = new HashSet<string>();

            foreach (var type in moveTypes)
                if (TypeChart.ContainsKey(type))
                    foreach (var s in TypeChart[type].Strong)
                        offensiveStrengths.Add(s);

            foreach (var type in pokemonTypes)
                if (TypeChart.ContainsKey(type))
                    foreach (var w in TypeChart[type].Weak)
                        defensiveWeaknesses.Add(w);

            result.StrongAgainst = offensiveStrengths.OrderBy(s => s).ToList();
            result.WeakAgainst = defensiveWeaknesses.OrderBy(w => w).ToList();
            result.Summary = GenerateSummary(pokemonTypes, result.WeakAgainst);

            return result;
        }

        private List<string> GenerateSummary(List<string> teamTypes, List<string> weaknesses)
        {
            var summaryParts = new List<string>();

            if (teamTypes.Count < 2)
            {
                summaryParts.Add("Please add more Pokémon to generate a detailed competitive team analysis.");
                return summaryParts;
            }

            if (weaknesses.Contains("water") && !teamTypes.Contains("grass") && !teamTypes.Contains("electric"))
                summaryParts.Add("The team has a defensive vulnerability to Water-type attacks. Consider adding Grass or Electric-type coverage to counter opposing Water-types.");

            if (weaknesses.Contains("fire") && !teamTypes.Contains("water") && !teamTypes.Contains("rock") && !teamTypes.Contains("fire"))
                summaryParts.Add("The team is vulnerable to Fire-type offensive threats. Integrating a Water, Rock, or Ground-type Pokémon will provide necessary defensive utility against moves like Flamethrower.");

            if (weaknesses.Contains("grass") && !teamTypes.Contains("fire") && !teamTypes.Contains("ice") && !teamTypes.Contains("flying") && !teamTypes.Contains("bug") && !teamTypes.Contains("poison"))
                summaryParts.Add("The team lacks sufficient defensive checks against Grass-type Pokémon. Consider incorporating Fire, Ice, or Flying-type offensive coverage.");

            if (weaknesses.Contains("electric") && !teamTypes.Contains("ground"))
                summaryParts.Add("The team is susceptible to high-damage Electric-type moves (e.g., Thunderbolt). Adding a Ground-type Pokémon is highly recommended due to their complete immunity to Electric-type attacks.");

            if (weaknesses.Contains("ice") && !teamTypes.Contains("fire") && !teamTypes.Contains("fighting") && !teamTypes.Contains("rock") && !teamTypes.Contains("steel"))
                summaryParts.Add("Ice-type attacks represent a significant threat to your team's synergy. A Fire or Fighting-type offensive counter is recommended to break through Ice-type defenses.");

            if (weaknesses.Contains("fighting") && !teamTypes.Contains("flying") && !teamTypes.Contains("psychic") && !teamTypes.Contains("fairy"))
                summaryParts.Add("The team exhibits a collective weakness to Fighting-type physical attackers. Utilizing Psychic, Flying, or Fairy-type Pokémon can leverage natural resistances.");

            if (weaknesses.Contains("poison") && !teamTypes.Contains("ground") && !teamTypes.Contains("psychic") && !teamTypes.Contains("steel"))
                summaryParts.Add("Your roster is vulnerable to Poison-type strategies and poison status conditions. A Steel-type Pokémon offers complete immunity, while Ground and Psychic-types provide effective offensive counters.");

            if (weaknesses.Contains("ground") && !teamTypes.Contains("flying") && !teamTypes.Contains("grass") && !teamTypes.Contains("water"))
                summaryParts.Add("The team is heavily exposed to Ground-type offensive moves, particularly Earthquake. Adding a Flying-type Pokémon will provide crucial immunity to Ground-type damage.");

            if (weaknesses.Contains("flying") && !teamTypes.Contains("electric") && !teamTypes.Contains("ice") && !teamTypes.Contains("rock"))
                summaryParts.Add("Opposing Flying-type offensive threats could outmaneuver your current core. Bolster your defenses with an Electric or Rock-type counter.");

            if (weaknesses.Contains("psychic") && !teamTypes.Contains("bug") && !teamTypes.Contains("ghost") && !teamTypes.Contains("dark"))
                summaryParts.Add("The team lacks proper defensive measures against Psychic-type setups. Dark-type Pokémon provide full immunity, while Ghost and Bug-types can deal super-effective damage.");

            if (weaknesses.Contains("bug") && !teamTypes.Contains("fire") && !teamTypes.Contains("flying") && !teamTypes.Contains("rock"))
                summaryParts.Add("The team could face structural issues against potent Bug-type setups. Fire and Flying-type moves offer optimal defensive and offensive coverage.");

            if (weaknesses.Contains("rock") && !teamTypes.Contains("water") && !teamTypes.Contains("grass") && !teamTypes.Contains("fighting") && !teamTypes.Contains("ground") && !teamTypes.Contains("steel"))
                summaryParts.Add("There is a notable weakness to Rock-type offensive moves (e.g., Rock Slide, Stone Edge). Leverage Rock-type weaknesses by adding Water, Grass, Fighting, or Ground-type utility.");

            if (weaknesses.Contains("ghost") && !teamTypes.Contains("ghost") && !teamTypes.Contains("dark"))
                summaryParts.Add("Ghost-type offensive threats can easily bypass your defensive core. A fast Ghost-type for a mirror matchup or a reliable Dark-type for structural resistance is advised.");

            if (weaknesses.Contains("dragon") && !teamTypes.Contains("ice") && !teamTypes.Contains("dragon") && !teamTypes.Contains("fairy"))
                summaryParts.Add("High-powered Dragon-type Pokémon present a major sweep hazard to this team. A Fairy-type Pokémon guarantees an immunity check, while Ice-types provide essential super-effective damage output.");

            if (weaknesses.Contains("dark") && !teamTypes.Contains("fighting") && !teamTypes.Contains("bug") && !teamTypes.Contains("fairy"))
                summaryParts.Add("The team has defensive gaps against Dark-type physical or special offensive threats. Integrating Fairy-type utility or Fighting-type raw power is highly effective.");

            if (weaknesses.Contains("steel") && !teamTypes.Contains("fire") && !teamTypes.Contains("fighting") && !teamTypes.Contains("ground"))
                summaryParts.Add("The defensive bulk of opposing Steel-type Pokémon may stall your offensive momentum. Fire, Fighting, or Ground-type moves are necessary to breach their defensive profile.");

            if (weaknesses.Contains("fairy") && !teamTypes.Contains("poison") && !teamTypes.Contains("steel"))
                summaryParts.Add("Fairy-type offensive strategies can disrupt your team's tactical flow. Incorporating Poison or Steel-type Pokémon will assist in neutralizing these threats.");

            if (weaknesses.Contains("normal") && !teamTypes.Contains("fighting"))
                summaryParts.Add("The team lacks an optimal counter to high-base-power Normal-type physical moves. Introducing a Fighting-type for super-effective damage or a Ghost-type for complete immunity is recommended.");

            if (summaryParts.Count == 0)
                summaryParts.Add("The team's defensive and offensive profiles are exceptionally well-balanced. There are no immediate type-coverage gaps identified for competitive play.");

            return summaryParts;
        }
    }
}