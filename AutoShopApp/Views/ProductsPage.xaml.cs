using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MMN.App.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Dispatching;
using MMN.Models;
using Microsoft.UI.Xaml.Media.Animation;
using CommunityToolkit.WinUI.UI.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MMN.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductsPage : Page
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public ProductsPage()
        {
            InitializeComponent();
        }

        public ProductViewModel ViewModel { get; } = new ProductViewModel();

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.LoadProducts();
        }

        /// <summary>
        /// Navigates to the product detail page when the user
        /// double-clicks an product. 
        /// </summary>
        private void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) =>
            Frame.Navigate(typeof(ProductDetailPage), ViewModel.SelectedProduct.Id);


        /// <summary>
        /// Opens the product in the order details page for editing. 
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(ProductDetailPage), ViewModel.SelectedProduct.Id);



        // Navigates to the details page for the selected customer when the user presses SPACE.
        private void DataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                Frame.Navigate(typeof(ProductDetailPage), ViewModel.SelectedProduct.Id);
            }
        }


        /// <summary>
        /// Selects the tapped product. 
        /// </summary>
        private void DataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) =>
            ViewModel.SelectedProduct = (e.OriginalSource as FrameworkElement).DataContext as Product;

        /// <summary>
        /// Navigates to the product details page.
        /// </summary>
        private void MenuFlyoutViewDetails_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(ProductDetailPage), ViewModel.SelectedProduct.Id, new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Sorts the data in the DataGrid.
        /// </summary>
        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e) =>
            (sender as DataGrid).Sort(e.Column, ViewModel.Products.Sort);

    }
}
