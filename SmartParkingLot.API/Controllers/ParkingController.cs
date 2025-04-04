using Microsoft.AspNetCore.Mvc;
using SmartParkingLot.API.DTOs.Responses;
using SmartParkingLot.Application.DTOs.Request;
using SmartParkingLot.Application.DTOs.Responses;
using SmartParkingLot.Application.Exceptions;
using SmartParkingLot.Application.Interfaces;

namespace SmartParkingLot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingController : ControllerBase
    {
        private readonly IParkingService _parkingService;
        private readonly ILogger<ParkingController> _logger; 

        private const string DeviceIdHeader = "X-Device-ID";

        public ParkingController(IParkingService parkingService, ILogger<ParkingController> logger)
        {
            _parkingService = parkingService ?? throw new ArgumentNullException(nameof(parkingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET /api/parking-spots?pageNumber=1&pageSize=10
        [HttpGet]
        [ProducesResponseType(typeof(ApiPaginatedResponse<ParkingSpotDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiPaginatedResponse<ParkingSpotDto>>> GetAllParkingSpots(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Getting all parking spots (Page: {Page}, Size: {Size})", pageNumber, pageSize);
            var result = await _parkingService.GetAllSpotsAsync(pageNumber, pageSize);

            // Map to API-specific pagination response format
            var response = new ApiPaginatedResponse<ParkingSpotDto>
            {
                Data = result.Items,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalItems = result.TotalCount,
                TotalPages = result.TotalPages
            };

            return Ok(response);
        }

        // GET /api/parking-spots/available-count (Example endpoint not in spec, but useful)
        [HttpGet("available-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetAvailableSpotCount()
        {
            _logger.LogInformation("Getting available parking spot count");
            var count = await _parkingService.GetAvailableSpotCountAsync();
            return Ok(count);
        }

        // GET /api/parking-spots/{id} (Example endpoint not in spec, but useful)
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ParkingSpotDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ParkingSpotDto>> GetParkingSpotById(Guid id)
        {
            _logger.LogInformation("Getting parking spot by ID: {SpotId}", id);
            var spot = await _parkingService.GetSpotByIdAsync(id);
            if (spot == null)
            {
                return NotFound($"Parking spot with ID {id} not found.");
            }
            return Ok(spot);
        }

        // POST /api/parking-spots
        [HttpPost]
        [ProducesResponseType(typeof(ParkingSpotDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ParkingSpotDto>> AddParkingSpot([FromBody] CreateParkingSpotDto createDto)
        {
            if (!ModelState.IsValid) // Basic validation
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Adding new parking spot with Name: {SpotName}", createDto.Name);
            try
            {
                var newSpot = await _parkingService.AddSpotAsync(createDto);
                // Return 201 Created with the location of the new resource and the resource itself
                return CreatedAtAction(nameof(GetParkingSpotById), new { id = newSpot.Id }, newSpot);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while adding spot: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex) // Catch unexpected errors
            {
                _logger.LogError(ex, "Error adding parking spot");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        public Task<IActionResult> DeleteParkingSpot(Guid id)
        {
            return DeleteParkingSpot(id, _logger);
        }

        // DELETE /api/parking-spots/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteParkingSpot(Guid id, ILogger _logger)
        {
            _logger.LogInformation("Deleting parking spot with ID: {SpotId}", id);
            try
            {
                await _parkingService.DeleteSpotAsync(id);
                return NoContent(); // Standard response for successful DELETE
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Delete failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting parking spot {SpotId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // POST /api/parking-spots/{id}/occupy
        [HttpPost("{id:guid}/occupy")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)] // For Rate Limit
        public async Task<IActionResult> OccupyParkingSpot(Guid id, [FromHeader(Name = DeviceIdHeader)] Guid deviceId)
        {
            if (deviceId == Guid.Empty)
            {
                return BadRequest($"Missing or invalid header: {DeviceIdHeader}");
            }

            _logger.LogInformation("Device {DeviceId} attempting to occupy spot {SpotId}", deviceId, id);
            try
            {
                await _parkingService.OccupySpotAsync(id, deviceId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "NotFoundException: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "ValidationException: {Message}", ex.Message);
                return BadRequest(ex.Message);
            } // Catches unregistered device & rate limit
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "ConflictException: {Message}", ex.Message);
                return Conflict(ex.Message);
            } // Catches already occupied/free
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occupying parking spot {SpotId} by device {DeviceId}", id, deviceId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // POST /api/parking-spots/{id}/free
        [HttpPost("{id:guid}/free")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)] // For Rate Limit
        public async Task<IActionResult> FreeParkingSpot(Guid id, [FromHeader(Name = DeviceIdHeader)] Guid deviceId)
        {
            if (deviceId == Guid.Empty)
            {
                return BadRequest($"Missing or invalid header: {DeviceIdHeader}");
            }

            _logger.LogInformation("Device {DeviceId} attempting to free spot {SpotId}", deviceId, id);
            try
            {
                await _parkingService.FreeSpotAsync(id, deviceId);
                return NoContent();
            }
            catch (NotFoundException ex) { 
                _logger.LogWarning(ex, "NotFoundException: {Message}", ex.Message); 
                return NotFound(ex.Message); 
            }
            catch (ValidationException ex) { 
                _logger.LogWarning(ex, "ValidationException: {Message}", ex.Message); 
                return BadRequest(ex.Message); 
            }
            catch (ConflictException ex) { 
                _logger.LogWarning(ex, "ConflictException: {Message}", ex.Message);
                return Conflict(ex.Message); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error freeing parking spot {SpotId} by device {DeviceId}", id, deviceId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
