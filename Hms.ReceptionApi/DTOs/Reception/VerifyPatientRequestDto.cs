namespace Hms.ReceptionApi.DTOs.Reception;

public class VerifyPatientRequestDto
{
    public DateOnly? DateOfBirth { get; set; }
    public string? MobileNumber { get; set; }
}