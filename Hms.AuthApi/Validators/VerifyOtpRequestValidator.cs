using FluentValidation;
using Hms.AuthApi.DTOs.Auth;

namespace Hms.AuthApi.Validators;

public class VerifyOtpRequestValidator
    : AbstractValidator<VerifyOtpRequestDto>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Valid patient id is required");

        RuleFor(x => x.MobileNumber)
            .NotEmpty()
            .WithMessage("Mobile number is required")
            .Matches(@"^[0-9]{10}$")
            .WithMessage("Mobile number must be exactly 10 digits");

        RuleFor(x => x.OtpCode)
            .NotEmpty()
            .WithMessage("OTP is required")
            .Matches(@"^[0-9]{6}$")
            .WithMessage("OTP must be exactly 6 digits");

        RuleFor(x => x.Purpose)
            .NotEmpty()
            .WithMessage("Purpose is required");
    }
}