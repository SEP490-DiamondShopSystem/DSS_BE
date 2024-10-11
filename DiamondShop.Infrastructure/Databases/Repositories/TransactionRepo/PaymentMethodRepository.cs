using DiamondShop.Domain.Models.Transactions.Entities;
using DiamondShop.Domain.Repositories.TransactionRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Repositories.TransactionRepo
{
    internal class PaymentMethodRepository : BaseRepository<PaymentMethod>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(DiamondShopDbContext dbContext) : base(dbContext)
        {

        }

        public Task<PaymentMethod?> GetByName(string nameNormalized, CancellationToken cancellationToken = default)
        {
            return _set.Where(_set => _set.MethodName.ToUpper() == nameNormalized.ToUpper()).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
