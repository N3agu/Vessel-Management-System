using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VesselManagementClient.Model;

namespace VesselManagementClient.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public OwnersListViewModel OwnersListVM { get; }
        public ShipsListViewModel ShipsListVM { get; }

        public MainViewModel()
        {
            // direct instantiation to make keep it simple
            var apiService = new Services.ApiService();
            OwnersListVM = new OwnersListViewModel(apiService);
            ShipsListVM = new ShipsListViewModel(apiService);

            // Subscribe to events from ShipsListViewModel to handle window opening
            ShipsListVM.RequestOpenAddEditShip += OnRequestOpenAddEditShip;
            ShipsListVM.RequestOpenShipDetails += OnRequestOpenShipDetails;
        }

        // event handlers
        private void OnRequestOpenAddEditShip(object? sender, ShipDto? shipToEdit)
        {
            // raise another event here that the MainWindow can subscribe to.
            OpenAddEditShipRequested?.Invoke(this, shipToEdit);
        }

        private void OnRequestOpenShipDetails(object? sender, int shipId)
        {
            OpenShipDetailsRequested?.Invoke(this, shipId);
        }

        // Events that the MainWindow code-behind will subscribe to
        public event EventHandler<ShipDto?>? OpenAddEditShipRequested;
        public event EventHandler<int>? OpenShipDetailsRequested;

    }
}
