using MMN.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMN.Repository.Rest
{
    /// <summary>
    /// Contains methods for interacting with the vehicles backend using REST.
    /// </summary>
    public class RestVehicleRepository : IVehicleRepository
    {
        private readonly HttpHelper _http;
        private readonly string _accessToken;

        public RestVehicleRepository(string baseUrl, string accessToken)
        {
            _http = new HttpHelper(baseUrl);
            _accessToken = accessToken;
        }

        public async Task<IEnumerable<Vehicle>> GetAsync() =>
            await _http.GetAsync<IEnumerable<Vehicle>>("vehicle", _accessToken);

        public async Task<Vehicle> GetAsync(Guid id) =>
            await _http.GetAsync<Vehicle>($"vehicle/{id}", _accessToken);

        public async Task<IEnumerable<Vehicle>> GetAsync(string search) =>
            await _http.GetAsync<IEnumerable<Vehicle>>($"vehicle/search?value={search}", _accessToken);

        public async Task<IEnumerable<Vehicle>> GetForCustomerAsync(Guid customerId) =>
            await _http.GetAsync<IEnumerable<Vehicle>>($"vehicle/customer/{customerId}", _accessToken);

        public async Task<Vehicle> UpsertAsync(Vehicle vehicle) =>
            await _http.PostAsync<Vehicle, Vehicle>("vehicle", vehicle, _accessToken);

        public async Task DeleteAsync(Guid vehicleId) =>
            await _http.DeleteAsync("vehicle", vehicleId, _accessToken);
    }
}