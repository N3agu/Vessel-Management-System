using VesselManagementApi.DTOs;

namespace VesselManagementApi.Services
{
    public interface IOwnerService
    {
        Task<IEnumerable<OwnerDto>> GetAllOwnersAsync();
        Task<OwnerDto?> GetOwnerByIdAsync(int id);
        Task<OwnerDto> CreateOwnerAsync(CreateOwnerDto createOwnerDto);
        Task<bool> DeleteOwnerAsync(int id);
    }
}
