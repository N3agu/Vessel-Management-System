using AutoMapper;
using VesselManagementApi.DTOs;
using VesselManagementApi.Models;

namespace VesselManagementApi.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Owner Mappings
            CreateMap<Owner, OwnerDto>();
            CreateMap<CreateOwnerDto, Owner>();

            // Ship Mappings
            CreateMap<Ship, ShipDto>();

            // Map Ship to ShipDetailsDto, including Owner information
            CreateMap<Ship, ShipDetailsDto>()
                .ForMember(dest => dest.Owners,
                opt => opt.MapFrom(src => src.ShipOwners.Select(so => so.Owner)));

            CreateMap<CreateShipDto, Ship>(); // Map CreateShipDto to Ship (OwnerIds handled separately in service)
            CreateMap<UpdateShipDto, Ship>(); // Map UpdateShipDto to Ship
        }
    }
}
