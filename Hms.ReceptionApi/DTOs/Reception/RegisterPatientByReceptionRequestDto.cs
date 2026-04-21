namespace Hms.ReceptionApi.DTOs.Reception;

public class RegisterPatientByReceptionRequestDto
{
    public string FullName { get; set; } = default!;
    public DateOnly DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string? BloodGroup { get; set; }

    public string? AddressLine1 { get; set; }
    public int? StateId { get; set; }
    public string? State { get; set; }
    public int? CityId { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? EmergencyContactRelation { get; set; }

    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }

    public bool PortalAccessEnabled { get; set; }
    public bool SendPortalActivationSms { get; set; }
}