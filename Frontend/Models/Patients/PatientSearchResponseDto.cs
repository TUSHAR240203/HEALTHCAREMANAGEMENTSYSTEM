namespace Frontend.Models.Patients
{
    public class PatientSearchResponseDto
    {
        public List<PatientResponseDto> Patients { get; set; } = new();
    }
}