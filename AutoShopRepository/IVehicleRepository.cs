using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMN.Models
{
    /// <summary>
    /// Defines methods for interacting with the vehicles backend.
    /// </summary>
    public interface IVehicleRepository
    {
        /// <summary>
        /// Returns all vehicles.
        /// </summary>
        Task<IEnumerable<Vehicle>> GetAsync();

        /// <summary>
        /// Returns the vehicle with the given Id.
        /// </summary>
        Task<Vehicle> GetAsync(Guid id);

        /// <summary>
        /// Returns all vehicles with a data field matching the start of the given string.
        /// </summary>
        Task<IEnumerable<Vehicle>> GetAsync(string search);

        /// <summary>
        /// Returns all vehicles for the given customer.
        /// </summary>
        Task<IEnumerable<Vehicle>> GetForCustomerAsync(Guid customerId);

        /// <summary>
        /// Adds a new vehicle if the vehicle does not exist, updates the
        /// existing vehicle otherwise.
        /// </summary>
        Task<Vehicle> UpsertAsync(Vehicle vehicle);

        /// <summary>
        /// Deletes a vehicle.
        /// </summary>
        Task DeleteAsync(Guid vehicleId);
    }
}