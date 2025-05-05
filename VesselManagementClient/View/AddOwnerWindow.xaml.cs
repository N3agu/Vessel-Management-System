using System.Windows;
using VesselManagementClient.ViewModel;

namespace VesselManagementClient.View
{
    public partial class AddOwnerWindow : Window
    {
        public AddOwnerWindow(AddOwnerViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.RequestClose += (s, e) => this.Close();
        }
    }
}
