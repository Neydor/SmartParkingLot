using SmartParkingLot.Domain.Entities;

namespace SmartParkingLot.Domain.Interfaces
{
    public interface IParkingSpotRepository
    {
        Task<ParkingSpot?> GetByIdAsync(Guid id);
        // Added Pagination support
        Task<(IEnumerable<ParkingSpot> Spots, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
        Task AddAsync(ParkingSpot spot);
        Task UpdateAsync(ParkingSpot spot);
        Task DeleteAsync(Guid id);
        Task<int> GetTotalCountAsync(); // Helper for pagination
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsOccupiedSpotByDeviceAsync(Guid deviceId);
    }
}
