using MMN.Models;

namespace MMN.Repository
{
    /// <summary>
    /// Defines methods for interacting with the app backend.
    /// </summary>
    public interface IContosoRepository
    {
        /// <summary>
        /// Returns the customers repository.
        /// </summary>
        ICustomerRepository Customers { get; }

        /// <summary>
        /// Returns the orders repository.
        /// </summary>
        IOrderRepository Orders { get; }

        /// <summary>
        /// Returns the products repository.
        /// </summary>
        IProductRepository Products { get;  }
    }
}
