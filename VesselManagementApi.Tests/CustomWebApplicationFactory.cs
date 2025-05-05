using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VesselManagementApi.Models;
using VesselManagementApi.Data;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;


namespace VesselManagementApi.Tests
{
    // Tests will run against the database configured in appsettings.json
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private static bool _databaseInitialized = false;
        private static readonly object _lock = new object();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Console.WriteLine("Using CustomWebApplicationFactory - Running against configured database.");

            builder.ConfigureTestServices(services =>
            {
                lock (_lock)
                {
                    if (_databaseInitialized)
                    {
                        return;
                    }

                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<VesselManagementDbContext>();
                        var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

                        logger.LogInformation("Attempting to initialize the test database (Cleanup + Seed)...");


                        logger.LogInformation("Database checked/created.");

                        try
                        {
                            CleanupDatabase(db, logger);

                            logger.LogInformation("Seeding test database.");
                            SeedData(db);
                            logger.LogInformation("Seeding complete.");

                            _databaseInitialized = true; // Mark as initialized
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred cleaning/seeding the database.");
                        }
                    }
                }
            });

            builder.UseEnvironment("Development");
        }

        private static void SeedData(VesselManagementDbContext context)
        {
            Console.WriteLine("Seeding initial data...");
            // Add Owners
            var owner1 = new Owner { Name = "Test Owner 1" }; // Let DB generate ID
            var owner2 = new Owner { Name = "Test Owner 2" };
            context.Owners.AddRange(owner1, owner2);
            context.SaveChanges();

            // Add Ships
            var ship1 = new Ship { Name = "Test Ship 1", ImoNumber = "1111111", Type = "Cargo", Tonnage = 10000 };
            var ship2 = new Ship { Name = "Test Ship 2", ImoNumber = "2222222", Type = "Tanker", Tonnage = 20000 };
            var shipForDeleteTest = new Ship { Name = "Ship For Delete Test", ImoNumber = "9999999", Type = "Delete", Tonnage = 500 };
            context.Ships.AddRange(ship1, ship2, shipForDeleteTest);
            context.SaveChanges();

            // Add ShipOwners (links)
            context.ShipOwners.AddRange(
                new ShipOwner { OwnerId = owner1.Id, ShipId = ship1.Id },
                new ShipOwner { OwnerId = owner2.Id, ShipId = ship1.Id }, // Ship 1 owned by Owner 1 & 2
                new ShipOwner { OwnerId = owner2.Id, ShipId = ship2.Id }  // Ship 2 owned by Owner 2
            );
            context.SaveChanges();
            Console.WriteLine("Initial data seeded.");
        }

        private static void CleanupDatabase(VesselManagementDbContext context, ILogger logger)
        {
            logger.LogInformation("Cleaning up test database...");

            context.ShipOwners.ExecuteDelete();
            context.Ships.ExecuteDelete();
            context.Owners.ExecuteDelete();

            logger.LogInformation("ExecuteDelete cleanup complete.");
        }
    }
}