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
                mainViewModel.OpenAddEditShipRequested += MainViewModel_OpenAddEditShipRequested;
                mainViewModel.OpenShipDetailsRequested += MainViewModel_OpenShipDetailsRequested;
            }
        }

        private void MainViewModel_OpenAddEditShipRequested(object? sender, ShipDto? shipToEdit)
        {
            // Create the ViewModel for the dialog
            // Ideally, resolve ApiService via DI if set up, otherwise create new
            var apiService = new ApiService();
            var addEditViewModel = new AddEditShipViewModel(apiService, shipToEdit);

            // Create and show the dialog window
            var dialog = new AddEditShipWindow(addEditViewModel)
            {
                Owner = this // Set owner to center the dialog over the main window
            };
            dialog.ShowDialog(); // Show as modal dialog

            // After the dialog closes, refresh the ship list in the main view model
            if (this.DataContext is MainViewModel mainViewModel)
            {
                // Use Dispatcher if the event handler is not on the UI thread,
                // but in this case, it should be fine.
                _ = mainViewModel.ShipsListVM.LoadShipsAsync(); // Refresh list
            }
        }

        private void MainViewModel_OpenShipDetailsRequested(object? sender, int shipId)
        {
            var apiService = new ApiService();
            var detailsViewModel = new ShipDetailsDialogViewModel(apiService, shipId);
            var dialog = new ShipDetailsDialog(detailsViewModel)
            {
                Owner = this
            };
            dialog.ShowDialog();
        }
    }
}