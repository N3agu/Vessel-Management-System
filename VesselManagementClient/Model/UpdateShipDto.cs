using System.ComponentModel.DataAnnotations;

namespace VesselManagementClient.Model
{
    public class UpdateShipDto
    {
        [Required(ErrorMessage = "Ship name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "IMO number is required.")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "IMO Number must be 7 digits.")]
        [RegularExpression("^[0-9]{7}$", ErrorMessage = "IMO Number must be 7 digits.")]
        public string ImoNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ship type is required.")]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tonnage is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tonnage must be greater than 0.")]
        public decimal Tonnage { get; set; }
    }
}
