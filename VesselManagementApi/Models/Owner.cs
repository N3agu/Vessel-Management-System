using System.ComponentModel.DataAnnotations;

namespace VesselManagementApi.Models
{
    public class Owner
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // many-to-many relationship(linking entity)
        public virtual ICollection<ShipOwner> ShipOwners { get; set; } = new List<ShipOwner>();
    }
}
