namespace Hms.AuthApi.Entities;

public class PatientUserLink : BaseEntity
{
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public int UserId { get; set; }
    public bool PortalActivated { get; set; } = false;
    public DateTime? ActivatedAtUtc { get; set; }
}