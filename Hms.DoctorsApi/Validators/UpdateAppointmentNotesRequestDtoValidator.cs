using FluentValidation;
using Hms.DoctorsApi.DTOs.Appointments;

namespace Hms.DoctorsApi.Validators;

public class UpdateAppointmentNotesRequestDtoValidator : AbstractValidator<UpdateAppointmentNotesRequestDto>
{
    public UpdateAppointmentNotesRequestDtoValidator()
    {
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(1000);
    }
}
