using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;

namespace MMN.App.ViewModels
{
    /// <summary>
    /// Provides data and commands accessible to the entire app.  
    /// </summary>
    public class MainViewModel : BindableBase
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Creates a new MainViewModel.
        /// </summary>
        public MainViewModel() => Task.Run(GetCustomerListAsync);

        /// <summary>
        /// The collection of customers in the list. 
        /// </summary>
        public ObservableCollection<CustomerViewModel> Customers { get; }
            = new ObservableCollection<CustomerViewModel>();







        public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();

        private ProductViewModel _selectedProduct;

        public ProductViewModel SelectedProduct
        {
            get => _selectedProduct;
            set => Set(ref _selectedProduct, value);
        }

        public async Task GetProductListAsync()
        {
            IsLoading = true;
            var products = await App.Repository.Products.GetAsync();
            if (products == null)
            {
                IsLoading = false;
                return;
            }

            _ = dispatcherQueue.EnqueueAsync(() =>
            {
                Products.Clear();
                foreach (var p in products)
                {
                    Products.Add(new ProductViewModel(p));
                }
                IsLoading = false;
            });
        }

        public void SyncProducts()
        {
            Task.Run(async () =>
            {
                foreach (var modifiedProduct in Products
                    .Where(product => product.IsModified).Select(product => product.Model))
                {
                    await App.Repository.Products.UpsertAsync(modifiedProduct);
                }

                await GetProductListAsync();
            });
        }





        private CustomerViewModel _selectedCustomer;

        /// <summary>
        /// Gets or sets the selected customer, or null if no customer is selected. 
        /// </summary>
        public CustomerViewModel SelectedCustomer
        {
            get => _selectedCustomer;
            set => Set(ref _selectedCustomer, value);
        }

        private bool _isLoading = false;

        /// <summary>
        /// Gets or sets a value indicating whether the Customers list is currently being updated. 
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading; 
            set => Set(ref _isLoading, value);
        }

        /// <summary>
        /// Gets the complete list of customers from the database.
        /// </summary>
        public async Task GetCustomerListAsync()
        {
            await dispatcherQueue.EnqueueAsync(() => IsLoading = true);

            var customers = await App.Repository.Customers.GetAsync();
            if (customers == null)
            {
                return;
            }

            await dispatcherQueue.EnqueueAsync(() =>
            {
                Customers.Clear();
                foreach (var c in customers)
                {
                    Customers.Add(new CustomerViewModel(c));
                }
                IsLoading = false;
            });
        }

        /// <summary>
        /// Saves any modified customers and reloads the customer list from the database.
        /// </summary>
        public void Sync()
        {
            Task.Run(async () =>
            {
                foreach (var modifiedCustomer in Customers
                    .Where(customer => customer.IsModified).Select(customer => customer.Model))
                {
                    await App.Repository.Customers.UpsertAsync(modifiedCustomer);
                }

                await GetCustomerListAsync();
            });
        }
    }
}
