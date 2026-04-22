using System.ComponentModel.DataAnnotations;

namespace Hms.Web.Models.Patients
{
    public class PatientSearchViewModel
    {
        [Display(Name = "Search")]
        public string? Query { get; set; }

        [Display(Name = "UHID")]
        public string? UHID { get; set; }

        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        public List<PatientResponseDto> Results { get; set; } = new();
    }
}