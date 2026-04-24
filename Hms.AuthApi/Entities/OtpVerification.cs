namespace Hms.AuthApi.Entities;

public class OtpVerification : BaseEntity
{
    public int PatientId { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
    public string Purpose { get; set; } = default!; // PortalActivation / Login
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; } = false;
}