using Contoso.Models;

namespace Contoso.Repository.Rest
{
    /// <summary>
    /// Contains methods for interacting with the app backend using REST. 
    /// </summary>
    public class RestContosoRepository : IContosoRepository
    {
        private readonly string _url; 
        private readonly string _accessToken;

        public RestContosoRepository(string url, string accessToken)
        {
            _url = url;
            _accessToken = accessToken;
        }

        public ICustomerRepository Customers => new RestCustomerRepository(_url, _accessToken); 

        public IOrderRepository Orders => new RestOrderRepository(_url, _accessToken);

        public IProductRepository Products => new RestProductRepository(_url, _accessToken); 
    }
}
