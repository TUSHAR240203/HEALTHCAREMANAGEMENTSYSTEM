using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Entities;
using Hms.PatientsApi.Enums;

namespace Hms.PatientsApi.Tests.TestHelpers;

public static class MockData
{
    public static CreatePatientRequestDto CreateRequest()
    {
        return new CreatePatientRequestDto
        {
            FirstName = "Tushar",
            LastName = "Sharma",
            DateOfBirth = new DateOnly(2003, 2, 24),
            Gender = Gender.Male,
            MobileNumber = "9999999999",
            Email = "tushar@gmail.com",
            PortalAccessEnabled = true
        };
    }

    public static UpdatePatientRequestDto UpdateRequest()
    {
        return new UpdatePatientRequestDto
        {
            FirstName = "Tushar",
            LastName = "Sharma",
            DateOfBirth = new DateOnly(2003, 2, 24),
            Gender = Gender.Male,
            MobileNumber = "8888888888",
            Email = "updated@gmail.com",
            BloodGroup = "O+",
            PortalAccessEnabled = true,
            PortalActivated = true,
            Status = PatientStatus.Active
        };
    }

    public static Patient Patient()
    {
        return new Patient
        {
            Id = 1,
            UHID = "UHID001",
            PatientIdentifier = "PAT001",          // 🔥 CRITICAL FIX (required by EF)

            FirstName = "Tushar",
            LastName = "Sharma",
            DateOfBirth = new DateOnly(2003, 2, 24),
            Gender = Gender.Male,
            MobileNumber = "9999999999",
            Email = "tushar@gmail.com",
            BloodGroup = "B+",

            PortalAccessEnabled = true,
            PortalActivated = false,
            Status = PatientStatus.Active,

            CreatedAtUtc = DateTime.UtcNow         // ✅ good practice
        };
    }

    public static PatientResponseDto PatientResponse()
    {
        return new PatientResponseDto
        {
            Id = 1,
            UHID = "UHID001",
            FullName = "Tushar Sharma",
            DateOfBirth = new DateOnly(2003, 2, 24),
            Gender = Gender.Male,
            MobileNumber = "9999999999",
            Email = "tushar@gmail.com",
            BloodGroup = "B+",
            PortalAccessEnabled = true,
            PortalActivated = false,
            Status = PatientStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}