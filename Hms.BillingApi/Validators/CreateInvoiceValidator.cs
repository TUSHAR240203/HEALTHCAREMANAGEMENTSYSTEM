using FluentValidation;
using Hms.BillingApi.DTOs;
using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Validators
{
    public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceRequestDto>
    {
        public CreateInvoiceValidator()
        {
            RuleFor(x => x.PatientId)
                .NotEmpty();

            RuleFor(x => x.Items)
                .NotEmpty();
        }
    }
}