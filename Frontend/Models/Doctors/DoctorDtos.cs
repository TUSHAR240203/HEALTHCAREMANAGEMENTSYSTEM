using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Frontend.Models.Doctors
{
    public class DoctorResponseDto
    {
        public int Id { get; set; }
        public int? AuthUserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public int ExperienceYears { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public bool SupportsTeleConsultation { get; set; }
        public bool IsActive { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class CreateDoctorViewModel
    {
        [Required, Display(Name = "Login ID")]
        public string LoginId { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public int? AuthUserId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Password Login")]
        public bool EnablePasswordLogin { get; set; } = true;

        [Display(Name = "OTP Login")]
        public bool EnableOtpLogin { get; set; } = true;

        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = "Male";

        [Required]
        public string Qualification { get; set; } = string.Empty;

        [Required]
        public string Specialization { get; set; } = string.Empty;

        [Display(Name = "Department ID")]
        public int DepartmentId { get; set; } = 1;

        [Required, Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "Consultation Fee")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Experience Years")]
        public int ExperienceYears { get; set; }

        [Required, Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        [Display(Name = "Tele-consultation")]
        public bool SupportsTeleConsultation { get; set; } = true;

        [Display(Name = "Photo URL")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Upload Photo")]
        public IFormFile? PhotoFile { get; set; }
    }

    public class UpdateDoctorViewModel
    {
        public int Id { get; set; }

        public int? AuthUserId { get; set; }

        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = "Male";

        [Required]
        public string Qualification { get; set; } = string.Empty;

        [Required]
        public string Specialization { get; set; } = string.Empty;

        [Display(Name = "Department ID")]
        public int DepartmentId { get; set; } = 1;

        [Required, Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "Consultation Fee")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Experience Years")]
        public int ExperienceYears { get; set; }

        [Required, Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        [Display(Name = "Tele-consultation")]
        public bool SupportsTeleConsultation { get; set; } = true;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Photo URL")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Upload Photo")]
        public IFormFile? PhotoFile { get; set; }
    }
}

namespace Frontend.Models.Doctors
{
    public class DoctorLeaveResponseDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReviewedAtUtc { get; set; }
        public string? ReviewedBy { get; set; }
    }

    public class CreateDoctorLeaveViewModel
    {
        [Required, DataType(DataType.Date), Display(Name = "Start Date")]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required, DataType(DataType.Date), Display(Name = "End Date")]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [StringLength(250)]
        public string? Reason { get; set; }
    }

    public class DoctorScheduleResponseDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public TimeOnly? BreakStartTime { get; set; }
        public TimeOnly? BreakEndTime { get; set; }

        public int SlotDurationMinutes { get; set; }
        public int? MaxPatientsPerDay { get; set; }

        public bool IsActive { get; set; }
    }

    public class CreateDoctorScheduleViewModel
    {
        [Required]
        [Display(Name = "Day of Week")]
        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeOnly StartTime { get; set; } = new(9, 0);

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeOnly EndTime { get; set; } = new(17, 0);

        [DataType(DataType.Time)]
        [Display(Name = "Break Start Time")]
        public TimeOnly? BreakStartTime { get; set; } = new(13, 0);

        [DataType(DataType.Time)]
        [Display(Name = "Break End Time")]
        public TimeOnly? BreakEndTime { get; set; } = new(14, 0);

        [Required]
        [Range(5, 180)]
        [Display(Name = "Slot Duration Minutes")]
        public int SlotDurationMinutes { get; set; } = 30;

        [Display(Name = "Max Patients Per Day")]
        public int? MaxPatientsPerDay { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class DoctorAvailabilityResponseDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string? Message { get; set; }
        public List<DoctorAvailabilitySlotDto> Slots { get; set; } = new();
    }

    public class DoctorAvailabilitySlotDto
    {
        public TimeOnly SlotStartTime { get; set; }
        public TimeOnly SlotEndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
