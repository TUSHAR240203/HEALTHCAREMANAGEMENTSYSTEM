using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Auth
{
    public class AuthPreferenceViewModel
    {
        public bool EnablePasswordLogin { get; set; }
        public bool EnableOtpLogin { get; set; } = true;

        [Display(Name = "Login ID")]
        public string? LoginId { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
