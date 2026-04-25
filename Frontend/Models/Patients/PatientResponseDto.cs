namespace Frontend.Models.Patients
{
    public class PatientResponseDto
    {
        public int Id { get; set; }
        public string PatientIdentifier { get; set; } = string.Empty;
        public string Uhid { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public int Gender { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? BloodGroup { get; set; }
        public bool PortalAccessEnabled { get; set; }
        public bool PortalActivated { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool IsProfileCompleted { get; set; }
        public string? PhotoUrl { get; set; }
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

        public string StatusText =>
            Status == 1 ? "Active" :
            Status == 2 ? "Inactive" :
            Status == 3 ? "Deleted" :
            "Unknown";

        public string GenderText =>
            Gender == 1 ? "Male" :
            Gender == 2 ? "Female" :
            Gender == 3 ? "Other" :
            "Unknown";
    }
}
