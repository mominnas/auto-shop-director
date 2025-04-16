using MMN.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMN.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the products backend using 
    /// SQL via Entity Framework Core 2.0.
    /// </summary>
    public class SqlProductRepository : IProductRepository
    {
        private readonly ContosoContext _db;

        public SqlProductRepository(ContosoContext db)
        {
            _db = db; 
        }

        public async Task<IEnumerable<Product>> GetAsync()
        {
            return await _db.Products
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product> GetAsync(Guid id)
        {
            return await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAsync(string value)
        {
            return await _db.Products.Where(product =>
                product.Name.StartsWith(value) ||
                product.Color.StartsWith(value) ||
                product.DaysToManufacture.ToString().StartsWith(value) ||
                product.StandardCost.ToString().StartsWith(value) ||
                product.ListPrice.ToString().StartsWith(value) ||
                product.Weight.ToString().StartsWith(value) ||
                product.Description.StartsWith(value))
            .AsNoTracking()
            .ToListAsync();
        }

        public async Task<Product> UpsertAsync(Product product)
        {
            var existing = _db.Products
                .FirstOrDefault(p => p.Id == product.Id);
            if (null == existing)
            {
                _db.Products.Add(product);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(product);
            }
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(Guid productId)
        {
            var match = await _db.Products.FirstOrDefaultAsync(product => product.Id == productId);
            if (null != match)
            {
                _db.Products.Remove(match);
            }
            await _db.SaveChangesAsync();
        }

    }
}
