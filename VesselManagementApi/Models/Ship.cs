using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VesselManagementApi.Models
{
    public class Ship
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(7, MinimumLength = 7)] // IMO number - 7 digits
        public string ImoNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tonnage { get; set; }

        // many-to-many relationship (linking entity)
        public virtual ICollection<ShipOwner> ShipOwners { get; set; } = new List<ShipOwner>();
    }
}
