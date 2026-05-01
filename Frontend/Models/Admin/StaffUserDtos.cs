using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Admin
{
    public class StaffUserResponseDto
    {
        public int UserId { get; set; }
        public string LoginId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public string[] Roles { get; set; } = [];
        public string Role => Roles.FirstOrDefault() ?? string.Empty;
        public bool IsPasswordLoginEnabled { get; set; }
        public bool IsOtpLoginEnabled { get; set; }
        public bool IsFirstLoginCompleted { get; set; }
    }

    public class CreateStaffUserViewModel
    {
        [Required, Display(Name = "Login ID")]
        public string LoginId { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Receptionist";

        [Required, StringLength(150), Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required, Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Password / JWT login")]
        public bool EnablePasswordLogin { get; set; } = true;

        [Display(Name = "OTP login")]
        public bool EnableOtpLogin { get; set; } = true;
    }

    public class UpdateUserStatusRequestDto
    {
        public bool IsActive { get; set; }
    }
}
