namespace Hms.ReceptionApi.DTOs.Reception;

public class ReceptionPatientSearchRequestDto
{
    public string? UHID { get; set; }
    public string? MobileNumber { get; set; }
    public string? Name { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}