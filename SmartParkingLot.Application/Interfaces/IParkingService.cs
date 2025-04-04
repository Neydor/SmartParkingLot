using SmartParkingLot.Application.DTOs.Request;
using SmartParkingLot.Application.DTOs.Responses;

namespace SmartParkingLot.Application.Interfaces
{
    public interface IParkingService
    {
        Task<PaginatedResultDto<ParkingSpotDto>> GetAllSpotsAsync(int pageNumber, int pageSize);
        Task<ParkingSpotDto?> GetSpotByIdAsync(Guid id);
        Task<ParkingSpotDto> AddSpotAsync(CreateParkingSpotDto createDto);
        Task DeleteSpotAsync(Guid id);
        Task OccupySpotAsync(Guid spotId, Guid deviceId);
        Task FreeSpotAsync(Guid spotId, Guid deviceId);
        Task<int> GetAvailableSpotCountAsync();
    }
}
