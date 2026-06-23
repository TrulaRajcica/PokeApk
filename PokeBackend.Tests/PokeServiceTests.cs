using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PokeBackend.Data;
using PokeBackend.DTOs;
using PokeBackend.Services;
using Xunit;

namespace PokeBackend.Tests
{
    public class PokeServiceTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly TeamService _service;

        public PokeServiceTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockContext = new Mock<DataContext>(options) { CallBase = true };
            _service = new TeamService(_mockContext.Object);
        }

        [Fact]
        public void IsTeamSizeValid_ReturnsTrue_WhenSizeIsBetweenOneAndSix()
        {
            var result = _service.IsTeamSizeValid(3);
            Assert.True(result);
        }

        [Fact]
        public void IsTeamSizeValid_ReturnsFalse_WhenSizeIsZero()
        {
            var result = _service.IsTeamSizeValid(0);
            Assert.False(result);
        }

        [Fact]
        public void IsTeamSizeValid_ReturnsFalse_WhenSizeIsOverSix()
        {
            var result = _service.IsTeamSizeValid(7);
            Assert.False(result);
        }

        [Fact]
        public async Task CreateTeamAsync_ReturnsFalse_WhenTeamSizeIsInvalid()
        {
            var invalidRequest = new TeamRequestDto
            {
                Name = "Tim bez Pokemona",
                Members = new List<TeamMemberRequestDto>()
            };

            var result = await _service.CreateTeamAsync("1", invalidRequest);
            Assert.False(result);
        }

        [Fact]
        public async Task CreateTeamAsync_ReturnsTrue_AndSavesToDatabase_WhenRequestIsValid()
        {
            var validRequest = new TeamRequestDto
            {
                Name = "Pobjednici",
                Members = new List<TeamMemberRequestDto>
                {
                    new TeamMemberRequestDto { PokemonId = 25 }
                }
            };

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);
            var result = await _service.CreateTeamAsync("1", validRequest);
            Assert.True(result);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateTeamAsync_HandlesDatabaseError_WhenDatabaseGoesDown()
        {
            var validRequest = new TeamRequestDto
            {
                Name = "Gubitnici",
                Members = new List<TeamMemberRequestDto>
                {
                    new TeamMemberRequestDto { PokemonId = 1 }
                }
            };

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new DbUpdateException("Baza je pala!"));
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
                _service.CreateTeamAsync("1", validRequest));

            Assert.Equal("Baza je pala!", exception.Message);
        }
    }
}