using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MMN.App.Helpers;
using MMN.App.ViewModels;
using MMN.Models;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MMN.App.Views
{
    /// <summary>
    /// Displays and edits an order.
    /// </summary>
    public sealed partial class OrderDetailPage : Page, INotifyPropertyChanged
    {
        private readonly String[] CompanyAddress = { "1312 Britannia RD E UNIT 19", "Mississauga, ON L4W 1C8" };
        private readonly String CompanyPhone = "Phone: (905) 696-8786";
        private readonly String CompanyName = "7 Star Auto Care Inc.";
        private readonly String CompanyEmail = "7StarAutoCare@gmail.com";
        private readonly String TaxId = "826929085RT0001";


        /// <summary>
        /// Initializes the page.
        /// </summary>
        public OrderDetailPage() => InitializeComponent();

        /// <summary>
        /// Stores the view model. 
        /// </summary>
        private OrderViewModel _viewModel;

        /// <summary>
        /// We use this object to bind the UI to our data.
        /// </summary>
        public OrderViewModel ViewModel
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
        /// Loads the specified order, a cached order, or creates a new order.
        /// </summary>
        /// <param name="e">Info about the event.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var guid = (Guid)e.Parameter;
            var customer = App.ViewModel.Customers.Where(cust => cust.Model.Id == guid).FirstOrDefault();

            if (customer != null)
            {
                // Order is a new order
                ViewModel = new OrderViewModel(new Order(customer.Model));
            }
            else
            {
                // Order is an existing order.
                var order = await App.Repository.Orders.GetAsync(guid);
                ViewModel = new OrderViewModel(order);
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
                    Title = $"Save changes to Invoice # {ViewModel.InvoiceNumber.ToString()}?",
                    Content = $"Invoice # {ViewModel.InvoiceNumber.ToString()} " + 
                        "has unsaved changes that will be lost. Do you want to save your changes?"
                };
                saveDialog.XamlRoot = this.Content.XamlRoot;
                await saveDialog.ShowAsync();
                SaveChangesDialogResult result = saveDialog.Result;

                switch (result)
                {
                    case SaveChangesDialogResult.Save:
                        await ViewModel.SaveOrderAsync();
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

                        // This flag gets cleared on navigation, so restore it. 
                        ViewModel.IsModified = true; 
                        break;
                }
            }

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Creates an email to the current customer.
        /// </summary>
        private async void emailButton_Click(object sender, RoutedEventArgs e)
        {
            var emailMessage = new EmailMessage
            {
                Body = $"Dear {ViewModel.CustomerName},",
                Subject = "A message from Contoso about order " +
                    $"#{ViewModel.InvoiceNumber} placed on {ViewModel.DatePlaced.ToString("MM/dd/yyyy")} "
            };

            if (!string.IsNullOrEmpty(ViewModel.Customer.Email))
            {
                var emailRecipient = new EmailRecipient(ViewModel.Customer.Email);
                emailMessage.To.Add(emailRecipient);
            }

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        /// <summary>
        /// Reloads the order.
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => 
            ViewModel = await OrderViewModel.CreateFromGuid(ViewModel.Id);

        /// <summary>
        /// Reverts the page.
        /// </summary>
        private async void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveChangesDialog()
            {
                Title = $"Save changes to Invoice # {ViewModel.InvoiceNumber.ToString()}?",
                Content = $"Invoice # {ViewModel.InvoiceNumber.ToString()} " + 
                    "has unsaved changes that will be lost. Do you want to save your changes?"
            };
            saveDialog.XamlRoot = this.Content.XamlRoot;
            await saveDialog.ShowAsync();
            SaveChangesDialogResult result = saveDialog.Result;

            switch (result)
            {
                case SaveChangesDialogResult.Save:
                    await ViewModel.SaveOrderAsync();
                    ViewModel = await OrderViewModel.CreateFromGuid(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.DontSave:
                    ViewModel = await OrderViewModel.CreateFromGuid(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.Cancel:
                    break;
            }         
        }

        /// <summary>
        /// Saves the current order.
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.SaveOrderAsync();
                // Update bindings after model is properly updated
                Bindings.Update();
                // Optionally refresh the entire view model if needed
                ViewModel = await OrderViewModel.CreateFromGuid(ViewModel.Id);
            }
            catch (OrderSavingException ex)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Unable to save",
                    Content = $"There was an error saving your order:\n{ex.Message}",
                    PrimaryButtonText = "OK"
                };
                dialog.XamlRoot = App.Window.Content.XamlRoot;
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Queries for products.
        /// </summary>
        private void ProductSearchBox_TextChanged(AutoSuggestBox sender, 
            AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.UpdateProductSuggestions(sender.Text);
            }
        }

        /// <summary>
        /// Notifies the page that a new item was chosen.
        /// </summary>
        private void ProductSearchBox_SuggestionChosen(AutoSuggestBox sender, 
            AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem != null)
            {
                var selectedProduct = args.SelectedItem as Product;
                ViewModel.NewLineItem.Product = selectedProduct;
            }
        }

        /// <summary>
        /// Adds the new line item to the list of line items.
        /// </summary>
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LineItems.Add(ViewModel.NewLineItem.Model);
            ClearCandidateProduct();
        }

        /// <summary>
        /// Clears the new line item without adding it to the list of line items.
        /// </summary>
        private void CancelProductButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCandidateProduct();
        }

        /// <summary>
        /// Clears the new line item entry area.
        /// </summary>
        private void ClearCandidateProduct()
        {
            ProductSearchBox.Text = string.Empty;
            ViewModel.NewLineItem = new LineItemViewModel();
        }

        /// <summary>
        /// Removes a line item from the order.
        /// </summary>
        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LineItems.Remove((sender as FrameworkElement).DataContext as LineItem);
        }

        /// <summary>
        /// Prints the current order.
        /// </summary>
        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            // Prepare the order model for PDF
            var order = ViewModel.Model;

            // Ask user where to save the PDF
            //var picker = new FileSavePicker();
            //picker.SuggestedStartLocation = PickerLocationId.Downloads;
            //picker.FileTypeChoices.Add("PDF Document", new[] { ".pdf" });
            //picker.SuggestedFileName = $"Invoice_{order.InvoiceNumber}";
            //picker.DefaultFileExtension = ".pdf";
            //picker.SettingsIdentifier = "SaveInvoicePDF";
            //picker.CommitButtonText = "Save Invoice";

            //// UWP/WinUI3: Need to initialize picker with window handle
            //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            //WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            //StorageFile file = await picker.PickSaveFileAsync();
            //if (file == null)
            //    return;








            var doc = new InvoiceDocument(
                    order,
                    CompanyName,
                    CompanyAddress,
                    CompanyPhone,
                    TaxId
                );

            _ = Task.Run(() =>
            {
                try
                {
                    doc.ShowInCompanion();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ShowInCompanion failed: {ex}");
                }
            });


            // Generate PDF to a memory stream
            //using (var mem = new MemoryStream())
            //{
            //    var doc = new InvoiceDocument(
            //        order,
            //        CompanyName,
            //        CompanyAddress,
            //        CompanyPhone,
            //        TaxId
            //    );

            //    doc.ShowInCompanion();

            //    doc.GeneratePdf(mem);

            //    mem.Seek(0, SeekOrigin.Begin);

            //    // Save to file
            //    using (var stream = await file.OpenStreamForWriteAsync())
            //    {
            //        await mem.CopyToAsync(stream);
            //    }
            //}

            //// Optionally, show a dialog or open the PDF
            //var dialog = new ContentDialog
            //{
            //    Title = "Invoice Saved",
            //    Content = $"Invoice PDF saved to {file.Path}",
            //    CloseButtonText = "OK",
            //    XamlRoot = this.Content.XamlRoot
            //};
            //await dialog.ShowAsync();
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
