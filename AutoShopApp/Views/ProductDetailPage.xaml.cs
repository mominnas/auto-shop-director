using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MMN.Models;
using MMN.App.ViewModels;

namespace MMN.App.Views
{
    public sealed partial class ProductDetailPage : Page
    {
        public ProductViewModel ViewModel { get; set; }

        public ProductDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var product = (Product)e.Parameter;
            ViewModel = new ProductViewModel(product);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.SaveProductAsync();
                App.ViewModel.SyncProducts(); // Refresh the product list
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Unable to save",
                    Content = $"There was an error saving your product:\n{ex.Message}",
                    PrimaryButtonText = "OK"
                };
                dialog.XamlRoot = App.Window.Content.XamlRoot;
                await dialog.ShowAsync();
            }
        }
    }
}
