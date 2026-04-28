using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Frontend.Models.Patients
{
    public class CompletePatientProfileViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UHID { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsProfileCompleted { get; set; }
        public int CompletionPercentage { get; set; }

        [Display(Name = "Profile Photo URL")]
        [Url(ErrorMessage = "Enter a valid image URL.")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Upload Photo from Device")]
        public IFormFile? PhotoFile { get; set; }

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        [Display(Name = "Marital Status")]
        public string? MaritalStatus { get; set; }

        [Display(Name = "Address Line 1")]
        public string? AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Number")]
        public string? EmergencyContactNumber { get; set; }

        [Display(Name = "Emergency Contact Relation")]
        public string? EmergencyContactRelation { get; set; }

        [Display(Name = "Aadhaar Number")]
        public string? AadhaarNumber { get; set; }

        [Display(Name = "Insurance Provider")]
        public string? InsuranceProvider { get; set; }

        [Display(Name = "Insurance Policy Number")]
        public string? InsurancePolicyNumber { get; set; }
    }

    public class CompletePatientProfileRequestDto
    {
        public string? BloodGroup { get; set; }
        public string? MaritalStatus { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
