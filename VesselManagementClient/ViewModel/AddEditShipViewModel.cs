using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.Windows;
using VesselManagementClient.Command;
using VesselManagementClient.Services;
using VesselManagementClient.Model;

namespace VesselManagementClient.ViewModel
{
    public class SelectableOwnerViewModel : BaseViewModel
    {
        private bool _isSelected;
        public OwnerDto Owner { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public SelectableOwnerViewModel(OwnerDto owner)
        {
            Owner = owner;
        }
    }

    public class AddEditShipViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ShipDto? _originalShip; // Keep track if editing
        private bool _isEditMode => _originalShip != null;

        private string _windowTitle = "Add New Ship";
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        // Ship Properties (bindable)
        private int _shipId;
        private string _shipName = string.Empty;
        private string _imoNumber = string.Empty;
        private string _shipType = string.Empty;
        private decimal _tonnage;

        public int ShipId { get => _shipId; set => SetProperty(ref _shipId, value); }

        [Required(ErrorMessage = "Ship name is required")]
        [StringLength(100)]
        public string ShipName { get => _shipName; set => SetProperty(ref _shipName, value); }

        [Required(ErrorMessage = "IMO number is required")]
        [RegularExpression("^[0-9]{7}$", ErrorMessage = "IMO must be 7 digits")]
        public string ImoNumber { get => _imoNumber; set => SetProperty(ref _imoNumber, value); }

        [Required(ErrorMessage = "Ship type is required")]
        [StringLength(50)]
        public string ShipType { get => _shipType; set => SetProperty(ref _shipType, value); }

        [Required(ErrorMessage = "Tonnage is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tonnage must be positive")]
        public decimal Tonnage { get => _tonnage; set => SetProperty(ref _tonnage, value); }


        // Owner Selection
        private ObservableCollection<SelectableOwnerViewModel> _availableOwners = new ObservableCollection<SelectableOwnerViewModel>();
        public ObservableCollection<SelectableOwnerViewModel> AvailableOwners
        {
            get => _availableOwners;
            set => SetProperty(ref _availableOwners, value);
        }

        public List<int> SelectedOwnerIds => AvailableOwners.Where(o => o.IsSelected).Select(o => o.Owner.Id).ToList();


        // Status and Loading
        private string _statusMessage = string.Empty;
        private bool _isLoading = false;
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }


        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; } // Maybe just close the window

        // Event to signal closing the window
        public event EventHandler? RequestClose;


        public AddEditShipViewModel(ApiService apiService, ShipDto? shipToEdit = null)
        {
            _apiService = apiService;
            _originalShip = shipToEdit;

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));

            if (_isEditMode && _originalShip != null)
            {
                WindowTitle = $"Edit Ship: {_originalShip.Name}";
                ShipId = _originalShip.Id;
                ShipName = _originalShip.Name;
                ImoNumber = _originalShip.ImoNumber;
                ShipType = _originalShip.Type;
                Tonnage = _originalShip.Tonnage;
                // Owner selection disabled in edit mode for simplicity (matches API)
            }
            else
            {
                WindowTitle = "Add New Ship";
                // Load owners only needed for Add mode
                _ = LoadAvailableOwnersAsync();
            }
        }

        public AddEditShipViewModel() : this(new ApiService()) { } // For designer

        private async Task LoadAvailableOwnersAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading available owners...";
            var ownersList = await _apiService.GetOwnersAsync();
            if (ownersList != null)
            {
                AvailableOwners = new ObservableCollection<SelectableOwnerViewModel>(
                    ownersList.OrderBy(o => o.Name).Select(o => new SelectableOwnerViewModel(o))
                );
                StatusMessage = "Select owners.";
            }
            else
            {
                StatusMessage = "Failed to load owners.";
                MessageBox.Show("Could not load owners list. Cannot add ship without owners.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsLoading = false;
        }


        private bool CanSave()
        {
            // Basic client-side check - more robust validation can be added
            bool ownersSelected = _isEditMode || SelectedOwnerIds.Any(); // Must select owners in Add mode
            return !string.IsNullOrWhiteSpace(ShipName) &&
                   !string.IsNullOrWhiteSpace(ImoNumber) && ImoNumber.Length == 7 && int.TryParse(ImoNumber, out _) &&
                   !string.IsNullOrWhiteSpace(ShipType) &&
                   Tonnage > 0 &&
                   ownersSelected &&
                   !IsLoading;
        }

        private async Task SaveAsync()
        {
            IsLoading = true;
            StatusMessage = "Saving...";

            bool success = false;
            string? errorMessage = null;

            try
            {
                if (_isEditMode)
                {
                    var updateDto = new UpdateShipDto
                    {
                        Name = this.ShipName,
                        ImoNumber = this.ImoNumber,
                        Type = this.ShipType,
                        Tonnage = this.Tonnage
                    };
                    // Validate DTO (optional here, relies on API validation mostly)
                    var (updateSuccess, updateError) = await _apiService.UpdateShipAsync(this.ShipId, updateDto);
                    success = updateSuccess;
                    errorMessage = updateError;
                }
                else // Add Mode
                {
                    if (!SelectedOwnerIds.Any())
                    {
                        MessageBox.Show("Please select at least one owner.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        IsLoading = false;
                        StatusMessage = "Validation failed.";
                        return;
                    }

                    var createDto = new CreateShipDto
                    {
                        Name = this.ShipName,
                        ImoNumber = this.ImoNumber,
                        Type = this.ShipType,
                        Tonnage = this.Tonnage,
                        OwnerIds = this.SelectedOwnerIds
                    };
                    // Validate DTO
                    var (createdShip, createError) = await _apiService.CreateShipAsync(createDto);
                    success = createdShip != null;
                    errorMessage = createError;
                }

                if (success)
                {
                    StatusMessage = "Save successful!";
                    MessageBox.Show("Ship saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestClose?.Invoke(this, EventArgs.Empty); // Close the window on success
                }
                else
                {
                    StatusMessage = $"Save failed: {errorMessage}";
                    MessageBox.Show(StatusMessage, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"An unexpected error occurred: {ex.Message}";
                MessageBox.Show(StatusMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
