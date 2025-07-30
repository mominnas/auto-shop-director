using System;
using System.Collections.Generic;
using System.Linq;

namespace MMN.Models
{
    /// <summary>
    /// Represents a customer's vehicle.
    /// </summary>
    public class Vehicle : DbObject
    {
        /// <summary>
        /// Gets or sets the make of the vehicle.
        /// </summary>
        public string Make { get; set; }

        /// <summary>
        /// Gets or sets the model of the vehicle.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the year of the vehicle.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the VIN (Vehicle Identification Number).
        /// </summary>
        public string VIN { get; set; }

        /// <summary>
        /// Gets or sets the customer who owns the vehicle.
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer who owns the vehicle.
        /// </summary>
        public Customer Customer { get; set; }
    }

}
