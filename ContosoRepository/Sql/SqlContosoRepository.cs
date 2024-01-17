using Contoso.Models;
using Microsoft.EntityFrameworkCore;

namespace Contoso.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the app backend using 
    /// SQL via Entity Framework Core 6.0. 
    /// </summary>
    public class SqlContosoRepository : IContosoRepository
    {
        private readonly DbContextOptions<ContosoContext> _dbOptions; 

        public SqlContosoRepository(DbContextOptionsBuilder<ContosoContext> 
            dbOptionsBuilder)
        {
            _dbOptions = dbOptionsBuilder.Options;
            using (var db = new ContosoContext(_dbOptions))
            {
                db.Database.EnsureCreated(); 
            }
        }

        public ICustomerRepository Customers => new SqlCustomerRepository(
            new ContosoContext(_dbOptions));

        public IOrderRepository Orders => new SqlOrderRepository(
            new ContosoContext(_dbOptions));

        public IProductRepository Products => new SqlProductRepository(
            new ContosoContext(_dbOptions));
    }
}
