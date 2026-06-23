using Microsoft.AspNetCore.Mvc;
using Moq;
using PokeBackend.Controllers;
using PokeBackend.DTOs;
using PokeBackend.Interfaces;
using PokeBackend.Models;
using PokeBackend.Services;


namespace PokeBackend.Tests
{
    // OVO ZANEMARI ALI KAD VEC IMAS OSTAVI UNIT TESTOVI SU NA SERVISIMA NE KONTROLERIMA !!!!!!!!!!!!!!!!
    public class PokeControllerTests
    {
        private readonly Mock<IPokemonRepository> _mockRepo;
        private readonly Mock<PokeService> _mockPokeService;

        public PokeControllerTests()
        {
            _mockRepo = new Mock<IPokemonRepository>();
        }

        [Fact]
        public async Task GetPokemonDetails_ReturnsNotFound_WhenPokemonDoesNotExist()
        {

            _mockRepo.Setup(r => r.GetPokemonByIdAsync(9999)).ReturnsAsync((Pokemon?)null);
            var controller = new PokeController(null!, _mockRepo.Object);

            var result = await controller.GetPokemonDetails(9999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPokemonDetails_ReturnsOk_WhenPokemonExists()
        {
            var fakePokemon = new Pokemon
            {
                Id = 1,
                Name = "bulbasaur",
                ImageUrl = "http://example.com/1.png",
                CryUrl = "http://example.com/1.ogg",
                Description = "Test opis",
                HP = 45,
                Attack = 49,
                Defense = 49,
                SpAttack = 65,
                SpDefense = 65,
                Speed = 45,
                PokemonTypes = new List<PokemonTypeMapping>
                {
                    new PokemonTypeMapping { TypeName = "grass" }
                },
                PokemonMoves = new List<PokemonMove>()
            };

            _mockRepo.Setup(r => r.GetPokemonByIdAsync(1)).ReturnsAsync(fakePokemon);
            var controller = new PokeController(null!, _mockRepo.Object);
            var result = await controller.GetPokemonDetails(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PokemonDetailsDto>(okResult.Value);
            Assert.Equal("bulbasaur", dto.Name);
        }

        [Fact]
        public async Task GetPokemonDetails_ReturnsPreviousId_WhenIdIsFirst()
        {
            var fakePokemon = new Pokemon
            {
                Id = 1,
                Name = "bulbasaur",
                PokemonTypes = new List<PokemonTypeMapping>(),
                PokemonMoves = new List<PokemonMove>()
            };
            _mockRepo.Setup(r => r.GetPokemonByIdAsync(1)).ReturnsAsync(fakePokemon);
            var controller = new PokeController(null!, _mockRepo.Object);

            var result = await controller.GetPokemonDetails(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PokemonDetailsDto>(okResult.Value);
            Assert.Equal(1025, dto.PreviousId);
        }
        [Fact]
        public async Task GetPokemonDetails_ReturnsNextId_WhenIdIsLast()
        {

            var fakePokemon = new Pokemon
            {
                Id = 1025,
                Name = "pecharunt",
                PokemonTypes = new List<PokemonTypeMapping>(),
                PokemonMoves = new List<PokemonMove>()
            };

            _mockRepo.Setup(r => r.GetPokemonByIdAsync(1025)).ReturnsAsync(fakePokemon);
            var controller = new PokeController(null!, _mockRepo.Object);
            var result = await controller.GetPokemonDetails(1025);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PokemonDetailsDto>(okResult.Value);
            Assert.Equal(1, dto.NextId);
        }
    }
}