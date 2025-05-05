using System.ComponentModel.DataAnnotations;

namespace VesselManagementApi.DTOs
{
    public class CreateOwnerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
