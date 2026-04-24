namespace Hms.DoctorsApi.DTOs.Doctors;

public class CreateDoctorScheduleRequestDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeOnly? BreakStartTime { get; set; }
    public TimeOnly? BreakEndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public int? MaxPatientsPerDay { get; set; }
}
