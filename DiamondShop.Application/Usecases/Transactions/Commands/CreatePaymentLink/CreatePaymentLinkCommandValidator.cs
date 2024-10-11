using FluentValidation;

namespace DiamondShop.Application.Usecases.Transactions.Commands.CreatePaymentLink
{
    public class CreatePaymentLinkCommandValidator : AbstractValidator<CreatePaymentLinkCommand>
    {
        public CreatePaymentLinkCommandValidator()
        {
            RuleFor(x => x.userId).NotEmpty();
            RuleFor(x => x.orderId).NotEmpty();
            RuleFor(x => x.paymentMethodName).NotEmpty();
            RuleFor(x => x.PaymentType).IsInEnum();
        }
    }
}
