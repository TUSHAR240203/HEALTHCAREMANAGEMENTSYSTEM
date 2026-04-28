using FluentValidation;
using Hms.PatientsApi.DTOs.Patients;

namespace Hms.PatientsApi.Validators;

public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequestDto>
{
    public CreatePatientRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.MiddleName));
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MobileNumber)
            .NotEmpty()
            .Matches(@"^[6-9]\d{9}$")
            .WithMessage("Mobile number must be a valid 10-digit Indian mobile number.");
        RuleFor(x => x.DateOfBirth).Must(x => x <= DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("DateOfBirth cannot be in the future.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    //    RuleFor(x => x.EmergencyContactNumber)
    //.NotEmpty()
    //.Matches(@"^[6-9]\d{9}$")
    //.WithMessage("Emergency contact number must be a valid 10-digit Indian mobile number.");
    }
}

public class UpdatePatientRequestValidator : AbstractValidator<UpdatePatientRequestDto>
{
    public UpdatePatientRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.MiddleName));
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MobileNumber)
            .NotEmpty()
            .Matches(@"^[6-9]\d{9}$")
            .WithMessage("Mobile number must be a valid 10-digit Indian mobile number.");
        RuleFor(x => x.DateOfBirth).Must(x => x <= DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("DateOfBirth cannot be in the future.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.EmergencyContactNumber)
    .NotEmpty()
    .Matches(@"^[6-9]\d{9}$")
    .WithMessage("Emergency contact number must be a valid 10-digit Indian mobile number.");
    }
}

public class RequestMobileNumberChangeOtpValidator : AbstractValidator<RequestMobileNumberChangeOtpDto>
{
    public RequestMobileNumberChangeOtpValidator()
    {
        RuleFor(x => x.MobileNumber).NotEmpty().Matches("^[0-9]{10}$").WithMessage("MobileNumber must be 10 digits.");
    }
}

public class VerifyMobileNumberChangeOtpValidator : AbstractValidator<VerifyMobileNumberChangeOtpDto>
{
    public VerifyMobileNumberChangeOtpValidator()
    {
        RuleFor(x => x.MobileNumber).NotEmpty().Matches("^[0-9]{10}$").WithMessage("MobileNumber must be 10 digits.");
        RuleFor(x => x.OtpCode).NotEmpty().Length(6);
    }
}
