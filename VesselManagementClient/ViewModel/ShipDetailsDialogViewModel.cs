using System.Windows.Input;
using VesselManagementClient.Command;
using VesselManagementClient.Model;
using VesselManagementClient.Services;

namespace VesselManagementClient.ViewModel
{
    public class ShipDetailsDialogViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ShipDetailsDto? _shipDetails;
        private string _statusMessage = "Loading...";
        private bool _isLoading = false;

        public ShipDetailsDto? ShipDetails
        {
            get => _shipDetails;
            set => SetProperty(ref _shipDetails, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand CloseCommand { get; }
        public event EventHandler? RequestClose;


        public ShipDetailsDialogViewModel(ApiService apiService, int shipId)
        {
            _apiService = apiService;
            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
            _ = LoadDetailsAsync(shipId);
        }

        public ShipDetailsDialogViewModel() : this(new ApiService(), 0) { } // For designer

        private async Task LoadDetailsAsync(int shipId)
        {
            if (shipId <= 0)
            {
                StatusMessage = "Invalid Ship ID.";
                return;
            }

            IsLoading = true;
            StatusMessage = $"Loading details for Ship ID: {shipId}...";
            ShipDetails = await _apiService.GetShipDetailsAsync(shipId);
            if (ShipDetails != null)
            {
                StatusMessage = $"Details loaded for {ShipDetails.Name}.";
            }
            else
            {
                StatusMessage = $"Failed to load details for Ship ID: {shipId}.";
            }
            IsLoading = false;
        }
    }
}