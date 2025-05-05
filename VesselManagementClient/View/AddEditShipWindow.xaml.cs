using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VesselManagementClient.ViewModel;

namespace VesselManagementClient.View
{
    public partial class AddEditShipWindow : Window
    {
        public AddEditShipWindow(AddEditShipViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            // Subscribe to the ViewModel's close request event
            viewModel.RequestClose += (s, e) => this.Close();
        }
    }
}
