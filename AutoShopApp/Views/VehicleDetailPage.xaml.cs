using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MMN.Models;
using MMN.App.ViewModels;

namespace MMN.App.Views
{
    /// <summary>
    /// Displays and edits a vehicle.
    /// </summary>
    public sealed partial class VehicleDetailPage : Page, INotifyPropertyChanged
    {
        private VehicleViewModel _viewModel;

        /// <summary>
        /// We use this object to bind the UI to our data.
        /// </summary>
        public VehicleViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != value)
                {
                    _viewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Initializes the page.
        /// </summary>
        public VehicleDetailPage() => InitializeComponent();

        /// <summary>
        /// Loads the specified vehicle, a cached vehicle, or creates a new vehicle.
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var guid = (Guid)e.Parameter;

            // Try to get the vehicle by ID
            var vehicle = await App.Repository.Vehicles.GetAsync(guid);

            if (vehicle != null)
            {
                // Editing an existing vehicle
                ViewModel = new VehicleViewModel(vehicle);
            }
            else
            {
                // Adding a new vehicle for the customer
                ViewModel = new VehicleViewModel();
                ViewModel.CustomerId = guid; // Set the customer ID for the new vehicle
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Check whether there are unsaved changes and warn the user.
        /// </summary>
        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ViewModel.IsModified)
            {
                var saveDialog = new SaveChangesDialog()
                {
                    Title = "Save changes to this vehicle?",
                    Content = "This vehicle has unsaved changes that will be lost. Do you want to save your changes?"
                };
                saveDialog.XamlRoot = this.Content.XamlRoot;
                await saveDialog.ShowAsync();
                SaveChangesDialogResult result = saveDialog.Result;

                switch (result)
                {
                    case SaveChangesDialogResult.Save:
                        await ViewModel.SaveVehicleAsync();
                        break;
                    case SaveChangesDialogResult.DontSave:
                        break;
                    case SaveChangesDialogResult.Cancel:
                        if (e.NavigationMode == NavigationMode.Back)
                        {
                            Frame.GoForward();
                        }
                        else
                        {
                            Frame.GoBack();
                        }
                        e.Cancel = true;
                        ViewModel.IsModified = true;
                        break;
                }
            }

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Saves the current vehicle.
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.SaveVehicleAsync();
                Bindings.Update();
                ViewModel = await VehicleViewModel.CreateFromGuid(ViewModel.Id);

                Frame.GoBack();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Unable to save",
                    Content = $"There was an error saving your vehicle:\n{ex.Message}",
                    PrimaryButtonText = "OK"
                };
                dialog.XamlRoot = App.Window.Content.XamlRoot;
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Reverts the page.
        /// </summary>
        private async void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveChangesDialog()
            {
                Title = "Save changes to this vehicle?",
                Content = "This vehicle has unsaved changes that will be lost. Do you want to save your changes?"
            };
            saveDialog.XamlRoot = this.Content.XamlRoot;
            await saveDialog.ShowAsync();
            SaveChangesDialogResult result = saveDialog.Result;

            switch (result)
            {
                case SaveChangesDialogResult.Save:
                    await ViewModel.SaveVehicleAsync();
                    ViewModel = await VehicleViewModel.CreateFromGuid(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.DontSave:
                    ViewModel = await VehicleViewModel.CreateFromGuid(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.Cancel:
                    break;
            }
        }

        /// <summary>
        /// Cancels and navigates back.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        /// <summary>
        /// Fired when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value changed.
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}