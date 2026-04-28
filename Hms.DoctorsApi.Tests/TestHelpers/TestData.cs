using AutoMapper;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.Entities;
using Hms.DoctorsApi.Mapping;

namespace Hms.DoctorsApi.Tests.TestHelpers;

public static class TestData
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<DoctorMappingProfile>());
        config.AssertConfigurationIsValid();
        return config.CreateMapper();
    }

    public static CreateDoctorRequestDto CreateDoctorRequest() => new()
    {
        FullName = "Dr John Smith",
        Email = "john.smith@hospital.test",
        Phone = "9999999999",
        Gender = "Male",
        Qualification = "MBBS MD",
        Specialization = "Cardiology",
        DepartmentId = 10,
        DepartmentName = "Cardiology",
        ConsultationFee = 500,
        ExperienceYears = 12,
        LicenseNumber = " LIC-100 ",
        RoomNumber = "A-101",
        SupportsTeleConsultation = true,
        PhotoUrl = "https://example.test/doctor.jpg"
    };

    public static Doctor Doctor(int id = 1, bool isActive = true) => new()
    {
        Id = id,
        DoctorCode = "DOC-JOHN",
        FullName = "Dr John Smith",
        Specialization = "Cardiology",
        DepartmentId = 10,
        DepartmentName = "Cardiology",
        ConsultationFee = 500,
        ExperienceYears = 12,
        LicenseNumber = "LIC-100",
        IsActive = isActive,
        SupportsTeleConsultation = true,
        CreatedAtUtc = DateTime.UtcNow
    };

    public static DateOnly Next(DayOfWeek day)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(1);
        while (date.DayOfWeek != day) date = date.AddDays(1);
        return date;
    }
}
