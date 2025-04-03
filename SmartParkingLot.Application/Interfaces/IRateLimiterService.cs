using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParkingLot.Application.Interfaces
{
    public interface IRateLimiter
    {
        Task<bool> IsActionAllowedAsync(Guid deviceId, string actionKey);
    }
}
