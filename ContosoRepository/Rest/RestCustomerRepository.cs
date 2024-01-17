using Contoso.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Contoso.Repository.Rest
{
    /// <summary>
    /// Contains methods for interacting with the customers backend using REST. 
    /// </summary>
    public class RestCustomerRepository : ICustomerRepository
    {
        private readonly HttpHelper _http;
        private readonly string _accessToken;

        public RestCustomerRepository(string baseUrl, string accessToken)
        {
            _http = new HttpHelper(baseUrl);
            _accessToken = accessToken;
        }

        public async Task<IEnumerable<Customer>> GetAsync() =>
            await _http.GetAsync<IEnumerable<Customer>>("customer", _accessToken);

        public async Task<IEnumerable<Customer>> GetAsync(string search) => 
            await _http.GetAsync<IEnumerable<Customer>>($"customer/search?value={search}", _accessToken);

        public async Task<Customer> GetAsync(Guid id) =>
            await _http.GetAsync<Customer>($"customer/{id}", _accessToken);

        public async Task<Customer> UpsertAsync(Customer customer) => 
            await _http.PostAsync<Customer, Customer>("customer", customer, _accessToken);

        public async Task DeleteAsync(Guid customerId) => 
            await _http.DeleteAsync("customer", customerId, _accessToken);
    }
}
