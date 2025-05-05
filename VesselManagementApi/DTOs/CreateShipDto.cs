using System.ComponentModel.DataAnnotations;

namespace VesselManagementApi.DTOs
{
    public class CreateShipDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(7, MinimumLength = 7)]
        [RegularExpression("^[0-9]{7}$", ErrorMessage = "IMO Number must be 7 digits.")]
        public string ImoNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tonnage must be greater than 0.")]
        public decimal Tonnage { get; set; }

        // List of Owner IDs to associate with the new ship
        [Required]
        [MinLength(1, ErrorMessage = "At least one owner ID must be provided.")]
        public List<int> OwnerIds { get; set; } = new List<int>();
    }
}
