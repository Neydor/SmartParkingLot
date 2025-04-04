using Microsoft.AspNetCore.Mvc;
using SmartParkingLot.API.DTOs.Responses;
using SmartParkingLot.Application.DTOs.Request;
using SmartParkingLot.Application.DTOs.Responses;
using SmartParkingLot.Application.Exceptions;
using SmartParkingLot.Application.Interfaces;

namespace SmartParkingLot.API.Controllers
{
    [Route("api/parking-spots")]
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

        // POST /api/parking-spots
        [HttpPost]
        [ProducesResponseType(typeof(ParkingSpotDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ParkingSpotDto>> AddParkingSpot([FromBody] CreateParkingSpotDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Adding new parking spot with Name: {SpotName}", createDto.Name);
            var newSpot = await _parkingService.AddSpotAsync(createDto);
            return CreatedAtAction(nameof(_parkingService.GetSpotByIdAsync), new { id = newSpot.Id }, newSpot);
        }

        // DELETE /api/parking-spots/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteParkingSpot(Guid id)
        {
            _logger.LogInformation("Deleting parking spot with ID: {SpotId}", id);
            await _parkingService.DeleteSpotAsync(id);
            return NoContent();

        }

        // POST /api/parking-spots/{id}/occupy
        [HttpPost("{id:guid}/occupy")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OccupyParkingSpot(Guid id, [FromHeader(Name = DeviceIdHeader)] Guid deviceId)
        {
            if (deviceId == Guid.Empty)
            {
                return BadRequest($"Missing or invalid header: {DeviceIdHeader}");
            }

            _logger.LogInformation("Device {DeviceId} attempting to occupy spot {SpotId}", deviceId, id);
            await _parkingService.OccupySpotAsync(id, deviceId);
            return NoContent();
        }

        // POST /api/parking-spots/{id}/free
        [HttpPost("{id:guid}/free")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FreeParkingSpot(Guid id, [FromHeader(Name = DeviceIdHeader)] Guid deviceId)
        {
            if (deviceId == Guid.Empty)
            {
                return BadRequest($"Missing or invalid header: {DeviceIdHeader}");
            }

            _logger.LogInformation("Device {DeviceId} attempting to free spot {SpotId}", deviceId, id);
            await _parkingService.FreeSpotAsync(id, deviceId);
            return NoContent();
        }
    }
}