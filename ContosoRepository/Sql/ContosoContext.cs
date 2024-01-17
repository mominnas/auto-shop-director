﻿using Contoso.Models;
using Microsoft.EntityFrameworkCore;

namespace Contoso.Repository.Sql
{
    /// <summary>
    /// Entity Framework Core DbContext for Contoso.
    /// </summary>
    public class ContosoContext : DbContext
    {
        /// <summary>
        /// Creates a new Contoso DbContext.
        /// </summary>
        public ContosoContext(DbContextOptions<ContosoContext> options) : base(options)
        { }

        /// <summary>
        /// Gets the customers DbSet.
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Gets the orders DbSet.
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// Gets the products DbSet.
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Gets the line items DbSet.
        /// </summary>
        public DbSet<LineItem> LineItems { get; set; }
    }
}
