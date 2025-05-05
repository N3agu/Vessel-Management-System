using System.Windows;
using VesselManagementClient.Model;
using VesselManagementClient.Services;
using VesselManagementClient.View;
using VesselManagementClient.ViewModel;

namespace VesselManagementClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Subscribe to ViewModel events to open windows
            if (this.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.OpenAddOwnerRequested += MainViewModel_OpenAddOwnerRequested; // <-- NEW SUBSCRIPTION
                mainViewModel.OpenAddEditShipRequested += MainViewModel_OpenAddEditShipRequested;
                mainViewModel.OpenShipDetailsRequested += MainViewModel_OpenShipDetailsRequested;
            }
        }

        private void MainViewModel_OpenAddOwnerRequested(object? sender, EventArgs e)
        {
            var apiService = new ApiService(); // Or use DI
            var addOwnerViewModel = new AddOwnerViewModel(apiService);
            var dialog = new AddOwnerWindow(addOwnerViewModel)
            {
                Owner = this
            };
            dialog.ShowDialog();

            // After the dialog closes, refresh the owner list
            if (this.DataContext is MainViewModel mainViewModel)
            {
                _ = mainViewModel.OwnersListVM.LoadOwnersAsync(); // Refresh list
            }
        }


        private void MainViewModel_OpenAddEditShipRequested(object? sender, ShipDto? shipToEdit)
        {
            var apiService = new ApiService(); // Or use DI
            var addEditViewModel = new AddEditShipViewModel(apiService, shipToEdit);
            var dialog = new AddEditShipWindow(addEditViewModel) { Owner = this };
            dialog.ShowDialog();

            // After the dialog closes, refresh the ship list
            if (this.DataContext is MainViewModel mainViewModel)
            {
                _ = mainViewModel.ShipsListVM.LoadShipsAsync(); // Refresh list
            }
        }

        private void MainViewModel_OpenShipDetailsRequested(object? sender, int shipId)
        {
            var apiService = new ApiService(); // Or use DI
            var detailsViewModel = new ShipDetailsDialogViewModel(apiService, shipId);
            var dialog = new ShipDetailsDialog(detailsViewModel) { Owner = this };
            dialog.ShowDialog();
        }
    }
}