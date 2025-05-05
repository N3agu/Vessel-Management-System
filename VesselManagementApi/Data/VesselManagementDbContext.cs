using Microsoft.EntityFrameworkCore;
using VesselManagementApi.Models;

namespace VesselManagementApi.Data
{
    public class VesselManagementDbContext : DbContext
    {
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Ship> Ships { get; set; }
        public DbSet<ShipOwner> ShipOwners { get; set; } // The junction table

        /// <summary>
        /// Constructor used by dependency injection.
        /// </summary>
        /// <param name="options">Database context options (e.g., connection string).</param>
        public VesselManagementDbContext(DbContextOptions<VesselManagementDbContext> options) : base(options) { }

        /// <summary>
        /// Configures the database model (relationships, keys, constraints, etc.).
        /// </summary>
        /// <param name="modelBuilder">Provides an API for configuring the model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // primary key - OwnerId, ShipId
            modelBuilder.Entity<ShipOwner>()
                .HasKey(so => new { so.OwnerId, so.ShipId });

            // Create relationship from ShipOwner to Owner (One-to-Many)
            modelBuilder.Entity<ShipOwner>()
                .HasOne(so => so.Owner)
                .WithMany(o => o.ShipOwners)
                .HasForeignKey(so => so.OwnerId)
                .OnDelete(DeleteBehavior.Cascade); // If an Owner is deleted, delete their ShipOwner entries

            // Create the relationship from ShipOwner to Ship (One-to-Many)
            modelBuilder.Entity<ShipOwner>()
                .HasOne(so => so.Ship)
                .WithMany(s => s.ShipOwners)
                .HasForeignKey(so => so.ShipId)
                .OnDelete(DeleteBehavior.Cascade); // If a Ship is deleted, delete its ShipOwner entries

            // Ensure IMO Number is unique
            modelBuilder.Entity<Ship>()
                .HasIndex(s => s.ImoNumber)
                .IsUnique();

            // Seed data for testing
            modelBuilder.Entity<Owner>().HasData(
                new Owner { Id = 1, Name = "Example Cruises" },
                new Owner { Id = 2, Name = "Maritime Inc." }
            );
            modelBuilder.Entity<Ship>().HasData(
                new Ship { Id = 1, Name = "Ocean Explorer", ImoNumber = "1234567", Type = "Cruise", Tonnage = 5000 }
            );
            modelBuilder.Entity<ShipOwner>().HasData(
                new ShipOwner { OwnerId = 1, ShipId = 1 }
            );
        }
    }
}
