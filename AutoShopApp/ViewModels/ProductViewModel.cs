﻿using MMN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.ComponentModel;
using System.Collections.Specialized;


namespace MMN.App.ViewModels
{
    /// <summary>
    /// Provides a bindable wrapper for the Product model class, encapsulating various services for access by the UI.
    /// </summary>
    public class ProductViewModel : BindableBase
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Initializes a new instance of the ProductViewModel class that wraps the specified Product object.
        /// </summary>
        /// <param name="model">The product to wrap.</param>
        public ProductViewModel(Product model = null)
        {
            Model = model ?? new Product();

        }


        /// <summary>
        /// Gets the products to display.
        /// </summary>
        public ObservableCollection<Product> Products { get; private set; } = new ObservableCollection<Product>();


        /// <summary>
        /// Gets the underlying Product object.
        /// </summary>
        public Product Model { get; set; }

        /// <summary>
        /// Gets or sets the product's ID.
        /// </summary>
        public Guid Id
        {
            get => Model.Id;
            set
            {
                if (Model.Id != value)
                {
                    Model.Id = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the product's name.
        /// </summary>
        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name != value)
                {
                    Model.Name = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }



        /// <summary>
        /// Gets a value that specifies whether the user can revert changes. 
        /// </summary>
        public bool CanRevert => Model != null && IsModified;


        public bool IsLoaded => Model != null;

        bool _IsModified = false;

        /// <summary>
        /// Gets or sets a value that indicates whether the underlying model has been modified. 
        /// </summary>
        public bool IsModified
        {
            get => _IsModified;
            set
            {
                if (value != _IsModified)
                {
                    // Only record changes after the order has loaded. 
                    if (IsLoaded)
                    {
                        _IsModified = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(CanRevert));
                    }
                }
            }
        }

        public decimal ListPrice
        {
            get => Model.ListPrice;
            set
            {
                if (Model.ListPrice != value)
                {
                    Model.ListPrice = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }


        public decimal StandardCost
        {
            get => Model.StandardCost;
            set
            {
                if (Model.StandardCost != value)
                {
                    Model.StandardCost = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }

        public string Color
        {
            get => Model.Color;
            set
            {
                if (Model.Color != value)
                {
                    Model.Color = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }

        public int DaysToManufacture
        {
            get => Model.DaysToManufacture;
            set
            {
                if (Model.DaysToManufacture != value)
                {
                    Model.DaysToManufacture = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }

        public decimal Weight
        {
            get => Model.Weight;
            set
            {
                if (Model.Weight != value)
                {
                    Model.Weight = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }


        public string Description
        {
            get => Model.Description;
            set
            {
                if (Model.Description != value)
                {
                    Model.Description = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }



        private Product _selectedProduct;

        /// <summary>
        /// Gets or sets the selected product.
        /// </summary>
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (Set(ref _selectedProduct, value))
                {
                    if (_selectedProduct != null)
                    {
                        Task.Run(() => LoadAsync(_selectedProduct.Id));
                    }
                    OnPropertyChanged(nameof(SelectedProduct));
                }
            }
        }


        /// <summary>
        /// Creates an OrderViewModel that wraps an Order object created from the specified ID.
        /// </summary>
        public async static Task<ProductViewModel> CreateFromGuid(Guid productId) =>
            new ProductViewModel(await GetProduct(productId));



        /// <summary>
        /// Returns the product with the specified ID.
        /// </summary>
        private static async Task<Product> GetProduct(Guid productId) =>
            await App.Repository.Products.GetAsync(productId);



        /// <summary>
        /// Saves the current product to the database.
        /// </summary>
        public async Task SaveProductAsync()
        {
            Product result = null;
            try
            {
                result = await App.Repository.Products.UpsertAsync(Model);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to save. There might have been a problem connecting to the database. Please try again.", ex);
            }

            if (result != null)
            {
                await dispatcherQueue.EnqueueAsync(() => IsModified = false);
            }
            else
            {
                await dispatcherQueue.EnqueueAsync(() => new ProductSavingException(
                    "Unable to save. There might have been a problem " +
                    "connecting to the database. Please try again."));
            }

        }

        /// <summary>
        /// Loads the product details from the database.
        /// </summary>
        public async Task LoadAsync(Guid productId)
        {
            try
            {
                var product = await App.Repository.Products.GetAsync(productId);
                if (product != null)
                {
                    Model = product;
                    OnPropertyChanged(string.Empty); // Refresh all properties
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to load. There might have been a problem connecting to the database. Please try again.", ex);
            }
        }

        /// <summary>
        /// Loads the product details from the database.
        /// </summary>
        public async Task LoadProducts()
        {
            try
            {
                var products = await App.Repository.Products.GetAsync();
                if (products != null)
                {
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to load. There might have been a problem connecting to the database. Please try again.", ex);
            }
        }
    }
}
