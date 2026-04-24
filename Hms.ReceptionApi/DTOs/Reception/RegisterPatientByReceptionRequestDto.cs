namespace Hms.ReceptionApi.DTOs.Reception;

public class RegisterPatientByReceptionRequestDto
{
<<<<<<< HEAD
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
=======
    public string FullName { get; set; } = default!;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public DateOnly DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string? BloodGroup { get; set; }

    public string? AddressLine1 { get; set; }
<<<<<<< HEAD
    public string? City { get; set; }
    public string? State { get; set; }
=======
    public int? StateId { get; set; }
    public string? State { get; set; }
    public int? CityId { get; set; }
    public string? City { get; set; }
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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