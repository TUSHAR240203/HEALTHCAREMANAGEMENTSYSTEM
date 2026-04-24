using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Api;

public class RescheduleAppointmentRequestDto
{
    [Required]
    [DataType(DataType.Date)]
    public DateOnly NewAppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [DataType(DataType.Time)]
    public TimeOnly NewSlotStartTime { get; set; } = new(10, 0);

    [Required]
    [DataType(DataType.Time)]
    public TimeOnly NewSlotEndTime { get; set; } = new(10, 30);

    public string? Reason { get; set; }
}