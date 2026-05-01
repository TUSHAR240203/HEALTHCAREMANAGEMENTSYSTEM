using FluentValidation;
using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Validators
{
    public class AddInvoiceItemValidator : AbstractValidator<AddInvoiceItemRequestDto>
    {
        public AddInvoiceItemValidator()
        {
            RuleFor(x => x.ServiceId).GreaterThan(0).WithMessage("ServiceId must reference a valid catalog entry.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be at least 1.");
        }
    }
}