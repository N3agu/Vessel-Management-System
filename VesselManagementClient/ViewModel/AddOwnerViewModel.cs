using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.Windows;
using VesselManagementClient.Command;
using VesselManagementClient.Services;
using VesselManagementClient.Model;

namespace VesselManagementClient.ViewModel
{
    public class AddOwnerViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _ownerName = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isLoading = false;

        public string WindowTitle => "Add New Owner";

        [Required(ErrorMessage = "Owner name is required")]
        [StringLength(100)]
        public string OwnerName
        {
            get => _ownerName;
            set => SetProperty(ref _ownerName, value);
        }

        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Event to signal closing the window
        public event EventHandler? RequestClose;

        public AddOwnerViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
        }

        public AddOwnerViewModel() : this(new ApiService()) { } // For designer

        private bool CanSave()
        {
            // Basic validation
            return !string.IsNullOrWhiteSpace(OwnerName) && OwnerName.Length <= 100 && !IsLoading;
        }

        private async Task SaveAsync()
        {
            IsLoading = true;
            StatusMessage = "Saving owner...";

            var createDto = new CreateOwnerDto { Name = this.OwnerName };
            var (createdOwner, errorMessage) = await _apiService.CreateOwnerAsync(createDto);

            if (createdOwner != null)
            {
                StatusMessage = "Save successful!";
                MessageBox.Show("Owner saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke(this, EventArgs.Empty); // Close window on success
            }
            else
            {
                StatusMessage = $"Save failed: {errorMessage}";
                MessageBox.Show(StatusMessage, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsLoading = false;
        }
    }
}
