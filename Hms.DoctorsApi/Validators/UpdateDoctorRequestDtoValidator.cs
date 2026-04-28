using FluentValidation;
using Hms.DoctorsApi.DTOs.Doctors;

namespace Hms.DoctorsApi.Validators;

public class UpdateDoctorRequestDtoValidator : AbstractValidator<UpdateDoctorRequestDto>
{
    public UpdateDoctorRequestDtoValidator()
    {
        Include(new CreateDoctorRequestDtoValidator());
    }
}
