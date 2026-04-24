using FluentValidation;
using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Validators;

public class BookAppointmentRequestDtoValidator : AbstractValidator<BookAppointmentRequestDto>
{
    public BookAppointmentRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.DepartmentId).GreaterThan(0);

        RuleFor(x => x.VisitType)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.AppointmentDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Appointment date cannot be in the past.");

        RuleFor(x => x)
            .Must(x => x.SlotEndTime > x.SlotStartTime)
            .WithMessage("Slot end time must be greater than slot start time.");
    }
}