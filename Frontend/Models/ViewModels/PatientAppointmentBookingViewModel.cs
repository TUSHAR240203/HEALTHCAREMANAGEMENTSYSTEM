using System.ComponentModel.DataAnnotations;
using Frontend.Models.Doctors;

namespace Frontend.Models.ViewModels
{
    public class PatientAppointmentBookingViewModel
    {
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a doctor.")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Please choose an available time slot.")]
        public string SelectedSlot { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Visit Type")]
        public string VisitType { get; set; } = "OPD";

        [Display(Name = "Reason for Visit")]
        public string? ReasonForVisit { get; set; }

        [Display(Name = "Tele-consultation")]
        public bool IsTeleConsultation { get; set; }

        public List<DoctorResponseDto> Doctors { get; set; } = new();
        public List<PatientSlotOption> Slots { get; set; } = new();

        public DoctorResponseDto? SelectedDoctor => Doctors.FirstOrDefault(d => d.Id == DoctorId);
    }

    public class PatientSlotOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsBooked { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
    }
}
