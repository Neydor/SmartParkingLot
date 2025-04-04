using SmartParkingLot.Domain.Entities;
using System.Collections.Concurrent;

namespace SmartParkingLot.Infraestructure.Persistence
{
    public static class DataStore
    {
        public static ConcurrentDictionary<Guid, ParkingSpot> ParkingSpots { get; } = new();
        public static ConcurrentDictionary<Guid, Device> Devices { get; } = new();
        static DataStore()
        {
            var device1 = new Device(Guid.Parse("a1a1a1a1-b2b2-c3c3-d4d4-e5e5e5e5e5e5"));
            var device2 = new Device(Guid.Parse("f6f6f6f6-g7g7-h8h8-i9i9-j0j0j0j0j0j0"));
            Devices.TryAdd(device1.Id, device1);
            Devices.TryAdd(device2.Id, device2);

            var spot1 = new ParkingSpot(Guid.Parse("10000000-0000-0000-0000-000000000001"), "A1");
            var spot2 = new ParkingSpot(Guid.Parse("10000000-0000-0000-0000-000000000002"), "A2");
            ParkingSpots.TryAdd(spot1.Id, spot1);
            ParkingSpots.TryAdd(spot2.Id, spot2);
        }
    }
}
