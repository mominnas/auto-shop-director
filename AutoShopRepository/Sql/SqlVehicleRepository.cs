using MMN.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMN.Repository.Sql
{
    public class SqlVehicleRepository : IVehicleRepository
    {
        private readonly ContosoContext _db;

        public SqlVehicleRepository(ContosoContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Vehicle>> GetForCustomerAsync(Guid customerId)
        {
            return await _db.Vehicles
                .Where(vehicle => vehicle.CustomerId == customerId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Vehicle> GetAsync(Guid id)
        {
            return await _db.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(vehicle => vehicle.Id == id);
        }

        public async Task<IEnumerable<Vehicle>> GetAsync()
        {
            return await _db.Vehicles
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<IEnumerable<Vehicle>> GetAsync(string value)
        {
            return await _db.Vehicles.Where(vehicle =>
                vehicle.Make.StartsWith(value) ||
                vehicle.Model.StartsWith(value) ||
                vehicle.Year.ToString().StartsWith(value) ||
                vehicle.VIN.ToString().StartsWith(value))
            .AsNoTracking()
            .ToListAsync();
        }

        public async Task<Vehicle> UpsertAsync(Vehicle vehicle)
        {
            if (vehicle.Id == Guid.Empty)
            {
                vehicle.Id = Guid.NewGuid();
                _db.Vehicles.Add(vehicle);
            }
            else
            {
                var existing = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicle.Id);
                if (existing != null)
                {
                    _db.Entry(existing).CurrentValues.SetValues(vehicle);
                }
                else
                {
                    // If the vehicle does not exist, add it as a new entry
                    _db.Vehicles.Add(vehicle);
                }
            }

            await _db.SaveChangesAsync();
            return vehicle;
        }

        public async Task DeleteAsync(Guid id)
        {
            var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle != null)
            {
                _db.Vehicles.Remove(vehicle);
                await _db.SaveChangesAsync();
            }
        }
    }
}