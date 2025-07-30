using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MMN.Models;
using MMN.App.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;

namespace MMN.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomerDetailPage : Page
    {
        /// <summary>
        /// Initializes the page.
        /// </summary>
        public CustomerDetailPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Used to bind the UI to the data.
        /// </summary>
        public CustomerViewModel ViewModel { get; set; }

        /// <summary>
        /// Navigate to the previous page when the user cancels the creation of a new customer record.
        /// </summary>
        private void AddNewCustomerCanceled(object sender, EventArgs e) => Frame.GoBack();

        /// <summary>
        /// Displays the selected customer data.
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null)
            {
                ViewModel = new CustomerViewModel
                {
                    IsNewCustomer = true,
                    IsInEdit = true
                };
                VisualStateManager.GoToState(this, "NewCustomer", false);
            }
            else
            {
                ViewModel = App.ViewModel.Customers.Where(
                    customer => customer.Model.Id == (Guid)e.Parameter).First();
            }

            ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;

            // Load vehicles for the customer
            if (ViewModel.Model != null && ViewModel.Model.Id != Guid.Empty)
            {
                await ViewModel.LoadVehiclesAsync();
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
                // Cancel the navigation immediately, otherwise it will continue at the await call. 
                e.Cancel = true;

                void resumeNavigation()
                {
                    if (e.NavigationMode == NavigationMode.Back)
                    {
                        Frame.GoBack();
                    }
                    else
                    {
                        Frame.Navigate(e.SourcePageType, e.Parameter, e.NavigationTransitionInfo);
                    }
                }

                var saveDialog = new SaveChangesDialog() { Title = $"Save changes?" };
                saveDialog.XamlRoot = this.Content.XamlRoot;
                await saveDialog.ShowAsync();
                SaveChangesDialogResult result = saveDialog.Result;

                switch (result)
                {
                    case SaveChangesDialogResult.Save:
                        await ViewModel.SaveAsync();
                        resumeNavigation();
                        break;
                    case SaveChangesDialogResult.DontSave:
                        await ViewModel.RevertChangesAsync();
                        resumeNavigation();
                        break;
                    case SaveChangesDialogResult.Cancel:
                        break;
                }
            }

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Disconnects the AddNewCustomerCanceled event handler from the ViewModel 
        /// when the parent frame navigates to a different page.
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.AddNewCustomerCanceled -= AddNewCustomerCanceled;
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Initializes the AutoSuggestBox portion of the search box.
        /// </summary>
        private void CustomerSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is UserControls.CollapsibleSearchBox searchBox)
            {
                searchBox.AutoSuggestBox.QuerySubmitted += CustomerSearchBox_QuerySubmitted;
                searchBox.AutoSuggestBox.TextChanged += CustomerSearchBox_TextChanged;
                searchBox.AutoSuggestBox.PlaceholderText = "Search customers...";
            }
        }

        /// <summary>
        /// Queries the database for a customer result matching the search text entered.
        /// </summary>
        private async void CustomerSearchBox_TextChanged(AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args)
        {
            // We only want to get results when it was a user typing,
            // otherwise we assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // If no search query is entered, refresh the complete list.
                if (string.IsNullOrEmpty(sender.Text))
                {
                    sender.ItemsSource = null;
                }
                else
                {
                    sender.ItemsSource = await App.Repository.Customers.GetAsync(sender.Text);
                }
            }
        }

        /// <summary>
        /// Search by customer name, email, or phone number, then display results.
        /// </summary>
        private void CustomerSearchBox_QuerySubmitted(AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is Customer customer)
            {
                Frame.Navigate(typeof(CustomerDetailPage), customer.Id);
            }
        }

        /// <summary>
        /// Navigates to the order page for the customer.
        /// </summary>
        private void ViewOrderButton_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ((sender as FrameworkElement).DataContext as Order).Id,
                new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Adds a new order for the customer.
        /// </summary>
        private void AddOrder_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ViewModel.Model.Id);

        /// <summary>
        /// Sorts the data in the DataGrid.
        /// </summary>
        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e) =>
            (sender as DataGrid).Sort(e.Column, ViewModel.Orders.Sort);

        // ---------------- VEHICLE MANAGEMENT ----------------

        /// <summary>
        /// Adds a new vehicle for the customer.
        /// </summary>
        private async void AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VehicleDialog(); // You need to implement VehicleDialog as a ContentDialog for vehicle entry
            dialog.XamlRoot = this.Content.XamlRoot;
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var newVehicle = dialog.VehicleViewModel.Model;
                newVehicle.CustomerId = ViewModel.Model.Id;
                var saved = await App.Repository.Vehicles.UpsertAsync(newVehicle);
                ViewModel.Vehicles.Add(new VehicleViewModel(saved));
            }
        }

        /// <summary>
        /// Edits the selected vehicle for the customer.
        /// </summary>
        private async void EditVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedVehicle == null) return;
            var dialog = new VehicleDialog(new VehicleViewModel(ViewModel.SelectedVehicle.Model));
            dialog.XamlRoot = this.Content.XamlRoot;
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var updated = await App.Repository.Vehicles.UpsertAsync(dialog.VehicleViewModel.Model);
                var idx = ViewModel.Vehicles.IndexOf(ViewModel.SelectedVehicle);
                if (idx >= 0)
                    ViewModel.Vehicles[idx] = new VehicleViewModel(updated);
            }
        }

        /// <summary>
        /// Deletes the selected vehicle for the customer.
        /// </summary>
        private async void DeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedVehicle == null) return;
            var dialog = new ContentDialog
            {
                Title = "Delete Vehicle",
                Content = "Are you sure you want to delete this vehicle?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.Content.XamlRoot
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await App.Repository.Vehicles.DeleteAsync(ViewModel.SelectedVehicle.Id);
                ViewModel.Vehicles.Remove(ViewModel.SelectedVehicle);
            }
        }
    }
}
