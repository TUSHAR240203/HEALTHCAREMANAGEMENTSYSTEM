<<<<<<< HEAD
﻿using Hms.PatientsApi.Enums;
=======
using Hms.PatientsApi.Enums;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

namespace Hms.PatientsApi.Entities;

public class Patient : BaseEntity
{
<<<<<<< HEAD
=======
    public string PatientIdentifier { get; set; } = default!;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public string UHID { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
<<<<<<< HEAD
    public string FullName { get; set; } = default!;
=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

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
<<<<<<< HEAD
    public string? Country { get; set; }
=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public string? PostalCode { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? EmergencyContactRelation { get; set; }

    public string? AadhaarNumber { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }

    public bool PortalAccessEnabled { get; set; } = false;
    public bool PortalActivated { get; set; } = false;

    public PatientStatus Status { get; set; } = PatientStatus.Active;
<<<<<<< HEAD
}
=======
    public bool IsProfileCompleted { get; set; } = false;
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
