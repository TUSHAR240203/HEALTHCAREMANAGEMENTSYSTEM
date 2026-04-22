namespace Hms.DoctorsApi.DTOs.Doctors;

public class UpdateDoctorRequestDto : CreateDoctorRequestDto
{
    public bool IsActive { get; set; }
}
