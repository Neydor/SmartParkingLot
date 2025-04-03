using SmartParkingLot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParkingLot.Domain.Interfaces
{
    public interface IDeviceRepository
    {
        Task<bool> IsDeviceRegisteredAsync(Guid deviceId);
        Task RegisterDeviceAsync(Device device); // For setup/testing
        Task<IEnumerable<Device>> GetAllRegisteredDevicesAsync(); // For potential admin tasks
    }
}
