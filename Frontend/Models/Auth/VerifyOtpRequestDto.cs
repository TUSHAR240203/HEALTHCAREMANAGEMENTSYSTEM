namespace Frontend.Models.Auth
{
    public class VerifyOtpRequestDto
    {
        public int PatientId { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }
}