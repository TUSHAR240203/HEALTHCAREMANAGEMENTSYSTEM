using Hms.PatientsApi.Enums;

namespace Hms.PatientsApi.DTOs.Patients;

public class UpdatePatientRequestDto
{
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }

    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }

    public string? BloodGroup { get; set; }
    public string? MaritalStatus { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? EmergencyContactRelation { get; set; }

    public string? AadhaarNumber { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public bool PortalActivated { get; set; }

    public bool PortalAccessEnabled { get; set; }
    public PatientStatus Status { get; set; }
}
