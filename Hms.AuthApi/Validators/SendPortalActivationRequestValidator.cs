using FluentValidation;
using Hms.AuthApi.DTOs.Auth;

namespace Hms.AuthApi.Validators;

public class SendPortalActivationRequestValidator
    : AbstractValidator<SendPatientPortalActivationRequestDto>
{
    public SendPortalActivationRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Valid patient id is required");

        RuleFor(x => x.MobileNumber)
            .NotEmpty()
            .WithMessage("Mobile number is required")
            .Matches(@"^[0-9]{10}$")
            .WithMessage("Mobile number must be exactly 10 digits");
    }
}