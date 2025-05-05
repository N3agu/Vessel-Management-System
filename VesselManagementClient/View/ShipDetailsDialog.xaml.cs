using System.Windows;
using VesselManagementClient.ViewModel;

namespace VesselManagementClient.View
{
    public partial class ShipDetailsDialog : Window
    {
        public ShipDetailsDialog(ShipDetailsDialogViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.RequestClose += (s, e) => this.Close();
        }
    }
}
