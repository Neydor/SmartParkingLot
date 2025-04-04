using SmartParkingLot.Application.Interfaces;
using System.Collections.Concurrent;

namespace SmartParkingLot.Application.Services
{
    public class RateLimiterService: IRateLimiterService
    {
        private readonly ConcurrentDictionary<(Guid deviceId, string actionKey), DateTime> _lastActionTimestamps = new();
        private readonly TimeSpan _rateLimitWindow = TimeSpan.FromSeconds(10); 

        public Task<bool> IsActionAllowedAsync(Guid deviceId, string actionKey)
        {
            var now = DateTime.UtcNow;
            var key = (deviceId, actionKey);

            if (_lastActionTimestamps.TryGetValue(key, out var lastActionTime))
            {
                if (now - lastActionTime < _rateLimitWindow)
                {
                    return Task.FromResult(false); 
                }
            }

            _lastActionTimestamps[key] = now;
            return Task.FromResult(true); 
        }
    }
}
