namespace SmartParkingLot.Application.Interfaces
{
    public interface IRateLimiterService
    {
        Task<bool> IsActionAllowedAsync(Guid deviceId, string actionKey);
    }
}
