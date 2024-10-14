using FluentValidation;

namespace DiamondShop.Application.Usecases.Transactions.Commands.AddManualRefunds
{
    public class AddManualRefundCommandValidator : AbstractValidator<AddManualRefundCommand>
    {
        public AddManualRefundCommandValidator()
        {
            RuleFor(x => x.orderId).NotEmpty();
            RuleFor(x => x.fineAmount).NotEmpty().GreaterThanOrEqualTo(0);
        }
    }
}
