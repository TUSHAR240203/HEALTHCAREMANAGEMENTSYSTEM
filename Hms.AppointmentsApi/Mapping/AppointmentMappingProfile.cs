using AutoMapper;
using Hms.AppointmentsApi.DTOs.Appointments;
using Hms.AppointmentsApi.Entities;
using Hms.AppointmentsApi.Enums;

namespace Hms.AppointmentsApi.Mapping;

public class AppointmentMappingProfile : Profile
{
    public AppointmentMappingProfile()
    {
        CreateMap<CreateAppointmentRequestDto, Appointment>()
            .ForMember(dest => dest.UHID, opt => opt.MapFrom(src => src.UHID.Trim()))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => Normalize(src.DoctorName)))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => Normalize(src.DepartmentName)))
            .ForMember(dest => dest.VisitType, opt => opt.MapFrom(src => src.VisitType.Trim()))
            .ForMember(dest => dest.ReasonForVisit, opt => opt.MapFrom(src => Normalize(src.ReasonForVisit)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => AppointmentStatus.Booked))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
            .ForMember(dest => dest.CompletionNotes, opt => opt.Ignore());

        CreateMap<Appointment, AppointmentResponseDto>()
            .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src =>
                (int)(src.SlotEndTime.ToTimeSpan() - src.SlotStartTime.ToTimeSpan()).TotalMinutes));
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
