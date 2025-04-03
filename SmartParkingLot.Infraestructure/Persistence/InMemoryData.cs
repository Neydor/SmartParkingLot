using SmartParkingLot.Domain.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParkingLot.Infraestructure.Persistence
{
    public class InMemoryData
    {
        public ConcurrentDictionary<Guid, ParkingSpot> ParkingSpots { get; } = new();
        public ConcurrentDictionary<Guid, Device> Devices { get; } = new();

        // Pre-seed some devices for testing registration requirement
        public InMemoryData()
        {
            // Example registered devices
            Devices.TryAdd(Guid.Parse("d1e1a1a1-0b0e-4e8a-8a1a-1e1e1a1a1e1a"), new Device(Guid.Parse("d1e1a1a1-0b0e-4e8a-8a1a-1e1e1a1a1e1a")));
            Devices.TryAdd(Guid.Parse("d2e2b2b2-0c0f-4f9b-9b2b-2f2f2b2b2f2b"), new Device(Guid.Parse("d2e2b2b2-0c0f-4f9b-9b2b-2f2f2b2b2f2b")));
        }
    }
}
