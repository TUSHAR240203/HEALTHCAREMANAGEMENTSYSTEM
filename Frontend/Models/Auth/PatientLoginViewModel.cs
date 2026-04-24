using System.ComponentModel.DataAnnotations;

namespace Hms.Web.Models.Auth
{
    public class PatientLoginViewModel
    {
        [Required]
        [Display(Name = "Patient ID")]
        public int PatientId { get; set; }

        [Required]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Display(Name = "OTP")]
        public string OtpCode { get; set; } = string.Empty;

        public bool OtpSent { get; set; }
    }
}