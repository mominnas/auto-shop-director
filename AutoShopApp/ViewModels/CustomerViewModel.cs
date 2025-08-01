﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using MMN.Models;


namespace MMN.App.ViewModels
{
    /// <summary>
    /// Provides a bindable wrapper for the Customer model class, encapsulating various services for access by the UI.
    /// </summary>
    public class CustomerViewModel : BindableBase, IEditableObject
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Initializes a new instance of the CustomerViewModel class that wraps a Customer object.
        /// </summary>
        public CustomerViewModel(Customer model = null) => Model = model ?? new Customer();

        private Customer _model;

        /// <summary>
        /// Gets the collection of vehicles owned by the customer.
        /// </summary>
        public ObservableCollection<VehicleViewModel> Vehicles { get; } = new ObservableCollection<VehicleViewModel>();

        /// <summary>
        /// Loads the vehicles for the customer.
        /// </summary>
        public async Task LoadVehiclesAsync()
        {
            var vehicles = await App.Repository.Vehicles.GetForCustomerAsync(Model.Id);
            Vehicles.Clear();
            foreach (var vehicle in vehicles)
            {
                Vehicles.Add(new VehicleViewModel(vehicle));
            }
        }

        private VehicleViewModel _selectedVehicle;
        /// <summary>
        /// Gets or sets the selected vehicle for the customer.
        /// </summary>
        public VehicleViewModel SelectedVehicle
        {
            get => _selectedVehicle;
            set => Set(ref _selectedVehicle, value);
        }

        /// <summary>
        /// Gets or sets the underlying Customer object.
        /// </summary>
        public Customer Model
        {
            get => _model;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Model cannot be null.");
                }

                if (_model != value)
                {
                    _model = value;
                    RefreshOrders();

                    // Raise the PropertyChanged event for all properties.
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's first name.
        /// </summary>
        public string FirstName
        {
            get => Model?.FirstName ?? string.Empty;
            set
            {
                if (value != Model.FirstName)
                {
                    Model.FirstName = value;
                    IsModified = true;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's last name.
        /// </summary>
        public string LastName
        {
            get => Model?.LastName ?? string.Empty;
            set
            {
                if (value != Model.LastName)
                {
                    Model.LastName = value;
                    IsModified = true;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// Gets the customers full (first + last) name.
        /// </summary>
        public string Name => IsNewCustomer && string.IsNullOrEmpty(FirstName)
            && string.IsNullOrEmpty(LastName) ? "New customer" : $"{FirstName} {LastName}";

        /// <summary>
        /// Gets or sets the customer's address.
        /// </summary>
        public string Address
        {
            get => Model?.Address ?? string.Empty; // Safely handle null values
            set
            {
                if (value != Model.Address)
                {
                    Model.Address = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's company.
        /// </summary>
        public string Company
        {
            get => Model?.Company ?? string.Empty;
            set
            {
                if (value != Model.Company)
                {
                    Model.Company = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's phone number. 
        /// </summary>
        public string Phone
        {
            get => Model?.Phone ?? string.Empty;
            set
            {
                if (value != Model.Phone)
                {
                    Model.Phone = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer's email. 
        /// </summary>
        public string Email
        {
            get => Model?.Email ?? string.Empty;
            set
            {
                if (value != Model.Email)
                {
                    Model.Email = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the underlying model has been modified. 
        /// </summary>
        /// <remarks>
        /// Used when sync'ing with the server to reduce load and only upload the models that have changed.
        /// </remarks>
        public bool IsModified { get; set; }

        /// <summary>
        /// Gets the collection of the customer's orders.
        /// </summary>
        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        private Order _selectedOrder;

        /// <summary>
        /// Gets or sets the currently selected order.
        /// </summary>
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => Set(ref _selectedOrder, value);
        }

        private bool _isLoading;

        /// <summary>
        /// Gets or sets a value that indicates whether to show a progress bar. 
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private bool _isNewCustomer;

        /// <summary>
        /// Gets or sets a value that indicates whether this is a new customer.
        /// </summary>
        public bool IsNewCustomer
        {
            get => _isNewCustomer;
            set => Set(ref _isNewCustomer, value);
        }

        private bool _isInEdit = false;

        /// <summary>
        /// Gets or sets a value that indicates whether the customer data is being edited.
        /// </summary>
        public bool IsInEdit
        {
            get => _isInEdit;
            set => Set(ref _isInEdit, value);
        }

        /// <summary>
        /// Saves customer data that has been edited.
        /// </summary>
        public async Task SaveAsync()
        {
            IsInEdit = false;
            IsModified = false;

            if (IsNewCustomer)
            {
                IsNewCustomer = false;
                App.ViewModel.Customers.Add(this);
            }

            // Save the customer to the database
            await App.Repository.Customers.UpsertAsync(Model);
        }

        /// <summary>
        /// Raised when the user cancels the changes they've made to the customer data.
        /// </summary>
        public event EventHandler AddNewCustomerCanceled;

        /// <summary>
        /// Cancels any in progress edits.
        /// </summary>
        public async Task CancelEditsAsync()
        {
            if (IsNewCustomer)
            {
                AddNewCustomerCanceled?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await RevertChangesAsync();
            }
        }

        /// <summary>
        /// Discards any edits that have been made, restoring the original values.
        /// </summary>
        public async Task RevertChangesAsync()
        {
            IsInEdit = false;
            if (IsModified)
            {
                await RefreshCustomerAsync();
                IsModified = false;
            }
        }

        /// <summary>
        /// Enables edit mode.
        /// </summary>
        public void StartEdit() => IsInEdit = true;

        /// <summary>
        /// Reloads all of the customer data.
        /// </summary>
        public async Task RefreshCustomerAsync()
        {
            RefreshOrders();
            Model = await App.Repository.Customers.GetAsync(Model.Id);
        }

        /// <summary>
        /// Resets the customer detail fields to the current values.
        /// </summary>
        public void RefreshOrders() => Task.Run(LoadOrdersAsync);

        /// <summary>
        /// Loads the order data for the customer.
        /// </summary>
        public async Task LoadOrdersAsync()
        {
            await dispatcherQueue.EnqueueAsync(() =>
            {
                IsLoading = true;
            });

            var orders = await App.Repository.Orders.GetForCustomerAsync(Model.Id);

            await dispatcherQueue.EnqueueAsync(() =>
            {
                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                IsLoading = false;
            });
        }

        /// <summary>
        /// Called when a bound DataGrid control causes the customer to enter edit mode.
        /// </summary>
        public void BeginEdit()
        {
            // Not used.
        }

        /// <summary>
        /// Called when a bound DataGrid control cancels the edits that have been made to a customer.
        /// </summary>
        public async void CancelEdit() => await CancelEditsAsync();

        /// <summary>
        /// Called when a bound DataGrid control commits the edits that have been made to a customer.
        /// </summary>
        public async void EndEdit() => await SaveAsync();
    }
}