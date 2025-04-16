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
            //this.InitializeComponent();
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
            }
            catch (Exception)
            {
                // Handle any errors that occurred during the save process.
            }
        }
    }
}
