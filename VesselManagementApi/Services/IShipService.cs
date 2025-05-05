using VesselManagementApi.DTOs;

namespace VesselManagementApi.Services
{
    public interface IShipService
    {
        Task<IEnumerable<ShipDto>> GetAllShipsAsync();
        Task<ShipDetailsDto?> GetShipDetailsAsync(int id);
        Task<(ShipDetailsDto? Ship, string? ErrorMessage)> CreateShipAsync(CreateShipDto createShipDto); // return tuple for potential errors
        Task<(bool Success, string? ErrorMessage)> UpdateShipAsync(int id, UpdateShipDto updateShipDto);
        Task<bool> DeleteShipAsync(int id);
    }
}
