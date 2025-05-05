namespace VesselManagementApi.DTOs
{
    public class ShipDetailsDto : ShipDto
    {
        public List<OwnerDto> Owners { get; set; } = new List<OwnerDto>();
    }
}
