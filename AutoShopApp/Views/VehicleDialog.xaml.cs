using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MMN.App.ViewModels;

namespace MMN.App.Views
{
    /// <summary>
    /// A dialog for adding or editing a vehicle.
    /// </summary>
    public sealed partial class VehicleDialog : ContentDialog
    {
        public VehicleViewModel VehicleViewModel { get; }

        public VehicleDialog() : this(new VehicleViewModel()) { }

        public VehicleDialog(VehicleViewModel viewModel)
        {
            this.InitializeComponent();
            VehicleViewModel = viewModel;
            this.DataContext = VehicleViewModel;
        }
    }
}