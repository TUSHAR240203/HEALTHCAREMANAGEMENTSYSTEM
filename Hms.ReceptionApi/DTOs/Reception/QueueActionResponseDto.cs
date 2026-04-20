namespace Hms.ReceptionApi.DTOs.Reception;

public class QueueActionResponseDto
{
    public int QueueTokenId { get; set; }
    public int TokenNumber { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public string PatientName { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Message { get; set; } = default!;
}