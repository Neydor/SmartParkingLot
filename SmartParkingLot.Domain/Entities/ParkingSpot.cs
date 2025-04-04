using SmartParkingLot.Domain.Enums;

namespace SmartParkingLot.Domain.Entities
{
    public class ParkingSpot
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } // e.g., "A1", "B5"
        public ParkingSpotStatus Status { get; private set; }
        public Guid? OccupyingDeviceId { get; private set; } // Track which device occupied it
        public DateTime? LastStatusChangeUtc { get; private set; }

        // Private constructor for persistence/mapping frameworks
        private ParkingSpot() { }

        public ParkingSpot(Guid id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Status = ParkingSpotStatus.Free;
            LastStatusChangeUtc = DateTime.UtcNow;
        }

        public void Occupy(Guid deviceId)
        {
            if (Status == ParkingSpotStatus.Occupied)
            {
                // Maybe throw specific domain exception later if needed
                throw new InvalidOperationException($"Spot {Name} ({Id}) is already occupied.");
            }
            Status = ParkingSpotStatus.Occupied;
            OccupyingDeviceId = deviceId;
            LastStatusChangeUtc = DateTime.UtcNow;
        }

        public void Free() // Pass deviceId for potential auditing/validation
        {
            if (Status == ParkingSpotStatus.Free)
            {
                throw new InvalidOperationException($"Spot {Name} ({Id}) is already free.");
            }
            Status = ParkingSpotStatus.Free;
            OccupyingDeviceId = null;
            LastStatusChangeUtc = DateTime.UtcNow;
            // deviceId parameter is currently unused, consider using it for auditing/validation
        }

        public void UpdateName(string newName)
        {
            Name = newName ?? throw new ArgumentNullException(nameof(newName));
        }
    }
}
