namespace Hms.DoctorsApi.Entities;

public class DoctorSchedule : BaseEntity
{
    public int DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeOnly? BreakStartTime { get; set; }
    public TimeOnly? BreakEndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public int? MaxPatientsPerDay { get; set; }
    public bool IsActive { get; set; } = true;

    public Doctor Doctor { get; set; } = default!;
}
