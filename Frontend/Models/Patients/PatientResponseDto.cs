namespace Hms.Web.Models.Patients
{
    public class PatientResponseDto
    {
        public int Id { get; set; }
        public string Uhid { get; set; } = string.Empty;
        public string FullName { get; set; } 
        public DateOnly DateOfBirth { get; set; }
        public int Gender { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? BloodGroup { get; set; }
        public bool PortalAccessEnabled { get; set; }
        public bool PortalActivated { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }

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