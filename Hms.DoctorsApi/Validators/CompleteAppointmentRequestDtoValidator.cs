using FluentValidation;
using Hms.DoctorsApi.DTOs.Appointments;

namespace Hms.DoctorsApi.Validators;

public class CompleteAppointmentRequestDtoValidator : AbstractValidator<CompleteAppointmentRequestDto>
{
    public CompleteAppointmentRequestDtoValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
