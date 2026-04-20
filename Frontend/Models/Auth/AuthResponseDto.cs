namespace Hms.Web.Models.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}