using FluentValidation;
using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Validators;

public class ReceptionPatientSearchRequestDtoValidator : AbstractValidator<ReceptionPatientSearchRequestDto>
{
    public ReceptionPatientSearchRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.MobileNumber)
            .Matches(@"^[6-9]\d{9}$")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber))
            .WithMessage("Mobile number must be valid.");
    }
}