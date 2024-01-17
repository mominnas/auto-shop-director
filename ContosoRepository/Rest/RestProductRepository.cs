using Contoso.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contoso.Repository.Rest
{
    /// <summary>
    /// Contains methods for interacting with the products backend using REST. 
    /// </summary>
    public class RestProductRepository : IProductRepository
    {
        private readonly HttpHelper _http;
        private readonly string _accessToken;

        public RestProductRepository(string baseUrl, string accessToken)
        {
            _http = new HttpHelper(baseUrl);
            _accessToken = accessToken;
        }

        public async Task<IEnumerable<Product>> GetAsync() =>
            await _http.GetAsync<IEnumerable<Product>>("product", _accessToken); 

        public async Task<Product> GetAsync(Guid id) => 
            await _http.GetAsync<Product>($"product/{id}", _accessToken);

        public async Task<IEnumerable<Product>> GetAsync(string search) =>
            await _http.GetAsync<IEnumerable<Product>>($"product/search?value={search}", _accessToken);
    }
}
