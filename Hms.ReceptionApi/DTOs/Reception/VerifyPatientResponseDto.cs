namespace Hms.ReceptionApi.DTOs.Reception;

public class VerifyPatientResponseDto
{
    public int PatientId { get; set; }
    public bool Verified { get; set; }
    public string Message { get; set; } = default!;
}