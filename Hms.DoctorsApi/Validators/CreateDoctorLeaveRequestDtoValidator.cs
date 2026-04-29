using FluentValidation;
using Hms.DoctorsApi.DTOs.Doctors;

namespace Hms.DoctorsApi.Validators;

public class CreateDoctorLeaveRequestDtoValidator : AbstractValidator<CreateDoctorLeaveRequestDto>
{
    public CreateDoctorLeaveRequestDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => x.EndDate >= x.StartDate)
            .WithMessage("End date must be greater than or equal to start date.");

        RuleFor(x => x.Reason)
            .MaximumLength(250);
    }
}
