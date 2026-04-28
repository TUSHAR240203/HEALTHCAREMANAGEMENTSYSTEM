using FluentValidation;
using Hms.BillingApi.DTOs;
using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Validators
{
    public class AddInvoiceItemValidator : AbstractValidator<AddInvoiceItemRequestDto>
    {
        public AddInvoiceItemValidator()
        {
            RuleFor(x => x.ServiceName).NotEmpty();
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}