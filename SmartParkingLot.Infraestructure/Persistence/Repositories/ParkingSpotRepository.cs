using SmartParkingLot.Domain.Entities;
using SmartParkingLot.Domain.Interfaces;
using System.Collections.Concurrent;

namespace SmartParkingLot.Infraestructure.Persistence.Repositories
{
    public class ParkingSpotRepository : IParkingSpotRepository
    {
        private readonly ConcurrentDictionary<Guid, ParkingSpot> _spots = DataStore.ParkingSpots;

        public Task<ParkingSpot?> GetByIdAsync(Guid id)
        {
            _spots.TryGetValue(id, out var spot);
            return Task.FromResult(spot);
        }

        public Task<(IEnumerable<ParkingSpot> Spots, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
        {
            var allSpots = _spots.Values.OrderBy(s => s.Name).ToList(); // Order for consistent pagination
            var totalCount = allSpots.Count;
            var paginatedSpots = allSpots
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList(); // Execute query

            return Task.FromResult(((IEnumerable<ParkingSpot>)paginatedSpots, totalCount));
        }

        public Task AddAsync(ParkingSpot spot)
        {
            if (!_spots.TryAdd(spot.Id, spot))
            {
                throw new InvalidOperationException($"Spot with ID {spot.Id} already exists.");
            }
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ParkingSpot spot)
        {
            if (!_spots.ContainsKey(spot.Id))
            {
                throw new InvalidOperationException($"Spot with ID {spot.Id} not found for update.");
            }
            _spots[spot.Id] = spot; // Replace existing entry
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            if (!_spots.TryRemove(id, out _))
            {
                throw new InvalidOperationException($"Spot with ID {id} not found for deletion.");
            }
            return Task.CompletedTask;
        }

        public Task<int> GetTotalCountAsync()
        {
            return Task.FromResult(_spots.Count);
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            return Task.FromResult(_spots.ContainsKey(id));
        }
        public Task<bool> ExistsOccupiedSpotByDeviceAsync(Guid deviceId)
        {
            var exists = _spots.Values.Any(spot => spot.OccupyingDeviceId == deviceId);
            return Task.FromResult(exists);
        }
    }
}