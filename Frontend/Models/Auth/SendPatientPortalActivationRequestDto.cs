namespace Frontend.Models.Auth
{
    public class SendPatientPortalActivationRequestDto
    {
        public int PatientId { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
    }
}