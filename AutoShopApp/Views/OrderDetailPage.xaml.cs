using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.Email;
using MMN.Models;
using MMN.App.ViewModels;
using MMN.App.Helpers;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Media;

namespace MMN.App.Views
{
    /// <summary>
    /// Displays and edits an order.
    /// </summary>
    public sealed partial class OrderDetailPage : Page, INotifyPropertyChanged
    {
        private readonly String[] CompanyAddress = { "123 Mechanic Street", "Anytown, ST 12345" };
        private readonly String CompanyPhone = "Phone: (555) 123-4567";
        private readonly String CompanyName = "7 Star Autos";

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
            var printHelper = new PrintHelper();
            var contentToPrint = new Grid();
            
            // Create content for printing with a professional layout using black text
            var printContent = new StackPanel
            {
                Margin = new Thickness(40)
            };

            // Add company logo and header
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Add logo
            var logoImage = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png")),
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 150,
                Height = 100,
                Margin = new Thickness(0, 0, 0, 20),
                Stretch = Stretch.Uniform
            };

            Grid.SetColumn(logoImage, 0);
            headerGrid.Children.Add(logoImage);

            // Add company info
            // Add company info with black text
            var companyInfo = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };
            companyInfo.Children.Add(new TextBlock
            {
                Text = CompanyName,  // Use CompanyName variable
                FontSize = 24,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });

            // Add address lines from CompanyAddress array
            foreach (var line in CompanyAddress)
            {
                companyInfo.Children.Add(new TextBlock
                {
                    Text = line,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                });
            }

            // Add phone number
            companyInfo.Children.Add(new TextBlock
            {
                Text = CompanyPhone,  // Use CompanyPhone variable
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });

            Grid.SetColumn(companyInfo, 1);
            headerGrid.Children.Add(companyInfo);

            printContent.Children.Add(headerGrid);

            // Add separator
            printContent.Children.Add(new Rectangle
            {
                Height = 2,
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                Margin = new Thickness(0, 20, 0, 20)
            });

            // Add invoice header
            printContent.Children.Add(new TextBlock
            {
                Text = $"INVOICE #{ViewModel.InvoiceNumber}",
                FontSize = 28,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });

            // Create grid for customer and invoice details
            var detailsGrid = new Grid();
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Customer info panel
            var customerInfo = new StackPanel { Margin = new Thickness(0, 0, 20, 0) };
            customerInfo.Children.Add(new TextBlock
            {
                Text = "BILL TO:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });
            customerInfo.Children.Add(new TextBlock 
            { 
                Text = ViewModel.CustomerName,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });
            customerInfo.Children.Add(new TextBlock 
            { 
                Text = ViewModel.Address,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });
            Grid.SetColumn(customerInfo, 0);
            detailsGrid.Children.Add(customerInfo);

            // Invoice details panel
            var invoiceDetails = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };
            invoiceDetails.Children.Add(new TextBlock
            {
                Text = $"Date: {ViewModel.DatePlaced:d}",
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });
            invoiceDetails.Children.Add(new TextBlock
            {
                Text = $"Status: {ViewModel.OrderStatus}",
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });
            Grid.SetColumn(invoiceDetails, 1);
            detailsGrid.Children.Add(invoiceDetails);

            printContent.Children.Add(detailsGrid);

            // Add separator before line items
            printContent.Children.Add(new Rectangle
            {
                Height = 2,
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                Margin = new Thickness(0, 20, 0, 10)
            });

            // Add line items header with background and borders
            var lineItemsHeader = new Grid 
            { 
                Margin = new Thickness(0, 0, 0, 10),
                Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray)
            };
            lineItemsHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            lineItemsHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            lineItemsHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            lineItemsHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var headers = new[] { "Description", "Quantity", "Unit Price", "Amount" };
            for (int i = 0; i < headers.Length; i++)
            {
                lineItemsHeader.Children.Add(new TextBlock
                {
                    Text = headers[i],
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                });
                Grid.SetColumn((FrameworkElement)lineItemsHeader.Children[i], i);
            }
            printContent.Children.Add(lineItemsHeader);

            // Add line items with alternating row colors
            for (int index = 0; index < ViewModel.LineItems.Count; index++)
            {
                var item = ViewModel.LineItems[index];
                var lineItemGrid = new Grid 
                { 
                    Margin = new Thickness(0, 0, 0, 5),
                    Background = index % 2 == 0 
                        ? new SolidColorBrush(Microsoft.UI.Colors.White) 
                        : new SolidColorBrush(Microsoft.UI.Colors.WhiteSmoke)
                };
                lineItemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                lineItemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                lineItemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                lineItemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var productName = new TextBlock 
                { 
                    Text = item.Product.Name, 
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                };
                var quantity = new TextBlock 
                { 
                    Text = item.Quantity.ToString(), 
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                };
                var unitPrice = new TextBlock 
                { 
                    Text = item.Product.ListPrice.ToString("C"), 
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                };
                var amount = new TextBlock 
                { 
                    Text = (item.Quantity * item.Product.ListPrice).ToString("C"), 
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                };

                Grid.SetColumn(productName, 0);
                Grid.SetColumn(quantity, 1);
                Grid.SetColumn(unitPrice, 2);
                Grid.SetColumn(amount, 3);

                lineItemGrid.Children.Add(productName);
                lineItemGrid.Children.Add(quantity);
                lineItemGrid.Children.Add(unitPrice);
                lineItemGrid.Children.Add(amount);

                printContent.Children.Add(lineItemGrid);
            }

            // Add separator before totals
            printContent.Children.Add(new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                Margin = new Thickness(0, 10, 0, 10)
            });

            // Add totals with black text
            var totalsPanel = new Border
            {
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(10),
                Child = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 10, 0, 20),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"Subtotal: {ViewModel.Subtotal:C}",
                            Margin = new Thickness(0, 0, 0, 5),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                        },
                        new TextBlock
                        {
                            Text = $"Tax: {ViewModel.Tax:C}",
                            Margin = new Thickness(0, 0, 0, 5),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                        },
                        new TextBlock
                        {
                            Text = $"Total: {ViewModel.GrandTotal:C}",
                            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                            Margin = new Thickness(0, 5, 0, 5),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                        }
                    }
                }
            };

            printContent.Children.Add(totalsPanel);

            // Add thank you note
            printContent.Children.Add(new TextBlock
            {
                Text = "Thank you for your business!",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                Margin = new Thickness(0, 20, 0, 0),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
            });

            contentToPrint.Children.Add(printContent);

            // Print the content
            await printHelper.Print(App.Window, contentToPrint, $"Invoice #{ViewModel.InvoiceNumber}");
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
