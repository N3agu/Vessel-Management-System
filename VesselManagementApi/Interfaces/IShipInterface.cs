using VesselManagementApi.Models;

namespace VesselManagementApi.Interfaces
{
    public interface IShipInterface
    {
        Task<IEnumerable<Ship>> GetAllAsync();
        Task<Ship?> GetByIdAsync(int id);
        Task<Ship?> GetDetailsByIdAsync(int id);
        Task<Ship> AddAsync(Ship ship);
        Task AddShipWithOwnerLinksAsync(Ship ship, IEnumerable<int> ownerIds);
        Task UpdateAsync(Ship ship);
        Task<bool> DeleteAsync(int id);
        Task<bool> ImoNumberExistsAsync(string imoNumber, int? currentShipId = null);
    }
}
