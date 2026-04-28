using FluentValidation;
using Hms.AppointmentsApi.DTOs.Appointments;

namespace Hms.AppointmentsApi.Validators.Appointments;

public class AppointmentSearchRequestDtoValidator : AbstractValidator<AppointmentSearchRequestDto>
{
    public AppointmentSearchRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
