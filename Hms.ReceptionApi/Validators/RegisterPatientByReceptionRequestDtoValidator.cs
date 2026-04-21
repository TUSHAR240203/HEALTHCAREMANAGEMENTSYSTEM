using FluentValidation;
using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Validators;

public class RegisterPatientByReceptionRequestDtoValidator : AbstractValidator<RegisterPatientByReceptionRequestDto>
{
    public RegisterPatientByReceptionRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required.")
            .Must(d => d < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.Gender)
            .InclusiveBetween(1, 3).WithMessage("Gender is invalid.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^[6-9]\d{9}$").WithMessage("Mobile number must be a valid 10-digit Indian mobile number.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email format is invalid.");

        RuleFor(x => x.BloodGroup)
            .MaximumLength(10).When(x => !string.IsNullOrWhiteSpace(x.BloodGroup));

        RuleFor(x => x.AddressLine1)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.AddressLine1));

        RuleFor(x => x.State)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{6}$").When(x => !string.IsNullOrWhiteSpace(x.PostalCode))
            .WithMessage("Postal code must be a valid 6-digit code.");

        RuleFor(x => x.EmergencyContactName)
            .MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactName));

        RuleFor(x => x.EmergencyContactNumber)
            .Matches(@"^[6-9]\d{9}$").When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactNumber))
            .WithMessage("Emergency contact number must be a valid 10-digit mobile number.");

        RuleFor(x => x.EmergencyContactRelation)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactRelation));

        RuleFor(x => x.InsuranceProvider)
            .MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.InsuranceProvider));

        RuleFor(x => x.InsurancePolicyNumber)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.InsurancePolicyNumber));
    }
}