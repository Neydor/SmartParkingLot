using SmartParkingLot.Domain.Entities;
using SmartParkingLot.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParkingLot.Infraestructure.Repositories
{
    public class ParkingRepository : IParkingRepository
    {
        private readonly List<ParkingSpot> _spots = new();
        private readonly List<Device> _devices = new();
        private readonly object _lock = new(); // Para thread-safety básico

        public ParkingSpot GetById(Guid id)
        {
            lock (_lock)
            {
                return _spots.FirstOrDefault(s => s.Id == id);
            }
        }

        public IEnumerable<ParkingSpot> GetAll()
        {
            lock (_lock)
            {
                return new List<ParkingSpot>(_spots); // Retorna copia para evitar modificaciones externas
            }
        }

        public void Add(ParkingSpot spot)
        {
            lock (_lock)
            {
                if (_spots.Any(s => s.Id == spot.Id))
                    throw new InvalidOperationException("El spot ya existe");

                _spots.Add(spot);
            }
        }

        public void Update(ParkingSpot spot)
        {
            lock (_lock)
            {
                var index = _spots.FindIndex(s => s.Id == spot.Id);
                if (index == -1)
                    throw new KeyNotFoundException("Spot no encontrado");

                _spots[index] = spot;
            }
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                var spot = GetById(id);
                if (spot == null)
                    throw new KeyNotFoundException("Spot no encontrado");

                _spots.Remove(spot);
            }
        }

        public bool IsDeviceRegistered(Guid deviceId)
        {
            lock (_lock)
            {
                return _devices.Any(d => d.Id == deviceId);
            }
        }

        public PagedList<ParkingSpot> GetAllPaged(int pageNumber, int pageSize)
        {
            lock (_lock)
            {
                var query = _spots.AsQueryable();
                var count = query.Count();
                var items = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagedList<ParkingSpot>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = count,
                };
            }
        }

        // Método auxiliar para inicializar datos de prueba (opcional)
        public void SeedTestData()
        {
            lock (_lock)
            {
                // Agregar 10 spots libres
                for (int i = 0; i < 10; i++)
                {
                    _spots.Add(new ParkingSpot
                    {
                        Id = Guid.NewGuid(),
                        IsOccupied = false
                    });
                }

                // Registrar un dispositivo de prueba
                _devices.Add(new Device { Id = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000000") });
            }
        }
    }
}
