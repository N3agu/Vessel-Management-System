using VesselManagementApi.Data;
using VesselManagementApi.DTOs;
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
    public class OwnersControllerTests
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

        private async Task SeedDataAsync(VesselManagementDbContext context)
        {
            var owner1 = new Owner { Id = 1, Name = "Test Owner 1" };
            var owner2 = new Owner { Id = 2, Name = "Test Owner 2" };
            context.Owners.AddRange(owner1, owner2);

            var ship1 = new Ship { Id = 1, Name = "Test Ship 1", ImoNumber = "1111111", Type = "Cargo", Tonnage = 10000 };
            var ship2 = new Ship { Id = 2, Name = "Test Ship 2", ImoNumber = "2222222", Type = "Tanker", Tonnage = 20000 };
            context.Ships.AddRange(ship1, ship2);

            context.ShipOwners.AddRange(
                new ShipOwner { OwnerId = 1, ShipId = 1 },
                new ShipOwner { OwnerId = 2, ShipId = 1 },
                new ShipOwner { OwnerId = 2, ShipId = 2 }
            );

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetOwners_ReturnsOkResult_WithListOfOwners()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                await SeedDataAsync(context);

                var Interface = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<OwnerService>.Instance;
                var service = new OwnerService(Interface, mapper, logger);
                var controllerLogger = NullLogger<OwnersController>.Instance;
                var controller = new OwnersController(service, controllerLogger);

                // Act
                var actionResult = await controller.GetOwners();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var owners = Assert.IsAssignableFrom<IEnumerable<OwnerDto>>(okResult.Value);
                Assert.Equal(2, owners.Count());
                Assert.Contains(owners, o => o.Name == "Test Owner 1");
                Assert.Contains(owners, o => o.Name == "Test Owner 2");
            }
        }

        [Fact]
        public async Task GetOwner_ExistingId_ReturnsOkResult_WithOwner()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                await SeedDataAsync(context);
                var existingOwnerId = 1;

                var Interface = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<OwnerService>.Instance;
                var service = new OwnerService(Interface, mapper, logger);
                var controllerLogger = NullLogger<OwnersController>.Instance;
                var controller = new OwnersController(service, controllerLogger);

                // Act
                var actionResult = await controller.GetOwner(existingOwnerId);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var owner = Assert.IsType<OwnerDto>(okResult.Value);
                Assert.Equal(existingOwnerId, owner.Id);
                Assert.Equal("Test Owner 1", owner.Name);
            }
        }

        [Fact]
        public async Task GetOwner_NonExistentId_ReturnsNotFoundResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var nonExistentId = 999;

                var Interface = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<OwnerService>.Instance;
                var service = new OwnerService(Interface, mapper, logger);
                var controllerLogger = NullLogger<OwnersController>.Instance;
                var controller = new OwnersController(service, controllerLogger);

                // Act
                var actionResult = await controller.GetOwner(nonExistentId);

                // Assert
                Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            }
        }

        [Fact]
        public async Task CreateOwner_ValidData_ReturnsCreatedAtActionResult_WithCreatedOwner()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var newOwnerDto = new CreateOwnerDto { Name = "New Create Owner" };

                var Interface = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<OwnerService>.Instance;
                var service = new OwnerService(Interface, mapper, logger);
                var controllerLogger = NullLogger<OwnersController>.Instance;
                var controller = new OwnersController(service, controllerLogger);

                // Act
                var actionResult = await controller.CreateOwner(newOwnerDto);

                // Assert
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var createdOwner = Assert.IsType<OwnerDto>(createdAtActionResult.Value);
                Assert.Equal(newOwnerDto.Name, createdOwner.Name);
                Assert.True(createdOwner.Id > 0); // InMemory DB assigns IDs
                Assert.Equal(nameof(controller.GetOwner), createdAtActionResult.ActionName);
                Assert.Equal(createdOwner.Id, createdAtActionResult.RouteValues["id"]);

                // Verify it was actually added to the context
                var ownerInDb = await context.Owners.FindAsync(createdOwner.Id);
                Assert.NotNull(ownerInDb);
                Assert.Equal(newOwnerDto.Name, ownerInDb.Name);
            }
        }

        [Fact]
        public async Task CreateOwner_InvalidData_ReturnsBadRequestResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var invalidOwnerDto = new CreateOwnerDto { Name = "" };

                var Interface = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<OwnerService>.Instance;
                var service = new OwnerService(Interface, mapper, logger);
                var controllerLogger = NullLogger<OwnersController>.Instance;
                var controller = new OwnersController(service, controllerLogger);
                controller.ModelState.AddModelError("Name", "The Name field is required.");

                // Act
                var actionResult = await controller.CreateOwner(invalidOwnerDto);

                // Assert
                Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            }
        }


        [Fact]
        public async Task DeleteOwner_ExistingId_ReturnsNoContentResult_AndRemovesOwner()
        {
            // Arrange
            var options = CreateNewContextOptions();
            int ownerIdToDelete;
            int shipIdOwned;
            await using (var context = new VesselManagementDbContext(options))
            {
                var owner = new Owner { Name = "OwnerToDelete" };
                context.Owners.Add(owner);
                await context.SaveChangesAsync();
                ownerIdToDelete = owner.Id;

                var ship = new Ship { Name = "ShipOwned", ImoNumber = "3333333", Type = "Test", Tonnage = 1 };
                context.Ships.Add(ship);
                await context.SaveChangesAsync();
                shipIdOwned = ship.Id;

                context.ShipOwners.Add(new ShipOwner { OwnerId = ownerIdToDelete, ShipId = shipIdOwned });
                await context.SaveChangesAsync();

                var Interface = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<OwnerService>.Instance;
                var service = new OwnerService(Interface, mapper, logger);
                var controllerLogger = NullLogger<OwnersController>.Instance;
                var controller = new OwnersController(service, controllerLogger);

                // Act
                var actionResult = await controller.DeleteOwner(ownerIdToDelete);

                // Assert
                Assert.IsType<NoContentResult>(actionResult);

                // Verify deletion from context
                var deletedOwner = await context.Owners.FindAsync(ownerIdToDelete);
                Assert.Null(deletedOwner);
                var deletedLink = await context.ShipOwners.FirstOrDefaultAsync(so => so.OwnerId == ownerIdToDelete);
                Assert.Null(deletedLink); // Cascade delete should remove link
                var existingShip = await context.Ships.FindAsync(shipIdOwned);
                Assert.NotNull(existingShip); // Ship should remain
            }
        }

        [Fact]
        public async Task DeleteOwner_NonExistentId_ReturnsNotFoundResult()
        {
            // Arrange
            var options = CreateNewContextOptions();
            await using (var context = new VesselManagementDbContext(options))
            {
                var nonExistentId = 999;

                var Interface = new OwnerInterface(context);
                var mapper = GetMapper();
                var logger = NullLogger<OwnerService>.Instance;
                var service = new OwnerService(Interface, mapper, logger);
                var controllerLogger = NullLogger<OwnersController>.Instance;
                var controller = new OwnersController(service, controllerLogger);

                // Act
                var actionResult = await controller.DeleteOwner(nonExistentId);

                // Assert
                Assert.IsType<NotFoundObjectResult>(actionResult);
            }
        }
    }
}