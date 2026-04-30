namespace Frontend.Models.Reception
{
    public class CheckInPageViewModel
    {
        public DateOnly Date { get; set; }

        public CheckInRequestDto CheckIn { get; set; } = new();

        public List<TodayAppointmentForCheckInDto> Appointments { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }
}