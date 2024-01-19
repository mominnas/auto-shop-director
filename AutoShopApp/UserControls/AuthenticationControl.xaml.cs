using Microsoft.UI.Xaml.Controls;

namespace MMN.App.UserControls
{
    public sealed partial class AuthenticationControl : UserControl
    {
        public ViewModels.AuthenticationViewModel ViewModel { get; set; } = new ViewModels.AuthenticationViewModel();

        public AuthenticationControl()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
