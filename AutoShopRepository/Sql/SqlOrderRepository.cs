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
            var existing = await _db.Orders
                .Include(o => o.LineItems)
                    .ThenInclude(l => l.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(_order => _order.Id == order.Id);

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


            // Handle Products in LineItems first
            foreach (var lineItem in order.LineItems)
            {
                if (lineItem.Product != null)
                {
                    // Attach existing product instead of trying to create a new one
                    var product = await _db.Products.FindAsync(lineItem.Product.Id);
                    if (product != null)
                    {
                        lineItem.Product = product;
                        lineItem.ProductId = product.Id;
                    }
                }
            }

            if (existing == null)
            {
                // New order
                order.InvoiceNumber = _db.Orders.Max(_order => _order.InvoiceNumber) + 1;
                _db.Orders.Add(order);
            }
            else
            {
                // Update existing order
                _db.Entry(existing).CurrentValues.SetValues(order);

                // Handle LineItems
                // Remove deleted line items
                foreach (var existingItem in existing.LineItems.ToList())
                {
                    if (!order.LineItems.Any(l => l.Id == existingItem.Id))
                    {
                        _db.LineItems.Remove(existingItem);
                    }
                }

                // Update and add line items
                foreach (var lineItem in order.LineItems)
                {
                    var existingItem = existing.LineItems.FirstOrDefault(l => l.Id == lineItem.Id);
                    if (existingItem != null)
                    {
                        // Update existing line item
                        _db.Entry(existingItem).CurrentValues.SetValues(lineItem);
                        if (lineItem.Product != null)
                        {
                            // Use reference to existing product
                            var product = await _db.Products.FindAsync(lineItem.Product.Id);
                            existingItem.Product = product;
                            existingItem.ProductId = product.Id;
                        }
                    }
                    else
                    {
                        // Add new line item
                        if (lineItem.Product != null)
                        {
                            // Use reference to existing product
                            var product = await _db.Products.FindAsync(lineItem.Product.Id);
                            lineItem.Product = product;
                            lineItem.ProductId = product.Id;
                        }

                        lineItem.OrderId = existing.Id;
                        existing.LineItems.Add(lineItem);
                        _db.LineItems.Add(lineItem);
                    }
                }
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
