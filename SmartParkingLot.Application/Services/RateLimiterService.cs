using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParkingLot.Application.Services
{
    public class RateLimiterService: IRateLimiterService
    {
        // Store last action time per device per action type
        private readonly ConcurrentDictionary<(Guid deviceId, string actionKey), DateTime> _lastActionTimestamps = new();
        private readonly TimeSpan _rateLimitWindow = TimeSpan.FromSeconds(10); // e.g., 1 action per 10 seconds

        public Task<bool> IsActionAllowedAsync(Guid deviceId, string actionKey)
        {
            var now = DateTime.UtcNow;
            var key = (deviceId, actionKey);

            if (_lastActionTimestamps.TryGetValue(key, out var lastActionTime))
            {
                if (now - lastActionTime < _rateLimitWindow)
                {
                    return Task.FromResult(false); // Rate limit exceeded
                }
            }

            // Update or add the timestamp for the current action
            _lastActionTimestamps[key] = now;
            return Task.FromResult(true); // Action allowed
        }
    }
}
