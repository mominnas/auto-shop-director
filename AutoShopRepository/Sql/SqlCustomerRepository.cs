﻿using MMN.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMN.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the customers backend using 
    /// SQL via Entity Framework Core 2.0.
    /// </summary>
    public class SqlCustomerRepository : ICustomerRepository
    {
        private readonly ContosoContext _db; 

        public SqlCustomerRepository(ContosoContext db)
        {
            _db = db; 
        }

        public async Task<IEnumerable<Customer>> GetAsync()
        {
            return await _db.Customers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Customer> GetAsync(Guid id)
        {
            return await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(customer => customer.Id == id);
        }

        public async Task<IEnumerable<Customer>> GetAsync(string value)
        {
            string[] parameters = value.Split(' ');
            return await _db.Customers
                .Where(customer =>
                    parameters.Any(parameter =>
                        customer.FirstName.StartsWith(parameter) ||
                        customer.LastName.StartsWith(parameter) ||
                        customer.Email.StartsWith(parameter) ||
                        customer.Phone.StartsWith(parameter) ||
                        customer.Address.StartsWith(parameter)))
                .OrderByDescending(customer =>
                    parameters.Count(parameter =>
                        customer.FirstName.StartsWith(parameter) ||
                        customer.LastName.StartsWith(parameter) ||
                        customer.Email.StartsWith(parameter) ||
                        customer.Phone.StartsWith(parameter) ||
                        customer.Address.StartsWith(parameter)))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Customer> UpsertAsync(Customer customer)
        {
            // For new customers, ensure they have a valid Id
            if (customer.Id == Guid.Empty)
            {
                customer.Id = Guid.NewGuid();
            }

            var current = await _db.Customers.FirstOrDefaultAsync(_customer => _customer.Id == customer.Id);
            if (current == null)
            {
                // Add the new customer
                _db.Customers.Add(customer);
            }
            else
            {
                // Attach the existing customer to the context and update its values
                _db.Entry(current).CurrentValues.SetValues(customer);
            }

            await _db.SaveChangesAsync();
            return customer;
        }

        public async Task DeleteAsync(Guid id)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(_customer => _customer.Id == id);
            if (null != customer)
            {
                var orders = await _db.Orders.Where(order => order.CustomerId == id).ToListAsync();
                _db.Orders.RemoveRange(orders);
                _db.Customers.Remove(customer);
                await _db.SaveChangesAsync();
            }
        }
    }
}
