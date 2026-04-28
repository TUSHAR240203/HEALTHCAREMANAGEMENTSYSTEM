using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Frontend.Models.Doctors
{
    public class DoctorResponseDto
    {
        public int Id { get; set; }
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

    public class UpdateDoctorViewModel : CreateDoctorViewModel
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace Frontend.Models.Doctors
{
    public class DoctorLeaveResponseDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateOnly LeaveDate { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReviewedAtUtc { get; set; }
        public string? ReviewedBy { get; set; }
    }

    public class CreateDoctorLeaveViewModel
    {
        [Required, Range(1, int.MaxValue, ErrorMessage = "Enter the doctor profile ID."), Display(Name = "Doctor ID")]
        public int DoctorId { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Leave Date")]
        public DateOnly LeaveDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [StringLength(250)]
        public string? Reason { get; set; }
    }
}
