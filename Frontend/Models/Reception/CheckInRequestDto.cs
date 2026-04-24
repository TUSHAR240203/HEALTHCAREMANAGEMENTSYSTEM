namespace Frontend.Models.Reception
{
    public class CheckInRequestDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int DepartmentId { get; set; }
        public DateTime CheckInTimeUtc { get; set; }
    }
}
