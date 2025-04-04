namespace SmartParkingLot.Domain.Entities
{
    public class Device
    {
        public Guid Id { get; private set; }
        private Device() { }
        public Device(Guid id)
        {
            Id = id;
        }
    }
}
