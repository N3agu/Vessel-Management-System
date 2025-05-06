using System.Windows;
using VesselManagementClient.Model;
using VesselManagementClient.Services;
using VesselManagementClient.View;
using VesselManagementClient.ViewModel;

namespace VesselManagementClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (this.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.OpenAddOwnerRequested += MainViewModel_OpenAddOwnerRequested; // <-- NEW SUBSCRIPTION
                mainViewModel.OpenAddEditShipRequested += MainViewModel_OpenAddEditShipRequested;
                mainViewModel.OpenShipDetailsRequested += MainViewModel_OpenShipDetailsRequested;
            }
        }
        private void MainViewModel_OpenAddOwnerRequested(object? sender, EventArgs e)
        {
            var apiService = new ApiService();
            var addOwnerViewModel = new AddOwnerViewModel(apiService);
            var dialog = new AddOwnerWindow(addOwnerViewModel)
            {
                Owner = this
            };
            dialog.ShowDialog();

            if (this.DataContext is MainViewModel mainViewModel)
            {
                _ = mainViewModel.OwnersListVM.LoadOwnersAsync();
            }
        }

        private void MainViewModel_OpenAddEditShipRequested(object? sender, ShipDto? shipToEdit)
        {
            var apiService = new ApiService();
            var addEditViewModel = new AddEditShipViewModel(apiService, shipToEdit);
            var dialog = new AddEditShipWindow(addEditViewModel) { Owner = this };
            dialog.ShowDialog();

            if (this.DataContext is MainViewModel mainViewModel)
            {
                _ = mainViewModel.ShipsListVM.LoadShipsAsync();
            }
        }

        private void MainViewModel_OpenShipDetailsRequested(object? sender, int shipId)
        {
            var apiService = new ApiService();
            var detailsViewModel = new ShipDetailsViewModel(apiService, shipId);
            var dialog = new ShipDetailsWindow(detailsViewModel) { Owner = this };
            dialog.ShowDialog();
        }
    }
}