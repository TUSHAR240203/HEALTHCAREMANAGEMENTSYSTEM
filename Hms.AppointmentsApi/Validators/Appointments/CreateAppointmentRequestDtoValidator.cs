using FluentValidation;
using Hms.AppointmentsApi.DTOs.Appointments;

namespace Hms.AppointmentsApi.Validators.Appointments;

public class CreateAppointmentRequestDtoValidator : AbstractValidator<CreateAppointmentRequestDto>
{
    public CreateAppointmentRequestDtoValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.UHID).NotEmpty().MaximumLength(30);
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.DoctorName).MaximumLength(150);
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.DepartmentName).MaximumLength(150);
        RuleFor(x => x.VisitType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReasonForVisit).MaximumLength(500);
        RuleFor(x => x.AppointmentDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Appointment date cannot be in the past.");
        RuleFor(x => x.SlotEndTime)
            .GreaterThan(x => x.SlotStartTime)
            .WithMessage("SlotEndTime must be greater than SlotStartTime.");
    }
}
