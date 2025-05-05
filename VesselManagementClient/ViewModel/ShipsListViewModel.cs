using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using VesselManagementClient.Command;
using VesselManagementClient.Services;
using VesselManagementClient.Model;

namespace VesselManagementClient.ViewModel
{
    public class ShipsListViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<ShipDto> _ships = new ObservableCollection<ShipDto>();
        private ShipDto? _selectedShip;
        private string _statusMessage = string.Empty;
        private bool _isLoading = false;

        public ObservableCollection<ShipDto> Ships
        {
            get => _ships;
            set => SetProperty(ref _ships, value);
        }

        public ShipDto? SelectedShip
        {
            get => _selectedShip;
            // When selection changes, potentially load details
            set => SetProperty(ref _selectedShip, value);
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

        public ICommand LoadShipsCommand { get; }
        public ICommand AddShipCommand { get; }
        public ICommand EditShipCommand { get; }
        public ICommand DeleteShipCommand { get; }
        public ICommand ViewShipDetailsCommand { get; }

        // Event to signal opening the Add/Edit Ship window/view
        public event EventHandler<ShipDto?>? RequestOpenAddEditShip;
        // Event to signal opening the Ship Details window/view
        public event EventHandler<int>? RequestOpenShipDetails;


        public ShipsListViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadShipsCommand = new RelayCommand(async _ => await LoadShipsAsync());
            AddShipCommand = new RelayCommand(_ => OpenAddEditShip(null)); // Pass null for Add
            EditShipCommand = new RelayCommand(_ => OpenAddEditShip(SelectedShip), _ => SelectedShip != null); // Pass selected for Edit
            DeleteShipCommand = new RelayCommand(async _ => await DeleteShipAsync(), _ => SelectedShip != null);
            ViewShipDetailsCommand = new RelayCommand(_ => OpenShipDetails(SelectedShip), _ => SelectedShip != null);

            // Load initially
            _ = LoadShipsAsync();
        }

        public ShipsListViewModel() : this(new ApiService()) { } // Default constructor for XAML designer


        public async Task LoadShipsAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading ships...";
            var shipsList = await _apiService.GetShipsAsync();
            if (shipsList != null)
            {
                Ships = new ObservableCollection<ShipDto>(shipsList.OrderBy(s => s.Name));
                StatusMessage = $"Loaded {Ships.Count} ships.";
            }
            else
            {
                Ships.Clear();
                StatusMessage = "Failed to load ships.";
                MessageBox.Show("Could not load ships from the API. Ensure the API is running and the URL is correct.", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsLoading = false;
        }

        private void OpenAddEditShip(ShipDto? shipToEdit)
        {
            // Raise event to notify the main window/view to open the dialog/view
            RequestOpenAddEditShip?.Invoke(this, shipToEdit);
        }

        private void OpenShipDetails(ShipDto? ship)
        {
            if (ship != null)
            {
                RequestOpenShipDetails?.Invoke(this, ship.Id);
            }
        }


        private async Task DeleteShipAsync()
        {
            if (SelectedShip == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete ship '{SelectedShip.Name}' (IMO: {SelectedShip.ImoNumber})?",
                                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                StatusMessage = $"Deleting ship {SelectedShip.Name}...";
                var (success, errorMessage) = await _apiService.DeleteShipAsync(SelectedShip.Id);
                if (success)
                {
                    StatusMessage = $"Ship '{SelectedShip.Name}' deleted.";
                    await LoadShipsAsync(); // Refresh the list
                }
                else
                {
                    StatusMessage = $"Failed to delete ship: {errorMessage}";
                    MessageBox.Show(StatusMessage, "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                IsLoading = false;
            }
        }
    }
}
