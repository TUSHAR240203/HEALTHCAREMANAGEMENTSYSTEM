namespace Frontend.Models.Patients
{
    public class PatientSearchRequestDto
    {
        public string? Query { get; set; }
        public string? UHID { get; set; }
        public string? MobileNumber { get; set; }
    }
}