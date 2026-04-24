using FluentValidation;
using Hms.DoctorsApi.DTOs.Doctors;

namespace Hms.DoctorsApi.Validators;

public class CreateDoctorLeaveRequestDtoValidator : AbstractValidator<CreateDoctorLeaveRequestDto>
{
    public CreateDoctorLeaveRequestDtoValidator()
    {
        RuleFor(x => x.LeaveDate).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(250);
    }
}
