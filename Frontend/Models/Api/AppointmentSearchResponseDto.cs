namespace Frontend.Models.Api;

public class AppointmentSearchResponseDto
{
    public int TotalCount { get; set; }
    public List<AppointmentResponseDto> Appointments { get; set; } = new();
}