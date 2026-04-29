
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Auth
{
    public class StaffLoginViewModel
    {
        [Display(Name = "Login ID")]
        public string LoginId { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Login ID or Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Display(Name = "OTP")]
        public string OtpCode { get; set; } = string.Empty;

        public bool OtpSent { get; set; }
    }
}
