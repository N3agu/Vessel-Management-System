using VesselManagementApi.DTOs;
using VesselManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using VesselManagementApi.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using VesselManagementApi.Controllers;
using VesselManagementApi.Services;
using VesselManagementApi.Interfaces;
using VesselManagementApi.Mapping;

namespace VesselManagementApi.Tests.Controllers
{
    public class ShipsControllerTests
    {
        private IMapper _mapper;

        // create unique DbContext options for each test
        private DbContextOptions<VesselManagementDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<VesselManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private IMapper GetMapper()
        {
            if (_mapper == null)
            {
                var mappingConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new MappingProfile());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;
            }
            return _mapper;
        }

        private async Task<(int owner1Id, int owner2Id, int ship1Id, int ship2Id)> SeedDataAsync(VesselManagementDbContext context)
        {
            var owner1 = new Owner { Name = "Seed Owner 1" };
            var owner2 = new Owner { Name = "Seed Owner 2" };
            context.Owners.AddRange(owner1, owner2);
            await context.SaveChangesAsync(); // Save to get IDs

            var ship1 = new Ship { Name = "Seed Ship 1", ImoNumber = "1111111", Type = "Cargo", Tonnage = 10000 };
            var ship2 = new Ship { Name = "Seed Ship 2", ImoNumber = "2222222", Type = "Tanker", Tonnage = 20000 };
            context.Ships.AddRange(ship1, ship2);
            await context.SaveChangesAsync(); // Save to get IDs

            context.ShipOwners.AddRange(
                new ShipOwner { OwnerId = owner1.Id, ShipId = ship1.Id },
                new ShipOwner { OwnerId = owner2.Id, ShipId = ship1.Id },
                new ShipOwner { OwnerId = owner2.Id, ShipId = ship2.Id }
            );

            await context.SaveChangesAsync();
            return (owner1.Id, owner2.Id, ship1.Id, ship2.Id);
        }

        // generate 7 digit IMO
        private string GenerateUniqueImo()
        {
            long ticks = DateTime.UtcNow.Ticks;
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 4);
            string combined = $"{ticks}{guidPart}";
            string potentialImo = new string(combined.Where(char.IsDigit).Reverse().Take(7).Reverse().ToArray());
            return potentialImo.PadLeft(7, '0');
        }

        [Fact]
        public async Task GetShips_ReturnsOkResult_WithListOfShips()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                await SeedDataAsync(context);

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.GetShips();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var ships = Assert.IsAssignableFrom<IEnumerable<ShipDto>>(okResult.Value);
                Assert.Equal(2, ships.Count());
                Assert.Contains(ships, s => s.ImoNumber == "1111111");
                Assert.Contains(ships, s => s.ImoNumber == "2222222");
            }
        }

        [Fact]
        public async Task GetShipDetails_ExistingId_ReturnsOkResult_WithShipAndOwners()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var (owner1Id, owner2Id, ship1Id, _) = await SeedDataAsync(context);

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.GetShipDetails(ship1Id);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var shipDetails = Assert.IsType<ShipDetailsDto>(okResult.Value);
                Assert.Equal(ship1Id, shipDetails.Id);
                Assert.Equal("1111111", shipDetails.ImoNumber);
                Assert.NotNull(shipDetails.Owners);
                Assert.Equal(2, shipDetails.Owners.Count);
                Assert.Contains(shipDetails.Owners, o => o.Id == owner1Id);
                Assert.Contains(shipDetails.Owners, o => o.Id == owner2Id);
            }
        }

        [Fact]
        public async Task GetShipDetails_NonExistentId_ReturnsNotFoundResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var nonExistentId = 999;
                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.GetShipDetails(nonExistentId);

                // Assert
                Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            }
        }


        [Fact]
        public async Task CreateShip_ValidData_ReturnsCreatedAtActionResult_WithShipDetails()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var owner = new Owner { Name = "Test Owner" };
                context.Owners.Add(owner);
                await context.SaveChangesAsync();
                var ownerId = owner.Id;

                var uniqueImo = GenerateUniqueImo();
                var newShipDto = new CreateShipDto
                {
                    Name = "Create Ship Test",
                    ImoNumber = uniqueImo,
                    Type = "TestCreate",
                    Tonnage = 1234,
                    OwnerIds = new List<int> { ownerId }
                };

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.CreateShip(newShipDto);

                // Assert
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var createdShip = Assert.IsType<ShipDetailsDto>(createdAtActionResult.Value);
                Assert.Equal(newShipDto.Name, createdShip.Name);
                Assert.Equal(uniqueImo, createdShip.ImoNumber);
                Assert.True(createdShip.Id > 0);
                Assert.NotNull(createdShip.Owners);
                Assert.Single(createdShip.Owners);
                Assert.Equal(ownerId, createdShip.Owners.First().Id);
                Assert.Equal(nameof(controller.GetShipDetails), createdAtActionResult.ActionName);
                Assert.Equal(createdShip.Id, createdAtActionResult.RouteValues["id"]);

                // Verify
                var shipInDb = await context.Ships.Include(s => s.ShipOwners).FirstOrDefaultAsync(s => s.Id == createdShip.Id);
                Assert.NotNull(shipInDb);
                Assert.Single(shipInDb.ShipOwners);
                Assert.Equal(ownerId, shipInDb.ShipOwners.First().OwnerId);
            }
        }

        [Fact]
        public async Task CreateShip_DuplicateImoNumber_ReturnsBadRequestResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var (owner1Id, _, _, _) = await SeedDataAsync(context);
                var existingImo = "1111111";

                var duplicateShipDto = new CreateShipDto
                {
                    Name = "Duplicate Test",
                    ImoNumber = existingImo,
                    Type = "Duplicate",
                    Tonnage = 100,
                    OwnerIds = new List<int> { owner1Id }
                };

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.CreateShip(duplicateShipDto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
                Assert.Contains(existingImo, badRequestResult.Value.ToString());
            }
        }

        [Fact]
        public async Task CreateShip_NonExistentOwnerId_ReturnsBadRequestResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var nonExistentOwnerId = 999;
                var newShipDto = new CreateShipDto
                {
                    Name = "Ship With Invalid Owner",
                    ImoNumber = GenerateUniqueImo(),
                    Type = "Invalid",
                    Tonnage = 200,
                    OwnerIds = new List<int> { nonExistentOwnerId }
                };

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.CreateShip(newShipDto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
                Assert.Contains("owner", badRequestResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
                Assert.Contains("exist", badRequestResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task UpdateShip_ValidData_ReturnsNoContentResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var (_, _, ship1Id, _) = await SeedDataAsync(context);

                var updateDto = new UpdateShipDto
                {
                    Name = "Updated Name",
                    ImoNumber = "1111111",
                    Type = "Updated Type",
                    Tonnage = 9999
                };

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.UpdateShip(ship1Id, updateDto);

                // Assert
                Assert.IsType<NoContentResult>(actionResult);

                // Verify
                var updatedShip = await context.Ships.FindAsync(ship1Id);
                Assert.NotNull(updatedShip);
                Assert.Equal(updateDto.Name, updatedShip.Name);
                Assert.Equal(updateDto.Type, updatedShip.Type);
                Assert.Equal(updateDto.Tonnage, updatedShip.Tonnage);
            }
        }

        [Fact]
        public async Task UpdateShip_NonExistentId_ReturnsNotFoundResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var nonExistentId = 999;
                var updateDto = new UpdateShipDto { Name = "UpdateFail", ImoNumber = "8888888", Type = "Fail", Tonnage = 100 };

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.UpdateShip(nonExistentId, updateDto);

                // Assert
                Assert.IsType<NotFoundObjectResult>(actionResult);
            }
        }

        [Fact]
        public async Task DeleteShip_ExistingId_ReturnsNoContentResult_AndRemovesShip()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var (_, _, ship1Id, _) = await SeedDataAsync(context); // Ship 1 has owners

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.DeleteShip(ship1Id);

                // Assert
                Assert.IsType<NoContentResult>(actionResult);

                // Verify
                var deletedShip = await context.Ships.FindAsync(ship1Id);
                Assert.Null(deletedShip);
                var deletedLinks = await context.ShipOwners.Where(so => so.ShipId == ship1Id).ToListAsync();
                Assert.Empty(deletedLinks);
            }
        }

        [Fact]
        public async Task DeleteShip_NonExistentId_ReturnsNotFoundResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var nonExistentId = 999;

                var shipRepo = new ShipInterface(context);
                var ownerRepo = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<ShipService>.Instance;
                var service = new ShipService(shipRepo, ownerRepo, mapper, logger);
                var controllerLogger = NullLogger<ShipsController>.Instance;
                var controller = new ShipsController(service, controllerLogger);

                // Act
                var actionResult = await controller.DeleteShip(nonExistentId);

                // Assert
                Assert.IsType<NotFoundObjectResult>(actionResult);
            }
        }
    }
}