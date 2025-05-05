using VesselManagementApi.Models;

namespace VesselManagementApi.Interfaces
{
    public interface IOwnerInterface
    {
        Task<IEnumerable<Owner>> GetAllAsync();
        Task<Owner?> GetByIdAsync(int id);
        Task<Owner> AddAsync(Owner owner);
        Task<bool> DeleteAsync(int id); // return true if deleted, false if not found
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Owner>> GetByIdsAsync(IEnumerable<int> ids);
    }
}
