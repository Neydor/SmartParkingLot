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
            try
            {
                // Example: Register a few devices
                var device1 = new Device(Guid.Parse("a1a1a1a1-b2b2-c3c3-d4d4-e5e5e5e5e5e5"));

                // --- CORRECTED GUID STRING ---
                // Replaced invalid characters with valid hex (a, b, c, d)
                var device2 = new Device(Guid.Parse("f6f6f6f6-a7a7-b8b8-c9c9-d0d0d0d0d0d0"));
                // --- OR generate a new one ---
                // var device2 = new Device(Guid.NewGuid());
                // Console.WriteLine($"Generated Device 2 ID: {device2.Id}"); // Log if needed

                Devices.TryAdd(device1.Id, device1);
                Devices.TryAdd(device2.Id, device2);

                // Example: Add a few parking spots
                var spot1 = new ParkingSpot(Guid.Parse("10000000-0000-0000-0000-000000000001"), "A1");
                var spot2 = new ParkingSpot(Guid.Parse("10000000-0000-0000-0000-000000000002"), "A2");
                var spot3 = new ParkingSpot(Guid.Parse("10000000-0000-0000-0000-000000000003"), "A3");
                ParkingSpots.TryAdd(spot1.Id, spot1);
                ParkingSpots.TryAdd(spot2.Id, spot2);
                ParkingSpots.TryAdd(spot3.Id, spot3);

                Console.WriteLine("DataStore Initialized Successfully."); // Confirmation
                Console.WriteLine($"Registered Device 1: {device1.Id}");
                Console.WriteLine($"Registered Device 2: {device2.Id}");
                Console.WriteLine($"Added A1: {spot1.Id} ({spot1.Name})");
                Console.WriteLine($"Added A2: {spot2.Id} ({spot2.Name})");
                Console.WriteLine($"Added A3: {spot3.Id} ({spot3.Name})");


            }
            catch (FormatException ex)
            {
                Console.WriteLine($"FATAL ERROR initializing DataStore: {ex.Message}");
                Console.WriteLine($"Input string causing error: Likely one of the Guid.Parse() calls."); // Pinpoint
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw; // Re-throw to stop the application as this is critical initialization
            }
            catch (Exception ex) // Catch other potential issues during initialization
            {
                Console.WriteLine($"FATAL UNEXPECTED ERROR initializing DataStore: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
