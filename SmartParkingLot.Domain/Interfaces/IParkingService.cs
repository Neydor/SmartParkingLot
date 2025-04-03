using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParkingLot.Domain.Interfaces
{
    public interface IParkingService
    {
        Task<IEnumerable<ParkingSpotDto>> GetAllSpotsAsync();
        Task<ParkingSpotDto?> GetSpotByIdAsync(Guid id); // Added for completeness
        Task<ParkingSpotDto> AddSpotAsync(CreateParkingSpotRequest request);
        Task DeleteSpotAsync(Guid id);
        Task OccupySpotAsync(Guid spotId, Guid deviceId); // Pass deviceId for validation
        Task FreeSpotAsync(Guid spotId, Guid deviceId);   // Pass deviceId for validation
        Task<int> GetAvailableSpotCountAsync();
    }
}
