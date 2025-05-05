using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using System.Net;
using VesselManagementApi.Data;
using VesselManagementApi.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using VesselManagementApi.Models;

namespace VesselManagementApi.Tests.Controllers
{
    public class OwnersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public OwnersControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private VesselManagementDbContext GetDbContext()
        {
            var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
        }


        [Fact]
        public async Task GetOwners_ReturnsOkAndListOfOwners()
        {
            // Act
            var response = await _client.GetAsync("/api/owners");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var owners = await response.Content.ReadFromJsonAsync<List<OwnerDto>>();
            Assert.NotNull(owners);
            Assert.True(owners.Count >= 2);
            Assert.Contains(owners, o => o.Name == "Test Owner 1");
            Assert.Contains(owners, o => o.Name == "Test Owner 2");
        }

        [Fact]
        public async Task GetOwner_ExistingId_ReturnsOkAndOwner()
        {
            // Arrange
            int existingOwnerId;
            using (var context = GetDbContext())
            {
                var seededOwner = await context.Owners.FirstOrDefaultAsync(o => o.Name == "Test Owner 1");
                Assert.NotNull(seededOwner);
                existingOwnerId = seededOwner.Id;
            }


            // Act
            var response = await _client.GetAsync($"/api/owners/{existingOwnerId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var owner = await response.Content.ReadFromJsonAsync<OwnerDto>();
            Assert.NotNull(owner);
            Assert.Equal(existingOwnerId, owner.Id);
            Assert.Equal("Test Owner 1", owner.Name);
        }

        [Fact]
        public async Task GetOwner_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = -999;

            // Act
            var response = await _client.GetAsync($"/api/owners/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateOwner_ValidData_ReturnsCreatedAndOwner()
        {
            // Arrange
            var newOwnerDto = new CreateOwnerDto { Name = $"New Test Owner {Guid.NewGuid()}" }; // Unique name

            // Act
            var response = await _client.PostAsJsonAsync("/api/owners", newOwnerDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdOwner = await response.Content.ReadFromJsonAsync<OwnerDto>();
            Assert.NotNull(createdOwner);
            Assert.Equal(newOwnerDto.Name, createdOwner.Name);
            Assert.True(createdOwner.Id > 0);
            Assert.NotNull(response.Headers.Location);

            var expectedPath = $"/api/owners/{createdOwner.Id}"; // Assuming lowercase 'owners'
            var actualPath = response.Headers.Location?.AbsolutePath;
            Assert.True(string.Equals(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase),
                        $"Expected location '{expectedPath}' (case-insensitive), but got '{actualPath}'");

            // Verify creation via GET
            var getResponse = await _client.GetAsync(response.Headers.Location);
            getResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        }

        [Fact]
        public async Task CreateOwner_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidOwnerDto = new CreateOwnerDto { Name = "" }; // Invalid name

            // Act
            var response = await _client.PostAsJsonAsync("/api/owners", invalidOwnerDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteOwner_ExistingId_ReturnsNoContentAndRemovesOwnerAndLinks()
        {
            // Arrange
            var ownerNameToDelete = $"OwnerToDelete {Guid.NewGuid()}";
            var shipNameToDelete = $"ShipOwnedByDeletedOwner {Guid.NewGuid()}";
            int ownerIdToDelete;
            int shipIdOwned;

            using (var setupScope = _factory.Services.CreateScope())
            {
                var setupContext = setupScope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var owner = new Owner { Name = ownerNameToDelete };
                setupContext.Owners.Add(owner);
                await setupContext.SaveChangesAsync();
                ownerIdToDelete = owner.Id;

                var uniqueImo = Guid.NewGuid().ToString().Substring(0, 7);
                while (await setupContext.Ships.AnyAsync(s => s.ImoNumber == uniqueImo))
                {
                    uniqueImo = Guid.NewGuid().ToString().Substring(0, 7);
                }
                var ship = new Ship { Name = shipNameToDelete, ImoNumber = uniqueImo, Type = "TestDelete", Tonnage = 1 };
                setupContext.Ships.Add(ship);
                await setupContext.SaveChangesAsync();
                shipIdOwned = ship.Id;

                setupContext.ShipOwners.Add(new ShipOwner { OwnerId = ownerIdToDelete, ShipId = shipIdOwned });
                await setupContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.DeleteAsync($"/api/owners/{ownerIdToDelete}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Assert - Owner is deleted
            var getOwnerResponse = await _client.GetAsync($"/api/owners/{ownerIdToDelete}");
            Assert.Equal(HttpStatusCode.NotFound, getOwnerResponse.StatusCode);

            // Assert - ShipOwner links are deleted
            using (var verifyScope = _factory.Services.CreateScope())
            {
                var verifyContext = verifyScope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
                var linkExists = await verifyContext.ShipOwners.AnyAsync(so => so.OwnerId == ownerIdToDelete);
                Assert.False(linkExists, "ShipOwner links for the deleted owner should be removed by cascade delete.");
                // Check that the ship still exists
                var shipExists = await verifyContext.Ships.AnyAsync(s => s.Id == shipIdOwned);
                Assert.True(shipExists, "Deleting an owner should not delete their ships.");
            }
        }

        [Fact]
        public async Task DeleteOwner_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = -999;

            // Act
            var response = await _client.DeleteAsync($"/api/owners/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}