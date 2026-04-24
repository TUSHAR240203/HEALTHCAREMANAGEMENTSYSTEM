using FluentValidation;
using Hms.DoctorsApi.DTOs.Doctors;

namespace Hms.DoctorsApi.Validators;

public class CreateDoctorScheduleRequestDtoValidator : AbstractValidator<CreateDoctorScheduleRequestDto>
{
    public CreateDoctorScheduleRequestDtoValidator()
    {
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage("EndTime must be greater than StartTime.");
        RuleFor(x => x.SlotDurationMinutes).InclusiveBetween(5, 240);
        RuleFor(x => x.MaxPatientsPerDay).GreaterThan(0).When(x => x.MaxPatientsPerDay.HasValue);
        RuleFor(x => x).Must(x => x.BreakStartTime.HasValue == x.BreakEndTime.HasValue)
            .WithMessage("BreakStartTime and BreakEndTime must be supplied together.");
        RuleFor(x => x).Must(x => !x.BreakStartTime.HasValue || x.BreakEndTime > x.BreakStartTime)
            .WithMessage("BreakEndTime must be greater than BreakStartTime.");
        RuleFor(x => x).Must(x => !x.BreakStartTime.HasValue || (x.BreakStartTime >= x.StartTime && x.BreakEndTime <= x.EndTime))
            .WithMessage("Break time must be inside the schedule time.");
    }
}
