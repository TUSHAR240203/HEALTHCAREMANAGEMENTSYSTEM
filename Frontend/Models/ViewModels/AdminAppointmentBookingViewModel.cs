using System.ComponentModel.DataAnnotations;
using Frontend.Models.Doctors;

namespace Frontend.Models.ViewModels;

public class AdminAppointmentBookingViewModel
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public string UHID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a doctor.")]
    public int DoctorId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Please select an available slot.")]
    public string SelectedSlot { get; set; } = string.Empty;

    [Required]
    public string VisitType { get; set; } = "OPD";

    public string? ReasonForVisit { get; set; }
    public bool IsTeleConsultation { get; set; }

    public List<DoctorResponseDto> Doctors { get; set; } = new();
    public List<PatientSlotOption> Slots { get; set; } = new();
    public DoctorResponseDto? SelectedDoctor => Doctors.FirstOrDefault(d => d.Id == DoctorId);
}
