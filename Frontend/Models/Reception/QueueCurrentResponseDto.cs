namespace Frontend.Models.Reception
{
    public class QueueCurrentResponseDto
    {
        public int QueueTokenId { get; set; }
        public int TokenNumber { get; set; }
        public int PatientId { get; set; }
    public int AppointmentId { get; set; }
        public string? UHID { get; set; }
        public string? PatientName { get; set; }
        public string? Status { get; set; }
        public DateTime? CalledAtUtc { get; set; }
        public DateTime? StartedAtUtc { get; set; }
    }
}
