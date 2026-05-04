using FluentValidation;
using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Validators
{
    public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceRequestDto>
    {
        public CreateInvoiceValidator()
        {
            RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("PatientId is required.");
            RuleFor(x => x.UHID).NotEmpty().WithMessage("UHID is required.");
        }
    }
}