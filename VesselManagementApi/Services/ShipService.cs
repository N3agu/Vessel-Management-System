using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VesselManagementApi.DTOs;
using VesselManagementApi.Interfaces;
using VesselManagementApi.Models;

namespace VesselManagementApi.Services
{
    public class ShipService : IShipService
    {
        private readonly IShipInterface _shipInterface;
        private readonly IOwnerInterface _ownerInterface;
        private readonly IMapper _mapper;
        private readonly ILogger<ShipService> _logger;

        public ShipService(IShipInterface shipInterface, IOwnerInterface ownerInterface, IMapper mapper, ILogger<ShipService> logger)
        {
            _shipInterface = shipInterface;
            _ownerInterface = ownerInterface;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(ShipDetailsDto? Ship, string? ErrorMessage)> CreateShipAsync(CreateShipDto createShipDto)
        {
            // check imo
            if (await _shipInterface.ImoNumberExistsAsync(createShipDto.ImoNumber))
            {
                return (null, $"Ship with IMO Number {createShipDto.ImoNumber} already exists.");
            }

            // check owner id
            if (createShipDto.OwnerIds == null || !createShipDto.OwnerIds.Any())
            {
                return (null, "At least one owner ID must be provided.");
            }
            var existingOwners = await _ownerInterface.GetByIdsAsync(createShipDto.OwnerIds);
            if (existingOwners.Count() != createShipDto.OwnerIds.Distinct().Count()) // Check if all unique IDs were found
            {
                var foundIds = existingOwners.Select(o => o.Id).ToList();
                var missingIds = createShipDto.OwnerIds.Except(foundIds);
                return (null, $"The following owner IDs do not exist: {string.Join(", ", missingIds)}");
            }

            // map DTO
            var ship = _mapper.Map<Ship>(createShipDto);

            await _shipInterface.AddShipWithOwnerLinksAsync(ship, createShipDto.OwnerIds);
            _logger.LogInformation("Created new ship with ID {ShipId} and linked owners.", ship.Id);

            // fetch details
            var newShipDetails = await _shipInterface.GetDetailsByIdAsync(ship.Id);

            // map DTO and return
            return (_mapper.Map<ShipDetailsDto>(newShipDetails), null); // Success
        }

        public async Task<bool> DeleteShipAsync(int id)
        {
            _logger.LogInformation("Attempting to delete ship with ID {ShipId}", id);
            bool deleted = await _shipInterface.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Ship with ID {ShipId} not found for deletion.", id);
            }
            else
            {
                _logger.LogInformation("Successfully deleted ship with ID {ShipId}", id);
            }
            return deleted;
        }

        public async Task<IEnumerable<ShipDto>> GetAllShipsAsync()
        {
            var ships = await _shipInterface.GetAllAsync();
            return _mapper.Map<IEnumerable<ShipDto>>(ships);
        }

        public async Task<ShipDetailsDto?> GetShipDetailsAsync(int id)
        {
            var ship = await _shipInterface.GetDetailsByIdAsync(id);
            if (ship == null)
            {
                return null;
            }
            return _mapper.Map<ShipDetailsDto>(ship);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateShipAsync(int id, UpdateShipDto updateShipDto)
        {
            var existingShip = await _shipInterface.GetByIdAsync(id); // Fetch without tracking for update
            if (existingShip == null)
            {
                return (false, $"Ship with ID {id} not found.");
            }

            if (existingShip.ImoNumber != updateShipDto.ImoNumber && await _shipInterface.ImoNumberExistsAsync(updateShipDto.ImoNumber, id))
            {
                return (false, $"Another ship with IMO Number {updateShipDto.ImoNumber} already exists.");
            }

            // ! AutoMapper can overwrite properties on an existing object
            _mapper.Map(updateShipDto, existingShip);
            existingShip.Id = id;

            try
            {
                await _shipInterface.UpdateAsync(existingShip);
                _logger.LogInformation("Updated ship with ID {ShipId}", id);
                return (true, null);
            }
            catch (DbUpdateConcurrencyException ex) {
                // handle concurrency issues
                _logger.LogError(ex, "Concurrency error updating ship with ID {ShipId}", id);
                return (false, "Could not update the ship due to a concurrency conflict.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ship with ID {ShipId}", id);
                return (false, "An unexpected error occurred while updating the ship.");
            }
        }
    }
}