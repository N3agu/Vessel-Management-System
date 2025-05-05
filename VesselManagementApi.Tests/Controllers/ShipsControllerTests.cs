using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using System.Net;
using VesselManagementApi.DTOs;
using VesselManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using VesselManagementApi.Models;

namespace VesselManagementApi.Tests.Controllers
{
    public class ShipsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ShipsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private IServiceScope CreateScope()
        {
            var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
            return scopeFactory.CreateScope();
        }

        private string GenerateUniqueImo()
        {
            long ticks = DateTime.UtcNow.Ticks;
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 4);
            string combined = $"{ticks}{guidPart}";
            string potentialImo = new string(combined.Where(char.IsDigit).Reverse().Take(7).Reverse().ToArray());
            return potentialImo.PadLeft(7, '0');
        }

        [Fact]
        public async Task GetShips_ReturnsOkAndListOfShips()
        {
            // Act
            var response = await _client.GetAsync("/api/ships");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var ships = await response.Content.ReadFromJsonAsync<List<ShipDto>>();
            Assert.NotNull(ships);
            Assert.Contains(ships, s => s.ImoNumber == "1111111");
            Assert.Contains(ships, s => s.ImoNumber == "2222222");
        }

        [Fact]
        public async Task GetShipDetails_ExistingId_ReturnsOkAndShipWithOwners()
        {
            // Arrange
            int existingShipId;
            List<int> expectedOwnerIds;
            using (var scope = CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var seededShip = await context.Ships
                                             .Include(s => s.ShipOwners)
                                             .ThenInclude(so => so.Owner)
                                             .FirstOrDefaultAsync(s => s.ImoNumber == "1111111");
                Assert.NotNull(seededShip);
                existingShipId = seededShip.Id;
                expectedOwnerIds = seededShip.ShipOwners.Select(so => so.OwnerId).ToList();
            }


            // Act
            var response = await _client.GetAsync($"/api/ships/{existingShipId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var shipDetails = await response.Content.ReadFromJsonAsync<ShipDetailsDto>();
            Assert.NotNull(shipDetails);
            Assert.Equal(existingShipId, shipDetails.Id);
            Assert.Equal("1111111", shipDetails.ImoNumber);
            Assert.NotNull(shipDetails.Owners);
            Assert.Equal(expectedOwnerIds.Count, shipDetails.Owners.Count);
            foreach (var ownerId in expectedOwnerIds)
            {
                Assert.Contains(shipDetails.Owners, o => o.Id == ownerId);
            }
        }

        [Fact]
        public async Task GetShipDetails_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = -999;

            // Act
            var response = await _client.GetAsync($"/api/ships/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateShip_ValidData_ReturnsCreatedAndShipDetails()
        {
            // Arrange
            int ownerId;
            using (var scope = CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var owner = await context.Owners.FirstAsync(); // Get first available owner
                ownerId = owner.Id;
            }

            string uniqueImo = GenerateUniqueImo();

            var newShipDto = new CreateShipDto
            {
                Name = "Brand New Ship",
                ImoNumber = uniqueImo,
                Type = "Cruise",
                Tonnage = 50000,
                OwnerIds = new List<int> { ownerId }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ships", newShipDto);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Fail($"POST /api/ships returned {response.StatusCode}. Content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdShip = await response.Content.ReadFromJsonAsync<ShipDetailsDto>();
            Assert.NotNull(createdShip);
            Assert.Equal(newShipDto.Name, createdShip.Name);
            Assert.Equal(newShipDto.ImoNumber, createdShip.ImoNumber);
            Assert.True(createdShip.Id > 0);
            Assert.NotNull(createdShip.Owners);
            Assert.Single(createdShip.Owners); // Should have 1 owner
            Assert.Equal(ownerId, createdShip.Owners.First().Id);
            Assert.NotNull(response.Headers.Location);

            var expectedPath = $"/api/ships/{createdShip.Id}";
            var actualPath = response.Headers.Location?.AbsolutePath;
            Assert.True(string.Equals(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase),
                        $"Expected location '{expectedPath}' (case-insensitive), but got '{actualPath}'");


            // Verify creation via GET
            var getResponse = await _client.GetAsync(response.Headers.Location);
            getResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        }

        [Fact]
        public async Task CreateShip_DuplicateImoNumber_ReturnsBadRequest()
        {
            // Arrange
            var existingImoNumber = "1111111"; // From seeded data
            int ownerId;
            using (var scope = CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var owner = await context.Owners.FirstAsync();
                ownerId = owner.Id;
            }

            var newShipDto = new CreateShipDto
            {
                Name = "Duplicate IMO Ship",
                ImoNumber = existingImoNumber, // Use existing IMO
                Type = "Duplicate",
                Tonnage = 100,
                OwnerIds = new List<int> { ownerId }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ships", newShipDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains($"IMO Number {existingImoNumber} already exists", error);
        }

        [Fact]
        public async Task CreateShip_NonExistentOwnerId_ReturnsBadRequest()
        {
            // Arrange
            var nonExistentOwnerId = -999;
            var newShipDto = new CreateShipDto
            {
                Name = "Ship With Invalid Owner",
                ImoNumber = GenerateUniqueImo(), // Unique IMO
                Type = "Invalid",
                Tonnage = 200,
                OwnerIds = new List<int> { nonExistentOwnerId } // Use non-existent owner
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ships", newShipDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("owner", errorContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("exist", errorContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateShip_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var invalidShipDto = new CreateShipDto
            {
                Name = "", // Invalid Name
                ImoNumber = "123", // Invalid IMO
                Type = "Test",
                Tonnage = -100, // Invalid Tonnage
                OwnerIds = new List<int>() // Invalid - requires at least one owner
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ships", invalidShipDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task UpdateShip_ValidData_ReturnsNoContent()
        {
            // Arrange
            int shipIdToUpdate;
            using (var scope = CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var shipToUpdate = await context.Ships.FirstOrDefaultAsync(s => s.ImoNumber == "1111111"); // Get ship from seed
                Assert.NotNull(shipToUpdate); // Ensure seed data exists
                shipIdToUpdate = shipToUpdate.Id;
            }

            var updateDto = new UpdateShipDto
            {
                Name = "Updated Test Ship 1",
                ImoNumber = "1111111",
                Type = "Updated Cargo",
                Tonnage = 15000
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/ships/{shipIdToUpdate}", updateDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify update via GET
            var getResponse = await _client.GetAsync($"/api/ships/{shipIdToUpdate}");
            getResponse.EnsureSuccessStatusCode();
            var updatedShip = await getResponse.Content.ReadFromJsonAsync<ShipDetailsDto>();
            Assert.NotNull(updatedShip);
            Assert.Equal(updateDto.Name, updatedShip.Name);
            Assert.Equal(updateDto.Type, updatedShip.Type);
            Assert.Equal(updateDto.Tonnage, updatedShip.Tonnage);
        }

        [Fact]
        public async Task UpdateShip_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = -999;
            
            // make sure the DTO being sent is valid!!!
            var updateDto = new UpdateShipDto
            {
                Name = "Update Non Existent",
                ImoNumber = GenerateUniqueImo(),
                Type = "Ghost",
                Tonnage = 100
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/ships/{nonExistentId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateShip_DuplicateImoNumber_ReturnsBadRequest()
        {
            // Arrange
            int shipIdToUpdate;
            string existingImoOfOtherShip;
            using (var scope = CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var shipToUpdate = await context.Ships.FirstAsync(s => s.ImoNumber == "1111111");
                var otherShip = await context.Ships.FirstAsync(s => s.ImoNumber == "2222222");
                Assert.NotNull(shipToUpdate);
                Assert.NotNull(otherShip);
                shipIdToUpdate = shipToUpdate.Id;
                existingImoOfOtherShip = otherShip.ImoNumber;
            }

            var updateDto = new UpdateShipDto
            {
                Name = "Trying Duplicate IMO",
                ImoNumber = existingImoOfOtherShip, // Attempt to use Ship 2's IMO
                Type = "Cargo",
                Tonnage = 10000
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/ships/{shipIdToUpdate}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Another ship with IMO Number {existingImoOfOtherShip} already exists", error);
        }

        [Fact]
        public async Task UpdateShip_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            int shipIdToUpdate;
            using (var scope = CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var shipToUpdate = await context.Ships.FirstAsync();
                Assert.NotNull(shipToUpdate);
                shipIdToUpdate = shipToUpdate.Id;
            }


            var invalidUpdateDto = new UpdateShipDto
            {
                // invalid formats
                Name = "",
                ImoNumber = "123",
                Type = "Test",
                Tonnage = 0
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/ships/{shipIdToUpdate}", invalidUpdateDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task DeleteShip_ExistingId_ReturnsNoContentAndRemovesShipAndLinks()
        {
            // Arrange: Create a dedicated ship and link to delete
            int shipIdToDelete;

            using (var setupScope = CreateScope())
            {
                var setupContext = setupScope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var owner = await setupContext.Owners.FirstAsync();
                // unique IMO
                var uniqueImo = GenerateUniqueImo();
                while (await setupContext.Ships.AnyAsync(s => s.ImoNumber == uniqueImo))
                {
                    uniqueImo = GenerateUniqueImo();
                }
                var ship = new Ship { Name = $"ShipToDelete {Guid.NewGuid()}", ImoNumber = uniqueImo, Type = "TestDeleteShip", Tonnage = 1 };
                setupContext.Ships.Add(ship);
                await setupContext.SaveChangesAsync();
                shipIdToDelete = ship.Id;

                setupContext.ShipOwners.Add(new ShipOwner { OwnerId = owner.Id, ShipId = shipIdToDelete });
                await setupContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.DeleteAsync($"/api/ships/{shipIdToDelete}");

            // Assert - Deletion successful
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Assert - Ship is deleted
            var getShipResponse = await _client.GetAsync($"/api/ships/{shipIdToDelete}");
            Assert.Equal(HttpStatusCode.NotFound, getShipResponse.StatusCode);

            // Assert - ShipOwner links are deleted
            using (var verifyScope = CreateScope())
            {
                var verifyContext = verifyScope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var linkExists = await verifyContext.ShipOwners.AnyAsync(so => so.ShipId == shipIdToDelete);
                Assert.False(linkExists, "ShipOwner links for the deleted ship should be removed by cascade delete.");
            }
        }

        [Fact]
        public async Task DeleteShip_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = -999;

            // Act
            var response = await _client.DeleteAsync($"/api/ships/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
