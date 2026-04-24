namespace Hms.Web.Models.Auth
{
    public class PatientLoginRequestDto
    {
        public int PatientId { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}