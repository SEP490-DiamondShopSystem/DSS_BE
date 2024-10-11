using DiamondShop.Application.Usecases.Transactions.Commands.AddManualPayments;
using FluentValidation;

namespace DiamondShop.Application.Usecases.Transactions.Commands.AddManual
{
    public class AddTransactionManuallyCommandValidator : AbstractValidator<AddTransactionManuallyCommand>
    {
        public AddTransactionManuallyCommandValidator()
        {
            RuleFor(x => x.description).NotNull();
            RuleFor(x => x.orderId).NotEmpty();
            RuleFor(x => x.PaymentType).IsInEnum();
        }
    }
}
