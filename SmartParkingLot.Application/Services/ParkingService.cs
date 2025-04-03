using SmartParkingLot.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParkingLot.Application.Services
{
    public class ParkingService
    {
        private readonly IParkingRepository _repo;
        private readonly IDeviceValidator _deviceValidator;

        public ParkingService(
            IParkingRepository repo, 
            IDeviceValidator validator
            )
        {
            _repo = repo;
            _deviceValidator = validator;
        }

        public async Task OccupySpotAsync(Guid spotId, Guid deviceId)
        {
            if (!_deviceValidator.IsValidDevice(deviceId))
                throw new UnauthorizedDeviceException();

            var spot = _repo.GetById(spotId) ?? throw new SpotNotFoundException();

            if (spot.IsOccupied)
                throw new InvalidOperationException("Spot ya ocupado");

            spot.Occupy();
            _repo.Update(spot);
        }

        // ...otros métodos
    }
}
