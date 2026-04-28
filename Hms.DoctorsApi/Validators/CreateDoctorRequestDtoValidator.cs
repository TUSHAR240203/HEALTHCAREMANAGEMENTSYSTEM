using FluentValidation;
using Hms.DoctorsApi.DTOs.Doctors;

namespace Hms.DoctorsApi.Validators;

public class CreateDoctorRequestDtoValidator : AbstractValidator<CreateDoctorRequestDto>
{
    public CreateDoctorRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Gender).MaximumLength(20);
        RuleFor(x => x.Qualification).MaximumLength(150);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.DepartmentName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExperienceYears).InclusiveBetween(0, 80);
        RuleFor(x => x.LicenseNumber).MaximumLength(50);
        RuleFor(x => x.RoomNumber).MaximumLength(20);
    }
}
