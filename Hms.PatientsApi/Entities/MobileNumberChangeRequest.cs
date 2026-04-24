namespace Hms.PatientsApi.Entities;

public class MobileNumberChangeRequest : BaseEntity
{
    public int PatientId { get; set; }
    public int ExistingOwnerPatientId { get; set; }
    public string NewMobileNumber { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsVerified { get; set; }
    public bool IsConsumed { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
}
