using Microsoft.EntityFrameworkCore;
using VesselManagementApi.Data;
using VesselManagementApi.Models;

namespace VesselManagementApi.Interfaces
{
    public class OwnerRepository : IOwnerInterface
    {
        private readonly VesselManagementDbContext _context;

        public OwnerRepository(VesselManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Owner> AddAsync(Owner owner)
        {
            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();
            return owner;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Deleting an Owner will automatically delete related ShipOwner entries.
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
            {
                return false; // not found
            }

            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Owners.AnyAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Owner>> GetAllAsync()
        {
            return await _context.Owners.AsNoTracking().ToListAsync(); // AsNoTracking for read-only queries
        }

        public async Task<Owner?> GetByIdAsync(int id)
        {
            return await _context.Owners.FindAsync(id);
        }

        public async Task<IEnumerable<Owner>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Owners.Where(o => ids.Contains(o.Id)).ToListAsync();
        }
    }
}
