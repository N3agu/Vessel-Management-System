using Microsoft.EntityFrameworkCore;
using VesselManagementApi.Data;
using VesselManagementApi.Models;

namespace VesselManagementApi.Interfaces
{
    public class ShipInterface : IShipInterface
    {
        private readonly VesselManagementDbContext _context;

        public ShipInterface(VesselManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Ship> AddAsync(Ship ship)
        {
            _context.Ships.Add(ship);
            await _context.SaveChangesAsync();
            return ship;
        }

        public async Task AddShipWithOwnerLinksAsync(Ship ship, IEnumerable<int> ownerIds)
        {
            // add thr ship itself
            _context.Ships.Add(ship);
            await _context.SaveChangesAsync();

            if (ownerIds != null && ownerIds.Any())
            {
                foreach (var ownerId in ownerIds)
                {
                    // check if owner exists!!
                    bool ownerExists = await _context.Owners.AnyAsync(o => o.Id == ownerId);
                    if (ownerExists) {
                        ShipOwner? shipOwner = new ShipOwner { ShipId = ship.Id, OwnerId = ownerId };
                        _context.ShipOwners.Add(shipOwner);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var ship = await _context.Ships.FindAsync(id);
            if (ship == null)
            {
                return false;
            }

            _context.Ships.Remove(ship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Ship>> GetAllAsync()
        {
            return await _context.Ships.AsNoTracking().ToListAsync();
        }

        public async Task<Ship?> GetByIdAsync(int id)
        {
            return await _context.Ships.FindAsync(id);
        }

        public async Task<Ship?> GetDetailsByIdAsync(int id)
        {
            return await _context.Ships.Include(s => s.ShipOwners)
                                 .ThenInclude(so => so.Owner)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> ImoNumberExistsAsync(string imoNumber, int? currentShipId = null)
        {
            // check if any other ship has the same IMO
            if (currentShipId.HasValue) {
                return await _context.Ships.AnyAsync(s => s.ImoNumber == imoNumber && s.Id != currentShipId.Value);
            } else {
                return await _context.Ships.AnyAsync(s => s.ImoNumber == imoNumber);
            }
        }

        public async Task UpdateAsync(Ship ship)
        {
            // Mark the entity as modified. EF Core tracks changes.
            _context.Entry(ship).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
