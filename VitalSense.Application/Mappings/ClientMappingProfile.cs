using AutoMapper;
using VitalSense.Domain.Entities;
using VitalSense.Application.DTOs;

namespace VitalSense.Application.Mappings;

public class ClientMappingProfile : Profile
{
    public ClientMappingProfile()
    {
        CreateMap<Client, ClientResponse>();
        CreateMap<CreateClientRequest, Client>();
        CreateMap<UpdateClientRequest, Client>()
            .ForMember(dest => dest.DieticianId, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}