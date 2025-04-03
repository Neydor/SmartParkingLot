using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
