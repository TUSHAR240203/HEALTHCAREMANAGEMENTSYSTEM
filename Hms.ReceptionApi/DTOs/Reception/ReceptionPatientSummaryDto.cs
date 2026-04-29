namespace Hms.ReceptionApi.DTOs.Reception;

public class ReceptionPatientSummaryDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public DateOnly DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
    public bool PortalAccessEnabled { get; set; }
    public bool PortalActivated { get; set; }
    public int Status { get; set; }
    public DateTime? LastVisitDateUtc { get; set; }
}