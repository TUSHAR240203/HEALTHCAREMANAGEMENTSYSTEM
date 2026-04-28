using FluentValidation;
using Hms.AppointmentsApi.DTOs.Appointments;

namespace Hms.AppointmentsApi.Validators.Appointments;

public class RescheduleAppointmentRequestDtoValidator : AbstractValidator<RescheduleAppointmentRequestDto>
{
    public RescheduleAppointmentRequestDtoValidator()
    {
        RuleFor(x => x.NewAppointmentDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("New appointment date cannot be in the past.");
        RuleFor(x => x.NewSlotEndTime)
            .GreaterThan(x => x.NewSlotStartTime)
            .WithMessage("NewSlotEndTime must be greater than NewSlotStartTime.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
