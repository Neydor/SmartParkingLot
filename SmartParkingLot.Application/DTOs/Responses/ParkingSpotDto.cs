namespace SmartParkingLot.Application.DTOs.Responses
{
    public class ParkingSpotDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; 
        public Guid? OccupyingDeviceId { get; set; }
        public DateTime? LastStatusChangeUtc { get; set; }
    }
}
