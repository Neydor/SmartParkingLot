using SmartParkingLot.Domain.Enums;

namespace SmartParkingLot.Domain.Entities
{
    public class ParkingSpot
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } 
        public ParkingSpotStatus Status { get; private set; }
        public Guid? OccupyingDeviceId { get; private set; } 
        public DateTime? LastStatusChangeUtc { get; private set; }

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
                throw new InvalidOperationException($"Spot {Name} ({Id}) is already occupied.");
            }
            Status = ParkingSpotStatus.Occupied;
            OccupyingDeviceId = deviceId;
            LastStatusChangeUtc = DateTime.UtcNow;
        }

        public void Free()
        {
            if (Status == ParkingSpotStatus.Free)
            {
                throw new InvalidOperationException($"Spot {Name} ({Id}) is already free.");
            }
            Status = ParkingSpotStatus.Free;
            OccupyingDeviceId = null;
            LastStatusChangeUtc = DateTime.UtcNow;
        }

        public void UpdateName(string newName)
        {
            Name = newName ?? throw new ArgumentNullException(nameof(newName));
        }
    }
}
