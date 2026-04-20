namespace Hms.ReceptionApi.DTOs.Reception;

public class QueueItemDto
{
    public int QueueTokenId { get; set; }
    public int TokenNumber { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public string PatientName { get; set; } = default!;
    public string Status { get; set; } = default!;
}