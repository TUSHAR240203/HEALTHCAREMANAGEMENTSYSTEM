namespace Frontend.Models.Reception
{
    public class QueueItemDto
    {
        public int QueueTokenId { get; set; }
        public int TokenNumber { get; set; }
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
