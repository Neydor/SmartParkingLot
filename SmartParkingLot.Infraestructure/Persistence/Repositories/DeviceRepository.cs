using SmartParkingLot.Domain.Entities;
using SmartParkingLot.Domain.Interfaces;
using System.Collections.Concurrent;

namespace SmartParkingLot.Infraestructure.Persistence.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ConcurrentDictionary<Guid, Device> _devices = DataStore.Devices;

        public Task<bool> IsDeviceRegisteredAsync(Guid deviceId)
        {
            return Task.FromResult(_devices.ContainsKey(deviceId));
        }

        public Task RegisterDeviceAsync(Device device)
        {
            _devices.TryAdd(device.Id, device); 
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Device>> GetAllRegisteredDevicesAsync()
        {
            return Task.FromResult(_devices.Values.ToList().AsEnumerable());
        }
    }
}
