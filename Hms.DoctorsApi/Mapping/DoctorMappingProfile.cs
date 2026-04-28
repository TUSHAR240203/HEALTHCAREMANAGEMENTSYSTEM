using AutoMapper;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.Entities;

namespace Hms.DoctorsApi.Mapping;

public class DoctorMappingProfile : Profile
{
    public DoctorMappingProfile()
    {
        CreateMap<CreateDoctorRequestDto, Doctor>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.Trim()))
            .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization.Trim()))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartmentName.Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => Normalize(src.Email)))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => Normalize(src.Phone)))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => Normalize(src.Gender)))
            .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => Normalize(src.Qualification)))
            .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => Normalize(src.LicenseNumber)))
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => Normalize(src.RoomNumber)))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => Normalize(src.PhotoUrl)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DoctorCode, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore())
            .ForMember(dest => dest.Leaves, opt => opt.Ignore());

        CreateMap<UpdateDoctorRequestDto, Doctor>()
            .IncludeBase<CreateDoctorRequestDto, Doctor>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<Doctor, DoctorResponseDto>();
        CreateMap<CreateDoctorScheduleRequestDto, DoctorSchedule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.Doctor, opt => opt.Ignore());
        CreateMap<DoctorSchedule, DoctorScheduleResponseDto>();

        CreateMap<CreateDoctorLeaveRequestDto, DoctorLeave>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => Normalize(src.Reason)))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.Doctor, opt => opt.Ignore());
        CreateMap<DoctorLeave, DoctorLeaveResponseDto>();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
