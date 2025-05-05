﻿namespace VesselManagementApi.DTOs
{
    public class ShipDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImoNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Tonnage { get; set; }
    }
}
