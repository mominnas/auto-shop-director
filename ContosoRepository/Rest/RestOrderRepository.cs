using Contoso.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Contoso.Repository.Rest
{
    /// <summary>
    /// Contains methods for interacting with the orders backend using REST. 
    /// </summary>
    public class RestOrderRepository : IOrderRepository
    {
        private readonly HttpHelper _http;
        private readonly string _accessToken;

        public RestOrderRepository(string baseUrl, string accessToken)
        {
            _http = new HttpHelper(baseUrl);
            _accessToken = accessToken;
        }

        public async Task<IEnumerable<Order>> GetAsync() =>
            await _http.GetAsync<IEnumerable<Order>>("order", _accessToken);

        public async Task<Order> GetAsync(Guid id) =>
            await _http.GetAsync<Order>($"order/{id}", _accessToken);

        public async Task<IEnumerable<Order>> GetForCustomerAsync(Guid customerId) =>
            await _http.GetAsync<IEnumerable<Order>>($"order/customer/{customerId}", _accessToken);

        public async Task<IEnumerable<Order>> GetAsync(string search) =>
            await _http.GetAsync<IEnumerable<Order>>($"order/search?value={search}", _accessToken);

        public async Task<Order> UpsertAsync(Order order) =>
            await _http.PostAsync<Order, Order>("order", order, _accessToken);

        public async Task DeleteAsync(Guid orderId) =>
            await _http.DeleteAsync("order", orderId, _accessToken);
    }
}
