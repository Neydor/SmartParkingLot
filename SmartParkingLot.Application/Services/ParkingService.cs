using SmartParkingLot.Application.DTOs.Request;
using SmartParkingLot.Application.DTOs.Responses;
using SmartParkingLot.Application.Exceptions;
using SmartParkingLot.Application.Interfaces;
using SmartParkingLot.Domain.Entities;
using SmartParkingLot.Domain.Enums;
using SmartParkingLot.Domain.Interfaces;

namespace SmartParkingLot.Application.Services
{
    public class ParkingService : IParkingService
    {
        private readonly IParkingSpotRepository _spotRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IRateLimiterService _rateLimiter; // Bonus
        private const string RateLimitActionKey = "spot_status_change"; // Bonus

        public ParkingService(IParkingSpotRepository spotRepository, IDeviceRepository deviceRepository, IRateLimiterService rateLimiter)
        {
            _spotRepository = spotRepository ?? throw new ArgumentNullException(nameof(spotRepository));
            _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter)); // Bonus
        }

        public async Task<PaginatedResultDto<ParkingSpotDto>> GetAllSpotsAsync(int pageNumber, int pageSize)
        {
            // Basic validation for pagination params
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 2, 100); // Example limits

            var (spots, totalCount) = await _spotRepository.GetAllAsync(pageNumber, pageSize);

            var spotDtos = spots.Select(spot => new ParkingSpotDto
            {
                Id = spot.Id,
                Name = spot.Name,
                Status = spot.Status.ToString(),
                OccupyingDeviceId = spot.OccupyingDeviceId,
                LastStatusChangeUtc = spot.LastStatusChangeUtc
            }).ToList();

            return new PaginatedResultDto<ParkingSpotDto>
            {
                Items = spotDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ParkingSpotDto?> GetSpotByIdAsync(Guid id)
        {
            var spot = await _spotRepository.GetByIdAsync(id);
            if (spot == null)
            {
                return null; // Or throw NotFoundException
            }

            return new ParkingSpotDto
            {
                Id = spot.Id,
                Name = spot.Name,
                Status = spot.Status.ToString(),
                OccupyingDeviceId = spot.OccupyingDeviceId,
                LastStatusChangeUtc = spot.LastStatusChangeUtc
            };
        }

        public async Task<ParkingSpotDto> AddSpotAsync(CreateParkingSpotDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                // Basic validation, could use FluentValidation later
                throw new System.ComponentModel.DataAnnotations.ValidationException("Parking spot name cannot be empty.");
            }

            var newSpot = new ParkingSpot(Guid.NewGuid(), createDto.Name);
            await _spotRepository.AddAsync(newSpot);

            // Map to DTO for return
            return new ParkingSpotDto
            {
                Id = newSpot.Id,
                Name = newSpot.Name,
                Status = newSpot.Status.ToString(),
                OccupyingDeviceId = newSpot.OccupyingDeviceId,
                LastStatusChangeUtc = newSpot.LastStatusChangeUtc
            };
        }

        public async Task DeleteSpotAsync(Guid id)
        {
            var spotExists = await _spotRepository.ExistsAsync(id);
            if (!spotExists)
            {
                throw new NotFoundException($"Parking spot with ID {id} not found.");
            }
            await _spotRepository.DeleteAsync(id);
        }

        public async Task OccupySpotAsync(Guid spotId, Guid deviceId)
        {
            //El bonus
            if (!await _rateLimiter.IsActionAllowedAsync(deviceId, RateLimitActionKey))
            {
                throw new ValidationException($"Device {deviceId} rate limit exceeded for action '{RateLimitActionKey}'. Please try again later.");
            }

            if (!await _deviceRepository.IsDeviceRegisteredAsync(deviceId))
            {
                throw new ValidationException($"Device with ID {deviceId} is not registered.");
            }
            var deviceInSpot = await _spotRepository.ExistsOccupiedSpotByDeviceAsync(deviceId);
            if (deviceInSpot)
            {
                throw new ValidationException($"Device with ID {deviceId} is occupied a parking spot.");
            }
            var spot = await _spotRepository.GetByIdAsync(spotId) ?? throw new NotFoundException($"Parking spot with ID {spotId} not found.");
            try
            {
                spot.Occupy(deviceId); 
                await _spotRepository.UpdateAsync(spot);
            }
            catch (InvalidOperationException ex) 
            {
                throw new ConflictException(ex.Message, ex);
            }
        }

        public async Task FreeSpotAsync(Guid spotId, Guid deviceId)
        {
            Guid? occupyingDeviceId;
            if (!await _rateLimiter.IsActionAllowedAsync(deviceId, RateLimitActionKey))
            {
                throw new ValidationException($"Device {deviceId} rate limit exceeded for action '{RateLimitActionKey}'. Please try again later.");
            }

            if (!await _deviceRepository.IsDeviceRegisteredAsync(deviceId))
            {
                throw new ValidationException($"Device with ID {deviceId} is not registered.");
            }

            var spot = await _spotRepository.GetByIdAsync(spotId) ?? throw new NotFoundException($"Parking spot with ID {spotId} not found.");
            occupyingDeviceId = spot.OccupyingDeviceId;
            try
            {
                spot.Free();
                if (occupyingDeviceId != deviceId)
                {
                    throw new ValidationException($"Device {deviceId} cannot free spot {spotId} as it is not occupying it.");
                }
                await _spotRepository.UpdateAsync(spot);
            }
            catch (InvalidOperationException ex) // Catch domain validation errors
            {
                // Map to application-level exception (Conflict)
                throw new ConflictException(ex.Message, ex);
            }
        }

        public async Task<int> GetAvailableSpotCountAsync()
        {
            // This could be optimized if needed (e.g., maintaining a counter)
            // but querying is fine for moderate numbers of spots.
            var (spots, _) = await _spotRepository.GetAllAsync(1, int.MaxValue); // Get all spots
            return spots.Count(s => s.Status == ParkingSpotStatus.Free);
        }
    }
}
