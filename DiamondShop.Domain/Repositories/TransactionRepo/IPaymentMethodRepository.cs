using DiamondShop.Domain.Models.Transactions.Entities;

namespace DiamondShop.Domain.Repositories.TransactionRepo
{
    public interface IPaymentMethodRepository : IBaseRepository<PaymentMethod>
    {
        Task<PaymentMethod?> GetByName(string nameNormalized, CancellationToken cancellationToken = default);
    }

}
