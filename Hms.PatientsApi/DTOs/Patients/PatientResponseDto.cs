<<<<<<< HEAD
﻿using Hms.PatientsApi.Enums;
=======
using Hms.PatientsApi.Enums;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

namespace Hms.PatientsApi.DTOs.Patients;

public class PatientResponseDto
{
    public int Id { get; set; }
<<<<<<< HEAD
=======
    public string PatientIdentifier { get; set; } = default!;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public string UHID { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string? BloodGroup { get; set; }
    public bool PortalAccessEnabled { get; set; }
    public bool PortalActivated { get; set; }
    public PatientStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
<<<<<<< HEAD
}
=======
    public bool IsProfileCompleted { get; set; }
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
