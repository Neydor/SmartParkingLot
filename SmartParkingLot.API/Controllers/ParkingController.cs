using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingLot.Application.Services;

namespace SmartParkingLot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingController : ControllerBase
    {
        private readonly IParkingService _parkingService;

        public ParkingController(IParkingService parkingService)
        {
            _parkingService = parkingService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ParkingSpotDto>>> GetAll(
            [FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var pagedSpots = await _parkingService.GetAllParkingSpotsAsync(paginationParams);
                return Ok(pagedSpots);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParkingSpotDto>> GetById(Guid id)
        {
            try
            {
                var spot = await _parkingService.GetParkingSpotByIdAsync(id);
                return Ok(spot);
            }
            catch (SpotNotFoundException)
            {
                return NotFound($"Parking spot {id} not found");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            try
            {
                var newSpot = await _parkingService.AddParkingSpotAsync();
                return CreatedAtAction(nameof(GetById), new { id = newSpot.Id }, newSpot);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _parkingService.RemoveParkingSpotAsync(id);
                return NoContent();
            }
            catch (SpotNotFoundException)
            {
                return NotFound($"Parking spot {id} not found");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("{id}/occupy")]
        [ServiceFilter(typeof(RateLimitFilter))]
        public async Task<IActionResult> OccupySpot(
            Guid id,
            [FromHeader(Name = "Device-Id")] Guid deviceId)
        {
            try
            {
                await _parkingService.OccupySpotAsync(id, deviceId);
                return Ok();
            }
            catch (UnauthorizedDeviceException)
            {
                return Forbid();
            }
            catch (SpotNotFoundException)
            {
                return NotFound($"Parking spot {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("{id}/free")]
        [ServiceFilter(typeof(RateLimitFilter))]
        public async Task<IActionResult> FreeSpot(
            Guid id,
            [FromHeader(Name = "Device-Id")] Guid deviceId)
        {
            try
            {
                await _parkingService.FreeSpotAsync(id, deviceId);
                return Ok();
            }
            catch (UnauthorizedDeviceException)
            {
                return Forbid();
            }
            catch (SpotNotFoundException)
            {
                return NotFound($"Parking spot {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        private ActionResult HandleException(Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { Message = "An unexpected error occurred", Details = ex.Message });
        }
    }
}
