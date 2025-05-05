using Microsoft.AspNetCore.Mvc;
using VesselManagementApi.DTOs;
using VesselManagementApi.Services;

namespace VesselManagementApi.Controllers
{
    [Route("api/[controller]")] // api/owners
    [ApiController]
    public class OwnersController : ControllerBase
    {
        private readonly IOwnerService _ownerService;
        private readonly ILogger<OwnersController> _logger;

        public OwnersController(IOwnerService ownerService, ILogger<OwnersController> logger)
        {
            _ownerService = ownerService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all owners.
        /// </summary>
        /// <returns>A list of owners.</returns>
        [HttpGet] // GET api/owners
        [ProducesResponseType(typeof(IEnumerable<OwnerDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OwnerDto>>> GetOwners()
        {
            var owners = await _ownerService.GetAllOwnersAsync();
            return Ok(owners);
        }

        /// <summary>
        /// Gets a specific owner by ID.
        /// </summary>
        /// <param name="id">The ID of the owner to retrieve.</param>
        /// <returns>The requested owner.</returns>
        [HttpGet("{id}")] // GET api/owners/{id}
        [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OwnerDto>> GetOwner(int id)
        {
            var owner = await _ownerService.GetOwnerByIdAsync(id);
            if (owner == null)
            {
                _logger.LogWarning("GetOwner: Owner with ID {OwnerId} not found.", id);
                return NotFound($"Owner with ID {id} not found.");
            }
            return Ok(owner);
        }

        /// <summary>
        /// Creates a new owner.
        /// </summary>
        /// <param name="createOwnerDto">The details of the owner to create.</param>
        /// <returns>The newly created owner.</returns>
        [HttpPost] // POST api/owners
        [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OwnerDto>> CreateOwner(CreateOwnerDto createOwnerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            OwnerDto? newOwner = await _ownerService.CreateOwnerAsync(createOwnerDto);

            return CreatedAtAction(nameof(GetOwner), new { id = newOwner.Id }, newOwner);
        }

        /// <summary>
        /// Deletes an owner and their associations with ships. (Requirement 6)
        /// </summary>
        /// <param name="id">The ID of the owner to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")] // DELETE api/owners/{id}
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOwner(int id)
        {
            bool success = await _ownerService.DeleteOwnerAsync(id);
            if (!success)
            {
                _logger.LogWarning("DeleteOwner: Owner with ID {OwnerId} not found.", id);
                return NotFound($"Owner with ID {id} not found.");
            }
            return NoContent();
        }
    }
}
