using FluentValidation;
using Hms.AppointmentsApi.DTOs.Appointments;

namespace Hms.AppointmentsApi.Validators.Appointments;

public class CompleteAppointmentRequestDtoValidator : AbstractValidator<CompleteAppointmentRequestDto>
{
    public CompleteAppointmentRequestDtoValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
