using AutoMapper;
using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<QueueToken, QueueItemDto>();

        CreateMap<PatientCheckIn, CheckInResponseDto>()
            .ForMember(d => d.CheckInId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.QueuePosition, o => o.MapFrom(s => s.TokenNumber));
    }
}