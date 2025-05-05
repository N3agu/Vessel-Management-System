using AutoMapper;
using VesselManagementApi.DTOs;
using VesselManagementApi.Interfaces;
using VesselManagementApi.Models;

namespace VesselManagementApi.Services
{
    public class OwnerService : IOwnerService
    {
        private readonly IOwnerInterface _ownerInterface;
        private readonly IMapper _mapper;
        private readonly ILogger<OwnerService> _logger;

        public OwnerService(IOwnerInterface ownerInterface, IMapper mapper, ILogger<OwnerService> logger)
        {
            _ownerInterface = ownerInterface;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OwnerDto> CreateOwnerAsync(CreateOwnerDto createOwnerDto)
        {
            var owner = _mapper.Map<Owner>(createOwnerDto);
            var newOwner = await _ownerInterface.AddAsync(owner);
            _logger.LogInformation("Created new owner with ID {OwnerId}", newOwner.Id);
            return _mapper.Map<OwnerDto>(newOwner);
        }

        public async Task<bool> DeleteOwnerAsync(int id)
        {
            // The interface's DeleteAsync handles the deletion of ShipOwner links
            _logger.LogInformation("Attempting to delete owner with ID {OwnerId}", id);
            bool deleted = await _ownerInterface.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Owner with ID {OwnerId} not found for deletion.", id);
            }
            else
            {
                _logger.LogInformation("Successfully deleted owner with ID {OwnerId}", id);
            }
            return deleted;
        }

        public async Task<IEnumerable<OwnerDto>> GetAllOwnersAsync()
        {
            var owners = await _ownerInterface.GetAllAsync();
            return _mapper.Map<IEnumerable<OwnerDto>>(owners);
        }

        public async Task<OwnerDto?> GetOwnerByIdAsync(int id)
        {
            var owner = await _ownerInterface.GetByIdAsync(id);
            if (owner == null)
            {
                return null;
            }
            return _mapper.Map<OwnerDto>(owner);
        }
    }
}
}
