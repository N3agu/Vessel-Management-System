namespace VesselManagementApi.Models
{
    public class ShipOwner
    {
        public int OwnerId { get; set; }
        public virtual Owner Owner { get; set; } = null!;

        public int ShipId { get; set; }
        public virtual Ship Ship { get; set; } = null!;
    }
}
