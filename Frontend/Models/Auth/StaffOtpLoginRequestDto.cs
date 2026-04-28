namespace Frontend.Models.Auth
{
    public class StaffOtpLoginRequestDto
    {
        public string? LoginId { get; set; }
        public string? MobileNumber { get; set; }
        public string OtpCode { get; set; } = string.Empty;
    }
}
