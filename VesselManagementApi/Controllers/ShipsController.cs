using Microsoft.AspNetCore.Mvc;
using VesselManagementApi.DTOs;
using VesselManagementApi.Services;

namespace VesselManagementApi.Controllers
{
    [Route("api/[controller]")] // api/ships
    [ApiController]
    public class ShipsController : ControllerBase
    {
        private readonly IShipService _shipService;
        private readonly ILogger<ShipsController> _logger;

        public ShipsController(IShipService shipService, ILogger<ShipsController> logger)
        {
            _shipService = shipService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all ships.
        /// </summary>
        /// <returns>A list of ships.</returns>
        [HttpGet] // GET api/ships
        [ProducesResponseType(typeof(IEnumerable<ShipDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ShipDto>>> GetShips()
        {
            var ships = await _shipService.GetAllShipsAsync();
            return Ok(ships);
        }

        /// <summary>
        /// Gets detailed information for a specific ship, including its owners.
        /// </summary>
        /// <param name="id">The ID of the ship to retrieve.</param>
        /// <returns>The detailed ship information.</returns>
        [HttpGet("{id}")] // GET api/ships/{id}
        [ProducesResponseType(typeof(ShipDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShipDetailsDto>> GetShipDetails(int id)
        {
            var shipDetails = await _shipService.GetShipDetailsAsync(id);
            if (shipDetails == null)
            {
                _logger.LogWarning("GetShipDetails: Ship with ID {ShipId} not found.", id);
                return NotFound($"Ship with ID {id} not found.");
            }
            return Ok(shipDetails);
        }

        /// <summary>
        /// Adds a new ship.
        /// </summary>
        /// <param name="createShipDto">The details of the ship to create, including owner IDs.</param>
        /// <returns>The newly created ship's details.</returns>
        [HttpPost] // POST api/ships
        [ProducesResponseType(typeof(ShipDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShipDetailsDto>> CreateShip(CreateShipDto createShipDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (newShip, errorMessage) = await _shipService.CreateShipAsync(createShipDto);

            if (newShip == null)
            {
                // duplicate IMO, non-existent owners, etc.
                _logger.LogWarning("CreateShip failed: {ErrorMessage}", errorMessage);
                return BadRequest(errorMessage);
            }

            // Return 201 Created status with a Location header and the created resource
            return CreatedAtAction(nameof(GetShipDetails), new { id = newShip.Id }, newShip);
        }

        /// <summary>
        /// Updates an existing ship's properties.
        /// </summary>
        /// <param name="id">The ID of the ship to update.</param>
        /// <param name="updateShipDto">The updated ship details.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}")] // PUT api/ships/{id}
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateShip(int id, UpdateShipDto updateShipDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, errorMessage) = await _shipService.UpdateShipAsync(id, updateShipDto);

            if (!success)
            {
                // Determine if it was not found or another error
                if (errorMessage != null && errorMessage.Contains("not found"))
                {
                    _logger.LogWarning("UpdateShip: Ship with ID {ShipId} not found.", id);
                    return NotFound(errorMessage);
                }
                else
                {
                    _logger.LogWarning("UpdateShip failed for ID {ShipId}: {ErrorMessage}", id, errorMessage);
                    return BadRequest(errorMessage);
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a ship.
        /// </summary>
        /// <param name="id">The ID of the ship to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")] // DELETE api/ships/{id}
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteShip(int id)
        {
            var success = await _shipService.DeleteShipAsync(id);
            if (!success)
            {
                _logger.LogWarning("DeleteShip: Ship with ID {ShipId} not found.", id);
                return NotFound($"Ship with ID {id} not found.");
            }
            return NoContent();
        }
    }
}
