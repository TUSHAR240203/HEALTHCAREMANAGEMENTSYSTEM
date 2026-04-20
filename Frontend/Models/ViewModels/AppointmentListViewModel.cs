using Frontend.Models.Api;

namespace Frontend.Models.ViewModels;

public class AppointmentListViewModel
{
    public AppointmentSearchRequestDto Search { get; set; } = new();
    public AppointmentSearchResponseDto Result { get; set; } = new();
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}