using MMN.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMN.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the orders backend using 
    /// SQL via Entity Framework Core 2.0.
    /// </summary>
    public class SqlOrderRepository : IOrderRepository
    {
        private readonly ContosoContext _db; 

        public SqlOrderRepository(ContosoContext db) => _db = db;

        public async Task<IEnumerable<Order>> GetAsync() =>
            await _db.Orders
                .Include(order => order.LineItems)
                .ThenInclude(lineItem => lineItem.Product)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Order> GetAsync(Guid id) =>
            await _db.Orders
                .Include(order => order.LineItems)
                .ThenInclude(lineItem => lineItem.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.Id == id);

        public async Task<IEnumerable<Order>> GetForCustomerAsync(Guid id) =>
            await _db.Orders
                .Where(order => order.CustomerId == id)
                .Include(order => order.LineItems)
                .ThenInclude(lineItem => lineItem.Product)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetAsync(string value)
        {
            string[] parameters = value.Split(' ');
            return await _db.Orders
                .Include(order => order.Customer)
                .Include(order => order.LineItems)
                .ThenInclude(lineItem => lineItem.Product)
                .Where(order => parameters
                    .Any(parameter =>
                        order.Address.StartsWith(parameter) ||
                        order.Customer.FirstName.StartsWith(parameter) ||
                        order.Customer.LastName.StartsWith(parameter) ||
                        order.InvoiceNumber.ToString().StartsWith(parameter)))
                .OrderByDescending(order => parameters
                    .Count(parameter =>
                        order.Address.StartsWith(parameter) ||
                        order.Customer.FirstName.StartsWith(parameter) ||
                        order.Customer.LastName.StartsWith(parameter) ||
                        order.InvoiceNumber.ToString().StartsWith(parameter)))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order> UpsertAsync(Order order)
        {
            // Ensure order has a valid customer reference
            if (order.Customer != null)
            {
                // Check if customer exists in the current context
                var existingCustomer = await _db.Customers
                    .FirstOrDefaultAsync(c => c.Id == order.Customer.Id);

                if (existingCustomer != null)
                {
                    // Use the existing customer reference from the context
                    order.Customer = existingCustomer;
                    order.CustomerId = existingCustomer.Id;
                }
            }



            // Ensure line items have valid product references
            if (order.LineItems != null)
            {
                foreach (var lineItem in order.LineItems)
                {
                    if (lineItem.Product != null)
                    {
                        var existingProduct = await _db.Products
                            .FirstOrDefaultAsync(p => p.Id == lineItem.Product.Id);

                        if (existingProduct != null)
                        {
                            // Use the existing product reference
                            lineItem.Product = existingProduct;
                            lineItem.ProductId = existingProduct.Id;
                            lineItem.Order = order;
                            lineItem.OrderId = order.Id;
                            lineItem.Quantity = lineItem.Quantity;
                        }
                    }
                }
            }



            var existing = await _db.Orders.FirstOrDefaultAsync(_order => _order.Id == order.Id);

            if (null == existing)
            {
                // New order
                order.InvoiceNumber = _db.Orders.Max(_order => _order.InvoiceNumber) + 1;
                _db.Orders.Add(order);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(order);
            }

            await _db.SaveChangesAsync();
            return order;
        }

        public async Task DeleteAsync(Guid orderId)
        {
            var match = await _db.Orders.FirstOrDefaultAsync(_order => _order.Id == orderId);
            if (match != null)
            {
                _db.Orders.Remove(match);
            }
            await _db.SaveChangesAsync();
        }
    }
}
