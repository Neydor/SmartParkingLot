using SmartParkingLot.Domain.Entities;
namespace SmartParkingLot.Domain.Interfaces
{
    public interface IDeviceRepository
    {
        Task<bool> IsDeviceRegisteredAsync(Guid deviceId);
        Task RegisterDeviceAsync(Device device); 
        Task<IEnumerable<Device>> GetAllRegisteredDevicesAsync();
    }
}
