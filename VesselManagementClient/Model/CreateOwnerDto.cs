using System.ComponentModel.DataAnnotations;

namespace VesselManagementClient.Model
{
    public class CreateOwnerDto
    {
        [Required(ErrorMessage = "Owner name is required.")]
        [StringLength(100, ErrorMessage = "Owner name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}
