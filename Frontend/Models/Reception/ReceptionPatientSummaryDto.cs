namespace Frontend.Models.Reception
{
    public class ReceptionPatientSummaryDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }

        public string? UHID { get; set; }
        public string? FullName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}
