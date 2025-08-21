using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using MMN.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MMN.App.ViewModels
{
    /// <summary>
    /// Provides a bindable wrapper for the Vehicle model class, encapsulating various services for access by the UI.
    /// </summary>
    public class VehicleViewModel : BindableBase
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        private Vehicle _model;

        /// <summary>
        /// Initializes a new instance of the VehicleViewModel class that wraps the specified Vehicle object.
        /// </summary>
        /// <param name="model">The vehicle to wrap.</param>
        public VehicleViewModel(Vehicle model = null)
        {
            Model = model ?? new Vehicle();
        }

        /// <summary>
        /// Gets the vehicles to display.
        /// </summary>
        public ObservableCollection<Vehicle> Vehicles { get; private set; } = new ObservableCollection<Vehicle>();

        /// <summary>
        /// Gets or sets the underlying Vehicle object.
        /// </summary>
        public Vehicle Model
        {
            get => _model;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Model cannot be null.");

                if (_model != value)
                {
                    _model = value;
                    OnPropertyChanged(string.Empty); // Refresh all properties
                }
            }
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

        /// <summary>
        /// Gets or sets the vehicle's ID.
        /// </summary>
        public Guid Id
        {
            get => Model.Id;
            set
            {
                if (Model.Id != value)
                {
                    Model.Id = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vehicle's make.
        /// </summary>
        public string Make
        {
            get => Model.Make;
            set
            {
                if (Model.Make != value)
                {
                    Model.Make = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vehicle's model.
        /// </summary>
        public string ModelName
        {
            get => Model.Model;
            set
            {
                if (Model.Model != value)
                {
                    Model.Model = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vehicle's year.
        /// </summary>
        public int Year
        {
            get => Model.Year;
            set
            {
                if (Model.Year != value)
                {
                    Model.Year = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vehicle's VIN.
        /// </summary>
        public string VIN
        {
            get => Model.VIN;
            set
            {
                if (Model.VIN != value)
                {
                    Model.VIN = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer ID for this vehicle.
        /// </summary>
        public Guid CustomerId
        {
            get => Model.CustomerId;
            set
            {
                if (Model.CustomerId != value)
                {
                    Model.CustomerId = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a value that specifies whether the user can revert changes. 
        /// </summary>
        public bool CanRevert => Model != null && IsModified;

        public bool IsLoaded => Model != null;

        /// <summary>
        /// Gets or sets a value that indicates whether the underlying model has been modified. 
        /// </summary>
        public bool IsModified { get; set; }

        private Vehicle _selectedVehicle;

        /// <summary>
        /// Gets or sets the selected vehicle.
        /// </summary>
        public Vehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (Set(ref _selectedVehicle, value))
                {
                    if (_selectedVehicle != null)
                    {
                        Task.Run(() => LoadAsync(_selectedVehicle.Id));
                    }
                    OnPropertyChanged(nameof(SelectedVehicle));
                }
            }
        }

        /// <summary>
        /// Creates a VehicleViewModel that wraps a Vehicle object created from the specified ID.
        /// </summary>
        public async static Task<VehicleViewModel> CreateFromGuid(Guid vehicleId) =>
            new VehicleViewModel(await GetVehicle(vehicleId));

        /// <summary>
        /// Returns the vehicle with the specified ID.
        /// </summary>
        private static async Task<Vehicle> GetVehicle(Guid vehicleId) =>
            await App.Repository.Vehicles.GetAsync(vehicleId);

        /// <summary>
        /// Saves the current vehicle to the database.
        /// </summary>
        public async Task SaveVehicleAsync()
        {
            Vehicle result = null;
            try
            {
                result = await App.Repository.Vehicles.UpsertAsync(Model);

                var savedVehicle = await App.Repository.Vehicles.GetAsync(result.Id);
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
                await dispatcherQueue.EnqueueAsync(() => new Exception(
                    "Unable to save. There might have been a problem " +
                    "connecting to the database. Please try again."));
            }
        }

        /// <summary>
        /// Loads the vehicle details from the database.
        /// </summary>
        public async Task LoadAsync(Guid vehicleId)
        {
            try
            {
                var vehicle = await App.Repository.Vehicles.GetAsync(vehicleId);
                if (vehicle != null)
                {
                    Model = vehicle;
                    OnPropertyChanged(string.Empty); // Refresh all properties
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to load. There might have been a problem connecting to the database. Please try again.", ex);
            }
        }

        /// <summary>
        /// Loads the vehicles for a customer from the database.
        /// </summary>
        public async Task LoadVehicles(Guid customerId)
        {
            try
            {
                var vehicles = await App.Repository.Vehicles.GetForCustomerAsync(customerId);
                if (vehicles != null)
                {
                    Vehicles.Clear();
                    foreach (var vehicle in vehicles)
                    {
                        Vehicles.Add(vehicle);
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