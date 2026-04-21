using FluentValidation;
using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Validators;

public class CheckInRequestDtoValidator : AbstractValidator<CheckInRequestDto>
{
    public CheckInRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.AppointmentId).GreaterThan(0);
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.CheckInTimeUtc).NotEmpty();
    }
}