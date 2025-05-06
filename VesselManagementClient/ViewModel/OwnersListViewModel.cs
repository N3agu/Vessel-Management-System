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

        public ObservableCollection<OwnerDto> Owners { get => _owners; set => SetProperty(ref _owners, value); }
        public OwnerDto? SelectedOwner { get => _selectedOwner; set => SetProperty(ref _selectedOwner, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        public ICommand LoadOwnersCommand { get; }
        public ICommand AddOwnerCommand { get; }
        public ICommand DeleteOwnerCommand { get; }
        public ICommand CopyCellCommand { get; }
        public ICommand CopyRowCommand { get; }

        public event EventHandler? RequestOpenAddOwner;

        public OwnersListViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadOwnersCommand = new RelayCommand(async _ => await LoadOwnersAsync());
           
            AddOwnerCommand = new RelayCommand(_ => RequestOpenAddOwner?.Invoke(this, EventArgs.Empty));
            DeleteOwnerCommand = new RelayCommand(async _ => await DeleteOwnerAsync(), _ => SelectedOwner != null);
            CopyCellCommand = new RelayCommand(CopyCell, _ => SelectedOwner != null);
            CopyRowCommand = new RelayCommand(CopyRow, _ => SelectedOwner != null);
            _ = LoadOwnersAsync();
        }

        public OwnersListViewModel() : this(new ApiService()) { }

        public async Task LoadOwnersAsync() // Made public for refresh
        {
            IsLoading = true;
            StatusMessage = "Loading owners...";
            var ownersList = await _apiService.GetOwnersAsync();
            if (ownersList != null) { Owners = new ObservableCollection<OwnerDto>(ownersList.OrderBy(o => o.Name)); StatusMessage = $"Loaded {Owners.Count} owners."; }
            else { Owners.Clear(); StatusMessage = "Failed to load owners."; MessageBox.Show("Could not load owners.", "API Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            IsLoading = false;
        }

        private async Task DeleteOwnerAsync()
        {
            if (SelectedOwner == null) return;
            var result = MessageBox.Show($"Delete owner '{SelectedOwner.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                StatusMessage = $"Deleting owner {SelectedOwner.Name}...";
                var (success, errorMessage) = await _apiService.DeleteOwnerAsync(SelectedOwner.Id);
                if (success) { StatusMessage = $"Owner '{SelectedOwner.Name}' deleted."; await LoadOwnersAsync(); }
                else { StatusMessage = $"Failed to delete owner: {errorMessage}"; MessageBox.Show(StatusMessage, "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error); }
                IsLoading = false;
            }
        }

        private void CopyCell(object? parameter)
        {
            if (SelectedOwner != null)
            {
                try { Clipboard.SetText(SelectedOwner.Name ?? string.Empty); StatusMessage = "Owner name copied."; }
                catch (Exception ex) { StatusMessage = $"Error copying: {ex.Message}"; }
            }
        }

        private void CopyRow(object? parameter)
        {
            if (SelectedOwner != null)
            {
                var rowText = $"ID: {SelectedOwner.Id}\tName: {SelectedOwner.Name}";
                try { Clipboard.SetText(rowText); StatusMessage = "Owner row copied."; }
                catch (Exception ex) { StatusMessage = $"Error copying: {ex.Message}"; }
            }
        }
    }
}
