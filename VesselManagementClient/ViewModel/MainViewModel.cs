using VesselManagementClient.Model;

namespace VesselManagementClient.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public OwnersListViewModel OwnersListVM { get; }
        public ShipsListViewModel ShipsListVM { get; }

        public MainViewModel()
        {
            var apiService = new Services.ApiService();
            OwnersListVM = new OwnersListViewModel(apiService);
            ShipsListVM = new ShipsListViewModel(apiService);

            OwnersListVM.RequestOpenAddOwner += OnRequestOpenAddOwner;
            ShipsListVM.RequestOpenAddEditShip += OnRequestOpenAddEditShip;
            ShipsListVM.RequestOpenShipDetails += OnRequestOpenShipDetails;
        }

        private void OnRequestOpenAddOwner(object? sender, EventArgs e)
        {
            OpenAddOwnerRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnRequestOpenAddEditShip(object? sender, ShipDto? shipToEdit)
        {
            OpenAddEditShipRequested?.Invoke(this, shipToEdit);
        }

        private void OnRequestOpenShipDetails(object? sender, int shipId)
        {
            OpenShipDetailsRequested?.Invoke(this, shipId);
        }

        public event EventHandler? OpenAddOwnerRequested;
        public event EventHandler<ShipDto?>? OpenAddEditShipRequested;
        public event EventHandler<int>? OpenShipDetailsRequested;
    }
}
