using FluentValidation;
using Hms.BillingApi.DTOs;
using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Validators
{
    public class PaymentValidator : AbstractValidator<PaymentRequestDto>
    {
        public PaymentValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.PaymentMethod).NotEmpty();
        }
    }
}