using System.Windows;
using VesselManagementClient.ViewModel;

namespace VesselManagementClient.View
{
    public partial class ShipDetailsWindow : Window
    {
        public ShipDetailsWindow(ShipDetailsViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.RequestClose += (s, e) => this.Close();
        }
    }
}
