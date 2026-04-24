namespace Hms.Web.Models.Auth
{
    public class CurrentUserResponseDto
    {
        public int UserId { get; set; }
        public int? PatientId { get; set; }
        public string? UHID { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}