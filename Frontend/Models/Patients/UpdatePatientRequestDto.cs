using System.ComponentModel.DataAnnotations;

namespace Hms.Web.Models.Patients
{
    public class UpdatePatientRequestDto
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Of Birth")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public int Gender { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? BloodGroup { get; set; }
        public bool PortalAccessEnabled { get; set; }
        public bool PortalActivated { get; set; }
        public int Status { get; set; }
    }
}