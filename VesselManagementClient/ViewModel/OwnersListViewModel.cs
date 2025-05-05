using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using VesselManagementClient.Command;
using VesselManagementClient.Services;
using VesselManagementClient.Model;

namespace VesselManagementClient.ViewModel
{
    public class OwnersListViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<OwnerDto> _owners = new ObservableCollection<OwnerDto>();
        private OwnerDto? _selectedOwner;
        private string _statusMessage = string.Empty;
        private bool _isLoading = false;

        public ObservableCollection<OwnerDto> Owners
        {
            get => _owners;
            set => SetProperty(ref _owners, value);
        }

        public OwnerDto? SelectedOwner
        {
            get => _selectedOwner;
            set => SetProperty(ref _selectedOwner, value);
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

        public ICommand LoadOwnersCommand { get; }
        public ICommand AddOwnerCommand { get; }
        public ICommand DeleteOwnerCommand { get; }

        public OwnersListViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadOwnersCommand = new RelayCommand(async _ => await LoadOwnersAsync());
            AddOwnerCommand = new RelayCommand(_ => AddOwner());
            DeleteOwnerCommand = new RelayCommand(async _ => await DeleteOwnerAsync(), _ => SelectedOwner != null);

            // load initially
            _ = LoadOwnersAsync();
        }

        public OwnersListViewModel() : this(new ApiService()) { } // Default constructor for XAML designer

        private async Task LoadOwnersAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading owners...";
            var ownersList = await _apiService.GetOwnersAsync();
            if (ownersList != null)
            {
                Owners = new ObservableCollection<OwnerDto>(ownersList.OrderBy(o => o.Name));
                StatusMessage = $"Loaded {Owners.Count} owners.";
            }
            else
            {
                Owners.Clear();
                StatusMessage = "Failed to load owners.";
                MessageBox.Show("Could not load owners from the API. Ensure the API is running and the URL is correct.", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsLoading = false;
        }

        private void AddOwner()
        {
            // Simple input dialog for adding (should have used a dedicated view/dialog)
            var newOwnerName = Microsoft.VisualBasic.Interaction.InputBox("Enter new owner name:", "Add Owner", "");
            if (!string.IsNullOrWhiteSpace(newOwnerName))
            {
                var createDto = new CreateOwnerDto { Name = newOwnerName.Trim() };
                _ = CreateOwnerAsync(createDto);
            }
        }

        private async Task CreateOwnerAsync(CreateOwnerDto dto)
        {
            IsLoading = true;
            StatusMessage = "Adding owner...";
            var (createdOwner, errorMessage) = await _apiService.CreateOwnerAsync(dto);
            if (createdOwner != null)
            {
                StatusMessage = $"Owner '{createdOwner.Name}' added.";
                await LoadOwnersAsync(); // Refresh list
            }
            else
            {
                StatusMessage = $"Failed to add owner: {errorMessage}";
                MessageBox.Show(StatusMessage, "Add Owner Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            IsLoading = false;
        }


        private async Task DeleteOwnerAsync()
        {
            if (SelectedOwner == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete owner '{SelectedOwner.Name}' (ID: {SelectedOwner.Id})? This will also remove their ownership links from ships.",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                StatusMessage = $"Deleting owner {SelectedOwner.Name}...";
                var (success, errorMessage) = await _apiService.DeleteOwnerAsync(SelectedOwner.Id);
                if (success)
                {
                    StatusMessage = $"Owner '{SelectedOwner.Name}' deleted.";
                    await LoadOwnersAsync(); // Refresh the list
                }
                else
                {
                    StatusMessage = $"Failed to delete owner: {errorMessage}";
                    MessageBox.Show(StatusMessage, "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                IsLoading = false;
            }
        }
    }
}
