using FluentValidation;
using Hms.AppointmentsApi.DTOs.Appointments;

namespace Hms.AppointmentsApi.Validators.Appointments;

public class CancelAppointmentRequestDtoValidator : AbstractValidator<CancelAppointmentRequestDto>
{
    public CancelAppointmentRequestDtoValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
